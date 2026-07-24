using FluentAssertions;
using Moq;
using Timebash.Application.Extensions;
using Timebash.Application.Extensions.Requests;
using Timebash.Core.DTOs.Requests;
using Timebash.Core.DTOs.Shared;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;
using Timebash.Core.Extensions;
using Timebash.Tests.Unit.Application.Services.ActivityService.TestData;

namespace Timebash.Tests.Unit.Application.Services.ActivityService;

public class CreateWithCorrectionAsyncTests : ActivityServiceTestsBase
{
    [Theory]
    [ClassData(typeof(ValidCategoriesData))]
    [ClassData(typeof(CategoriesWithEmptyGuidsData))]
    [ClassData(typeof(CategoryDuplicateIdsData))]
    public async Task CreateWithCorrection_WithoutResolutionsAndAdditionalParts_ShouldReturnResponse(
        Guid userId,
        List<Guid> categoryIds,
        List<Category> categories)
    {
        var journal = new Journal(Guid.NewGuid(), userId, Faker.Lorem.Word());
        var clearedCategoryIds = GetClearedCategoryIds(categoryIds);
        var request = new ActivityWithCorrectionRequest(journal.Id, string.Empty, DateTime.MinValue, DateTime.MaxValue, categoryIds, [], []);
        var currentJournalUpdatedTime = journal.UpdatedAt;
        var capturedActivities = new List<Activity>();

        JournalAccessServiceMock.SetupEnsureAccess(journal);
        SetupActivityGetByIds();
        ActivityRepositoryMock.Setup(repository => repository.Add(It.IsAny<Activity>()))
            .Callback<Activity>(capturedActivities.Add);
        SetupCategoryGetByIds(clearedCategoryIds, categories);
        SetupAddCategoriesToActivity(clearedCategoryIds);
        UnitOfWorkMock.SetupSaveChanges();

        var result = await Service.CreateWithCorrectionAsync(request, userId, CancellationToken.None);

        capturedActivities.Should().HaveCount(1);

        var capturedActivity = capturedActivities.First();
        capturedActivity.Id.Should().NotBeEmpty();
        capturedActivity.Should().BeEquivalentTo(
            request.ToActivity(capturedActivity.Id),
            options => options
                .Excluding(activity => activity.CreatedAt)
                .Excluding(activity => activity.UpdatedAt));

        result.MainActivity.Should().BeEquivalentTo(capturedActivity.ToResponse());
        result.AdditionalActivities.Should().BeEmpty();
        journal.UpdatedAt.Should().BeAfter(currentJournalUpdatedTime);

        JournalAccessServiceMock.VerifyEnsureAccessCalled(journal.Id, journal.UserId);
        VerifyActivityGetByIdsCalled(new List<Guid>());
        VerifyCategoryGetByIdsCalled(clearedCategoryIds);
        VerifyAddCategoriesToActivityCalled(capturedActivity.Id, clearedCategoryIds);
        UnitOfWorkMock.VerifySaveChangesCalled();
    }

    [Fact]
    public async Task CreateWithCorrection_WithoutCategoriesAndResolutionsAndAdditionalParts_ShouldReturnResponse()
    {
        var journal = new Journal(Guid.NewGuid(), Guid.NewGuid(), Faker.Lorem.Word());
        var request = new ActivityWithCorrectionRequest(journal.Id, string.Empty, DateTime.MinValue, DateTime.MaxValue, [], [], []);
        var currentJournalUpdatedTime = journal.UpdatedAt;
        var capturedActivities = new List<Activity>();

        JournalAccessServiceMock.SetupEnsureAccess(journal);
        SetupActivityGetByIds();
        ActivityRepositoryMock.Setup(repository => repository.Add(It.IsAny<Activity>())).Callback<Activity>(capturedActivities.Add);
        UnitOfWorkMock.SetupSaveChanges();

        var result = await Service.CreateWithCorrectionAsync(request, journal.UserId, CancellationToken.None);

        capturedActivities.Should().HaveCount(1);

        var capturedActivity = capturedActivities.First();
        capturedActivity.Should().NotBeNull();
        capturedActivity.Id.Should().NotBeEmpty();
        capturedActivity.Should().BeEquivalentTo(
            request.ToActivity(capturedActivity.Id),
            options => options
                .Excluding(activity => activity.CreatedAt)
                .Excluding(activity => activity.UpdatedAt));

        result.MainActivity.Should().BeEquivalentTo(capturedActivity.ToResponse());
        result.AdditionalActivities.Should().BeEmpty();
        journal.UpdatedAt.Should().BeAfter(currentJournalUpdatedTime);

        JournalAccessServiceMock.VerifyEnsureAccessCalled(journal.Id, journal.UserId);
        VerifyActivityGetByIdsCalled(new List<Guid>());
        UnitOfWorkMock.VerifySaveChangesCalled();
    }

    [Fact]
    public async Task CreateWithCorrection_WithShiftCorrection_ShouldApplyShift()
    {
        var journal = new Journal(Guid.NewGuid(), Guid.NewGuid(), Faker.Lorem.Word());
        var activities = new List<Activity>
        {
            new(Guid.NewGuid(), journal.Id, DateTime.MinValue, DateTime.MaxValue),
            new(Guid.NewGuid(), journal.Id, DateTime.UtcNow, DateTime.MaxValue)
        };
        var activityDictionary = activities.ToDictionary(activity => activity.Id, activity => activity);

        var requestEndTime = DateTime.MaxValue.AddHours(-1);
        var resolutions = activities
            .Select(activity => new ActivityConflictResolution(activity.Id, new ShiftCorrection(requestEndTime, activity.EndTime)))
            .ToList();
        var request = new ActivityWithCorrectionRequest(journal.Id, string.Empty, DateTime.MinValue, requestEndTime, [], resolutions, []);

        var currentJournalUpdatedTime = journal.UpdatedAt;
        var currentCreateTimes = activities.ToDictionary(activity => activity.Id, activity => activity.CreatedAt);
        var currentUpdateTimes = activities.ToDictionary(activity => activity.Id, activity => activity.UpdatedAt);
        var expectedActivities = resolutions
            .Select(resolution =>
            {
                var activity = activityDictionary[resolution.ActivityId];
                var correction = (ShiftCorrection)resolution.Correction;
                return new Activity(activity.Id, activity.JournalId, correction.NewStartTime, correction.NewEndTime, activity.Name);
            })
            .ToList();
        var capturedActivities = new List<Activity>();

        JournalAccessServiceMock.SetupEnsureAccess(journal);
        SetupActivityGetByIds(resolutions, activities);
        ActivityRepositoryMock.Setup(repository => repository.Add(It.IsAny<Activity>())).Callback<Activity>(capturedActivities.Add);
        UnitOfWorkMock.SetupSaveChanges();

        var result = await Service.CreateWithCorrectionAsync(request, journal.UserId, CancellationToken.None);

        capturedActivities.Should().HaveCount(1);

        var capturedActivity = capturedActivities.First();
        capturedActivity.Should().NotBeNull();
        capturedActivity.Id.Should().NotBeEmpty();
        capturedActivity.Should().BeEquivalentTo(
            request.ToActivity(capturedActivity.Id),
            options => options
                .Excluding(activity => activity.CreatedAt)
                .Excluding(activity => activity.UpdatedAt));

        result.MainActivity.Should().BeEquivalentTo(capturedActivity.ToResponse());
        result.AdditionalActivities.Should().BeEmpty();

        activities.Should().BeEquivalentTo(expectedActivities, options => options
            .Excluding(activity => activity.UpdatedAt)
            .Excluding(activity => activity.CreatedAt)
            .WithoutStrictOrdering());

        foreach (var activity in activities)
        {
            activity.CreatedAt.Should().Be(currentCreateTimes[activity.Id]);
            activity.UpdatedAt.Should().BeAfter(currentUpdateTimes[activity.Id]);
        }

        journal.UpdatedAt.Should().BeAfter(currentJournalUpdatedTime);

        JournalAccessServiceMock.VerifyEnsureAccessCalled(journal.Id, journal.UserId);
        VerifyActivityGetByIdsCalled(resolutions);
        UnitOfWorkMock.VerifySaveChangesCalled();
    }

    [Fact]
    public async Task CreateWithCorrection_WithSplitCorrection_WithoutCategories_ShouldApplySplit()
    {
        var journal = new Journal(Guid.NewGuid(), Guid.NewGuid(), Faker.Lorem.Word());
        var activities = new List<Activity>
        {
            new(Guid.NewGuid(), journal.Id, DateTime.MinValue, DateTime.MaxValue, Faker.Lorem.Sentence()),
            new(Guid.NewGuid(), journal.Id, DateTime.MinValue, DateTime.MaxValue, Faker.Lorem.Sentence()),
        };
        var activityDictionary = activities.ToDictionary(activity => activity.Id, activity => activity);

        var requestStartTime = DateTime.UtcNow.TruncateToSecond();
        var requestEndTime = DateTime.MaxValue.TruncateToSecond();
        var resolutions = activities
            .Select(activity => new ActivityConflictResolution(
                activity.Id,
                new SplitCorrection(
                    [
                        new(activity.StartTime, requestStartTime),
                        new(requestEndTime, activity.EndTime)
                    ]
                )))
            .ToList();
        var resolutionActivityIds = resolutions.Select(resolution => resolution.ActivityId);
        var request = new ActivityWithCorrectionRequest(
            journal.Id,
            Faker.Lorem.Sentence(),
            requestStartTime,
            requestEndTime,
            [],
            resolutions,
            []);

        var currentJournalUpdatedTime = journal.UpdatedAt;
        var currentCreateTimes = activities.ToDictionary(activity => activity.Id, activity => activity.CreatedAt);
        var currentUpdateTimes = activities.ToDictionary(activity => activity.Id, activity => activity.UpdatedAt);
        var expectedUpdated = resolutions
            .Select(resolution =>
            {
                var activity = activityDictionary[resolution.ActivityId];
                var correction = (SplitCorrection)resolution.Correction;
                return new Activity(activity.Id, activity.JournalId, correction.Parts[0].StartTime, correction.Parts[0].EndTime, activity.Name);
            })
            .ToList();
        var expectedAdditional = resolutions
            .SelectMany(resolution =>
            {
                var activity = activityDictionary[resolution.ActivityId];
                var correction = (SplitCorrection)resolution.Correction;
                return correction.Parts
                    .Skip(1)
                    .Select(part => new Activity(Guid.NewGuid(), activity.JournalId, part.StartTime, part.EndTime, activity.Name));
            })
            .ToList();
        var capturedActivities = new List<Activity>();

        JournalAccessServiceMock.SetupEnsureAccess(journal);
        SetupActivityGetByIds(resolutions, activities);
        ActivityRepositoryMock.Setup(repository => repository.Add(It.IsAny<Activity>())).Callback<Activity>(capturedActivities.Add);
        SetupActivityGetCategoryIdsByActivityId(resolutionActivityIds, []);
        UnitOfWorkMock.SetupSaveChanges();

        var result = await Service.CreateWithCorrectionAsync(request, journal.UserId, CancellationToken.None);

        capturedActivities.Should().NotBeEmpty();

        var capturedMainActivity = capturedActivities.First(activity =>
            activity.JournalId == request.JournalId &&
            activity.Name == request.Name &&
            activity.StartTime == request.StartTime &&
            activity.EndTime == request.EndTime);
        capturedMainActivity.Should().NotBeNull();
        capturedMainActivity.Id.Should().NotBeEmpty();

        result.MainActivity.Should().BeEquivalentTo(capturedMainActivity.ToResponse());

        capturedActivities.Remove(capturedMainActivity);
        capturedActivities.Should().BeEquivalentTo(
            expectedAdditional,
            options => options
                .Excluding(activity => activity.Id)
                .Excluding(activity => activity.UpdatedAt)
                .Excluding(activity => activity.CreatedAt)
                .WithoutStrictOrdering());

        result.AdditionalActivities.Should().BeEquivalentTo(
            capturedActivities.Select(activity => activity.ToResponse()),
            options => options.WithoutStrictOrdering());

        activities.Should().BeEquivalentTo(expectedUpdated, options => options
            .Excluding(activity => activity.UpdatedAt)
            .Excluding(activity => activity.CreatedAt)
            .WithoutStrictOrdering());

        foreach (var activity in activities)
        {
            activity.CreatedAt.Should().Be(currentCreateTimes[activity.Id]);
            activity.UpdatedAt.Should().BeAfter(currentUpdateTimes[activity.Id]);
        }

        journal.UpdatedAt.Should().BeAfter(currentJournalUpdatedTime);

        JournalAccessServiceMock.VerifyEnsureAccessCalled(journal.Id, journal.UserId);
        VerifyActivityGetByIdsCalled(resolutions);
        resolutionActivityIds.ToList().ForEach(VerifyGetCategoryIdsByActivityIdCalled);
        UnitOfWorkMock.VerifySaveChangesCalled();
    }

    [Theory]
    [ClassData(typeof(ValidCategoriesData))]
    [ClassData(typeof(CategoriesWithEmptyGuidsData))]
    [ClassData(typeof(CategoryDuplicateIdsData))]
    public async Task CreateWithCorrection_WithSplitCorrection_WithCategories_ShouldApplySplit(
        Guid userId,
        List<Guid> categoryIds,
        List<Category> categories)
    {
        var journal = new Journal(Guid.NewGuid(), userId, Faker.Lorem.Word());
        var activities = new List<Activity>
        {
            new(Guid.NewGuid(), journal.Id, DateTime.MinValue, DateTime.MaxValue, Faker.Lorem.Sentence()),
            new( Guid.NewGuid(), journal.Id, DateTime.MinValue, DateTime.MaxValue, Faker.Lorem.Sentence()),
        };
        var activityDictionary = activities.ToDictionary(activity => activity.Id, activity => activity);
        var clearedCategoryIds = GetClearedCategoryIds(categoryIds);

        var requestStartTime = DateTime.UtcNow.TruncateToSecond();
        var requestEndTime = DateTime.MaxValue.TruncateToSecond();
        var resolutions = activities
            .Select(activity => new ActivityConflictResolution(
                activity.Id,
                new SplitCorrection(
                    [
                        new(activity.StartTime, requestStartTime),
                        new(requestEndTime, activity.EndTime)
                    ]
                )))
            .ToList();
        var resolutionActivityIds = resolutions.Select(resolution => resolution.ActivityId);
        var request = new ActivityWithCorrectionRequest(
            journal.Id,
            Faker.Lorem.Sentence(),
            requestStartTime,
            requestEndTime,
            categoryIds,
            resolutions,
            []);

        var currentJournalUpdatedTime = journal.UpdatedAt;
        var currentCreateTimes = activities.ToDictionary(activity => activity.Id, activity => activity.CreatedAt);
        var currentUpdateTimes = activities.ToDictionary(activity => activity.Id, activity => activity.UpdatedAt);
        var expectedUpdated = resolutions
            .Select(resolution =>
            {
                var activity = activityDictionary[resolution.ActivityId];
                var correction = (SplitCorrection)resolution.Correction;
                return new Activity(activity.Id, activity.JournalId, correction.Parts[0].StartTime, correction.Parts[0].EndTime, activity.Name);
            })
            .ToList();
        var expectedAdditional = resolutions
            .SelectMany(resolution =>
            {
                var activity = activityDictionary[resolution.ActivityId];
                var correction = (SplitCorrection)resolution.Correction;
                return correction.Parts
                    .Skip(1)
                    .Select(part => new Activity(Guid.NewGuid(), activity.JournalId, part.StartTime, part.EndTime, activity.Name));
            })
            .ToList();
        var capturedActivities = new List<Activity>();

        JournalAccessServiceMock.SetupEnsureAccess(journal);
        SetupActivityGetByIds(resolutionActivityIds, activities);
        ActivityRepositoryMock.Setup(repository => repository.Add(It.IsAny<Activity>())).Callback<Activity>(capturedActivities.Add);
        SetupActivityGetCategoryIdsByActivityId(resolutionActivityIds, clearedCategoryIds);
        SetupCategoryGetByIds(clearedCategoryIds, categories);
        SetupAddCategoriesToActivity(clearedCategoryIds);
        UnitOfWorkMock.SetupSaveChanges();

        var result = await Service.CreateWithCorrectionAsync(request, userId, CancellationToken.None);

        capturedActivities.Should().NotBeEmpty();

        var capturedMainActivity = capturedActivities.First(activity =>
            activity.JournalId == request.JournalId &&
            activity.Name == request.Name &&
            activity.StartTime == request.StartTime &&
            activity.EndTime == request.EndTime);
        capturedMainActivity.Should().NotBeNull();
        capturedMainActivity.Id.Should().NotBeEmpty();

        result.MainActivity.Should().BeEquivalentTo(capturedMainActivity.ToResponse());

        capturedActivities.Remove(capturedMainActivity);
        capturedActivities.Should().BeEquivalentTo(
            expectedAdditional,
            options => options
                .Excluding(activity => activity.Id)
                .Excluding(activity => activity.UpdatedAt)
                .Excluding(activity => activity.CreatedAt)
                .WithoutStrictOrdering());

        result.AdditionalActivities.Should().BeEquivalentTo(
            capturedActivities.Select(activity => activity.ToResponse()),
            options => options.WithoutStrictOrdering());

        activities.Should().BeEquivalentTo(expectedUpdated, options => options
            .Excluding(activity => activity.UpdatedAt)
            .Excluding(activity => activity.CreatedAt)
            .WithoutStrictOrdering());

        foreach (var activity in activities)
        {
            activity.CreatedAt.Should().Be(currentCreateTimes[activity.Id]);
            activity.UpdatedAt.Should().BeAfter(currentUpdateTimes[activity.Id]);
        }

        journal.UpdatedAt.Should().BeAfter(currentJournalUpdatedTime);

        JournalAccessServiceMock.VerifyEnsureAccessCalled(journal.Id, journal.UserId);
        VerifyActivityGetByIdsCalled(resolutionActivityIds);
        resolutionActivityIds.ToList().ForEach(VerifyGetCategoryIdsByActivityIdCalled);
        VerifyCategoryGetByIdsCalled(clearedCategoryIds);
        capturedActivities.ForEach(activity => VerifyAddCategoriesToActivityCalled(activity.Id, clearedCategoryIds));
        ActivityRepositoryMock.Verify(
            repository => repository.AddCategoriesToActivity(It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>>()),
            Times.Exactly(capturedActivities.Count + 1));
        UnitOfWorkMock.VerifySaveChangesCalled();
    }

    [Fact]
    public async Task CreateWithCorrection_WithDeleteCorrection_ShouldApplyDelete()
    {
        var journal = new Journal(Guid.NewGuid(), Guid.NewGuid(), Faker.Lorem.Word());
        var activities = new List<Activity>
        {
            new(Guid.NewGuid(), journal.Id, DateTime.MinValue, DateTime.MaxValue),
            new(Guid.NewGuid(), journal.Id, DateTime.MinValue, DateTime.MaxValue)
        };
        var resolutions = activities
            .Select(activity => new ActivityConflictResolution(activity.Id, new DeleteCorrection()))
            .ToList();
        var request = new ActivityWithCorrectionRequest(journal.Id, string.Empty, DateTime.MinValue, DateTime.MaxValue, [], resolutions, []);
        var currentJournalUpdatedTime = journal.UpdatedAt;
        var capturedActivities = new List<Activity>();

        JournalAccessServiceMock.SetupEnsureAccess(journal);
        SetupActivityGetByIds(resolutions, activities);
        ActivityRepositoryMock.Setup(repository => repository.Add(It.IsAny<Activity>())).Callback<Activity>(capturedActivities.Add);
        activities.ForEach(SetupActivityDelete);
        UnitOfWorkMock.SetupSaveChanges();

        var result = await Service.CreateWithCorrectionAsync(request, journal.UserId, CancellationToken.None);

        capturedActivities.Should().HaveCount(1);

        var capturedActivity = capturedActivities.First();
        capturedActivity.Should().NotBeNull();
        capturedActivity.Id.Should().NotBeEmpty();
        capturedActivity.Should().BeEquivalentTo(
            request.ToActivity(capturedActivity.Id),
            options => options
                .Excluding(activity => activity.CreatedAt)
                .Excluding(activity => activity.UpdatedAt)
                .WithoutStrictOrdering());

        result.MainActivity.Should().BeEquivalentTo(capturedActivity.ToResponse());
        result.AdditionalActivities.Should().BeEmpty();
        journal.UpdatedAt.Should().BeAfter(currentJournalUpdatedTime);

        JournalAccessServiceMock.VerifyEnsureAccessCalled(journal.Id, journal.UserId);
        VerifyActivityGetByIdsCalled(resolutions);
        activities.ForEach(VerifyActivityDeleteCalled);
        UnitOfWorkMock.VerifySaveChangesCalled();
    }

    [Fact]
    public async Task CreateWithCorrection_WithAdditionalParts_WithoutCategories_ShouldCreateAdditional()
    {
        var journal = new Journal(Guid.NewGuid(), Guid.NewGuid(), Faker.Lorem.Word());
        var parts = new List<TimeInterval>
        {
            new(DateTime.UtcNow, DateTime.UtcNow.AddHours(1)),
            new(DateTime.UtcNow.AddHours(1), DateTime.MaxValue),
        };
        var request = new ActivityWithCorrectionRequest(
            journal.Id,
            Faker.Lorem.Sentence(),
            DateTime.MinValue.TruncateToSecond(),
            DateTime.UtcNow.TruncateToSecond(),
            [],
            [],
            parts);

        var currentJournalUpdatedTime = journal.UpdatedAt;
        var expectedAdditional = parts
            .Select(part => new Activity(Guid.NewGuid(), request.JournalId, part.StartTime, part.EndTime, request.Name))
            .ToList();
        var capturedActivities = new List<Activity>();

        JournalAccessServiceMock.SetupEnsureAccess(journal);
        SetupActivityGetByIds();
        ActivityRepositoryMock.Setup(repository => repository.Add(It.IsAny<Activity>())).Callback<Activity>(capturedActivities.Add);
        UnitOfWorkMock.SetupSaveChanges();

        var result = await Service.CreateWithCorrectionAsync(request, journal.UserId, CancellationToken.None);

        capturedActivities.Should().NotBeEmpty();

        var capturedMainActivity = capturedActivities.First(activity =>
            activity.JournalId == request.JournalId &&
            activity.Name == request.Name &&
            activity.StartTime == request.StartTime &&
            activity.EndTime == request.EndTime);
        capturedMainActivity.Should().NotBeNull();
        capturedMainActivity.Id.Should().NotBeEmpty();

        result.MainActivity.Should().BeEquivalentTo(capturedMainActivity.ToResponse());

        capturedActivities.Remove(capturedMainActivity);
        capturedActivities.Should().BeEquivalentTo(
            expectedAdditional,
            options => options
                .Excluding(activity => activity.Id)
                .Excluding(activity => activity.UpdatedAt)
                .Excluding(activity => activity.CreatedAt)
                .WithoutStrictOrdering());

        result.AdditionalActivities.Should().BeEquivalentTo(
            capturedActivities.Select(activity => activity.ToResponse()),
            options => options.WithoutStrictOrdering());
        journal.UpdatedAt.Should().BeAfter(currentJournalUpdatedTime);

        JournalAccessServiceMock.VerifyEnsureAccessCalled(journal.Id, journal.UserId);
        VerifyActivityGetByIdsCalled(new List<Guid>());
        UnitOfWorkMock.VerifySaveChangesCalled();
    }

    [Theory]
    [ClassData(typeof(ValidCategoriesData))]
    [ClassData(typeof(CategoriesWithEmptyGuidsData))]
    [ClassData(typeof(CategoryDuplicateIdsData))]
    public async Task CreateWithCorrection_WithAdditionalParts_WithCategories_ShouldCreateAdditional(
        Guid userId,
        List<Guid> categoryIds,
        List<Category> categories)
    {
        var journal = new Journal(Guid.NewGuid(), userId, Faker.Lorem.Word());
        var parts = new List<TimeInterval>
        {
            new(DateTime.UtcNow, DateTime.UtcNow.AddHours(1)),
            new(DateTime.UtcNow.AddHours(1), DateTime.MaxValue),
        };
        var clearedCategoryIds = GetClearedCategoryIds(categoryIds);
        var request = new ActivityWithCorrectionRequest(
            journal.Id,
            Faker.Lorem.Sentence(),
            DateTime.MinValue.TruncateToSecond(),
            DateTime.UtcNow.TruncateToSecond(),
            categoryIds,
            [],
            parts);

        var currentJournalUpdatedTime = journal.UpdatedAt;
        var expectedAdditional = parts
            .Select(part => new Activity(Guid.NewGuid(), request.JournalId, part.StartTime, part.EndTime, request.Name))
            .ToList();
        var capturedActivities = new List<Activity>();

        JournalAccessServiceMock.SetupEnsureAccess(journal);
        SetupActivityGetByIds();
        ActivityRepositoryMock.Setup(repository => repository.Add(It.IsAny<Activity>())).Callback<Activity>(capturedActivities.Add);
        SetupCategoryGetByIds(clearedCategoryIds, categories);
        SetupAddCategoriesToActivity(clearedCategoryIds);
        UnitOfWorkMock.SetupSaveChanges();

        var result = await Service.CreateWithCorrectionAsync(request, userId, CancellationToken.None);

        capturedActivities.Should().NotBeEmpty();

        var capturedMainActivity = capturedActivities.First(activity =>
            activity.JournalId == request.JournalId &&
            activity.Name == request.Name &&
            activity.StartTime == request.StartTime &&
            activity.EndTime == request.EndTime);
        capturedMainActivity.Should().NotBeNull();
        capturedMainActivity.Id.Should().NotBeEmpty();

        result.MainActivity.Should().BeEquivalentTo(capturedMainActivity.ToResponse());

        capturedActivities.Remove(capturedMainActivity);
        capturedActivities.Should().BeEquivalentTo(
            expectedAdditional,
            options => options
                .Excluding(activity => activity.Id)
                .Excluding(activity => activity.UpdatedAt)
                .Excluding(activity => activity.CreatedAt)
                .WithoutStrictOrdering());

        result.AdditionalActivities.Should().BeEquivalentTo(
            capturedActivities.Select(activity => activity.ToResponse()),
            options => options.WithoutStrictOrdering());
        journal.UpdatedAt.Should().BeAfter(currentJournalUpdatedTime);

        JournalAccessServiceMock.VerifyEnsureAccessCalled(journal.Id, journal.UserId);
        VerifyActivityGetByIdsCalled(new List<Guid>());
        VerifyCategoryGetByIdsCalled(clearedCategoryIds);
        VerifyAddCategoriesToActivityCalled(capturedMainActivity.Id, clearedCategoryIds);
        capturedActivities.ForEach(activity => VerifyAddCategoriesToActivityCalled(activity.Id, clearedCategoryIds));
        UnitOfWorkMock.VerifySaveChangesCalled();
    }

    [Fact]
    public async Task CreateWithCorrection_WithMixedResolutionsAndParts_ShouldReturnResponse()
    {
        var journal = new Journal(Guid.NewGuid(), Guid.NewGuid(), Faker.Lorem.Word());
        var activityToShift = new Activity(Guid.NewGuid(), journal.Id, DateTime.MinValue, DateTime.MaxValue);
        var activityToSplit = new Activity(Guid.NewGuid(), journal.Id, DateTime.MinValue, DateTime.MaxValue, Faker.Lorem.Sentence());
        var activityToDelete = new Activity(Guid.NewGuid(), journal.Id, DateTime.MinValue, DateTime.MaxValue);
        var updatedActivities = new List<Activity> { activityToShift, activityToSplit };

        var resolutions = new List<ActivityConflictResolution>
        {
            new(activityToShift.Id, new ShiftCorrection(DateTime.UtcNow.AddHours(2), DateTime.UtcNow.AddHours(2))),
            new(activityToSplit.Id, new SplitCorrection([
                new (DateTime.UtcNow.AddHours(1), DateTime.UtcNow.AddHours(2)),
                new (DateTime.UtcNow.AddHours(2), DateTime.MaxValue)
            ])),
            new(activityToDelete.Id, new DeleteCorrection())
        };
        var resolutionActivityIds = resolutions.Select(resolution => resolution.ActivityId);
        var parts = new List<TimeInterval>
        {
            new(DateTime.UtcNow, DateTime.UtcNow.AddHours(1)),
        };
        var request = new ActivityWithCorrectionRequest(
            journal.Id,
            Faker.Lorem.Sentence(),
            DateTime.MinValue.TruncateToSecond(),
            DateTime.UtcNow.TruncateToSecond(),
            [],
            resolutions,
            parts);

        var currentJournalUpdatedTime = journal.UpdatedAt;
        var currentCreateTimes = updatedActivities.ToDictionary(activity => activity.Id, activity => activity.CreatedAt);
        var currentUpdateTimes = updatedActivities.ToDictionary(activity => activity.Id, activity => activity.UpdatedAt);
        var expectedUpdated = new List<Activity>
        {
            new(
                activityToShift.Id,
                activityToShift.JournalId,
                ((ShiftCorrection)resolutions[0].Correction).NewStartTime,
                ((ShiftCorrection)resolutions[0].Correction).NewEndTime,
                activityToShift.Name),
            new(
                activityToSplit.Id,
                activityToSplit.JournalId,
                ((SplitCorrection)resolutions[1].Correction).Parts[0].StartTime,
                ((SplitCorrection)resolutions[1].Correction).Parts[0].EndTime,
                activityToSplit.Name),
        };
        var expectedAdditional = new List<Activity>
        {
            new(
                Guid.NewGuid(),
                activityToSplit.JournalId,
                ((SplitCorrection)resolutions[1].Correction).Parts[1].StartTime,
                ((SplitCorrection)resolutions[1].Correction).Parts[1].EndTime,
                activityToSplit.Name),
            new(
                Guid.NewGuid(),
                request.JournalId,
                request.AdditionalPartIntervals[0].StartTime,
                request.AdditionalPartIntervals[0].EndTime,
                request.Name)
        };
        var capturedActivities = new List<Activity>();

        JournalAccessServiceMock.SetupEnsureAccess(journal);
        SetupActivityGetByIds(resolutionActivityIds, [activityToDelete, activityToSplit, activityToShift]);
        ActivityRepositoryMock.Setup(repository => repository.Add(It.IsAny<Activity>())).Callback<Activity>(capturedActivities.Add);
        SetupGetCategoryIdsByActivityId(resolutions[1].ActivityId, []);
        SetupAddCategoriesToActivity([]);
        SetupActivityDelete(activityToDelete);
        UnitOfWorkMock.SetupSaveChanges();

        var result = await Service.CreateWithCorrectionAsync(request, journal.UserId, CancellationToken.None);

        capturedActivities.Should().NotBeEmpty();

        var capturedMainActivity = capturedActivities.First(activity =>
            activity.JournalId == request.JournalId &&
            activity.Name == request.Name &&
            activity.StartTime == request.StartTime &&
            activity.EndTime == request.EndTime);
        capturedMainActivity.Should().NotBeNull();
        capturedMainActivity.Id.Should().NotBeEmpty();

        result.MainActivity.Should().BeEquivalentTo(capturedMainActivity.ToResponse());

        capturedActivities.Remove(capturedMainActivity);
        capturedActivities.Should().BeEquivalentTo(
            expectedAdditional,
            options => options
                .Excluding(activity => activity.Id)
                .Excluding(activity => activity.UpdatedAt)
                .Excluding(activity => activity.CreatedAt)
                .WithoutStrictOrdering());

        result.AdditionalActivities.Should().BeEquivalentTo(
            capturedActivities.Select(activity => activity.ToResponse()),
            options => options.WithoutStrictOrdering());

        updatedActivities.Should().BeEquivalentTo(expectedUpdated, options => options
            .Excluding(activity => activity.UpdatedAt)
            .Excluding(activity => activity.CreatedAt)
            .WithoutStrictOrdering());

        foreach (var activity in updatedActivities)
        {
            activity.CreatedAt.Should().Be(currentCreateTimes[activity.Id]);
            activity.UpdatedAt.Should().BeAfter(currentUpdateTimes[activity.Id]);
        }

        journal.UpdatedAt.Should().BeAfter(currentJournalUpdatedTime);

        JournalAccessServiceMock.VerifyEnsureAccessCalled(journal.Id, journal.UserId);
        VerifyGetCategoryIdsByActivityIdCalled(resolutions[1].ActivityId);
        VerifyActivityGetByIdsCalled(resolutionActivityIds);
        VerifyActivityDeleteCalled(activityToDelete);
        UnitOfWorkMock.VerifySaveChangesCalled();
    }

    [Fact]
    public async Task CreateWithCorrection_EmptyJournalId_ShouldThrowBadRequest()
    {
        var journalId = Guid.Empty;
        var userId = Guid.NewGuid();

        JournalAccessServiceMock.SetupEnsureAccessThrowsBadRequest(journalId, userId);

        await FluentActions
            .Awaiting(() => Service.CreateWithCorrectionAsync(
                new(journalId, string.Empty, DateTime.MinValue, DateTime.MaxValue, [], [], []),
                userId,
                CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        JournalAccessServiceMock.VerifyEnsureAccessCalled(journalId, userId);
    }

    [Fact]
    public async Task CreateWithCorrection_JournalNotFound_ShouldThrowNotFound()
    {
        var journalId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        JournalAccessServiceMock.SetupEnsureAccessThrowsNotFound(journalId, userId);

        await FluentActions
            .Awaiting(() => Service.CreateWithCorrectionAsync(
                new(journalId, string.Empty, DateTime.MinValue, DateTime.MaxValue, [], [], []),
                userId,
                CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        JournalAccessServiceMock.VerifyEnsureAccessCalled(journalId, userId);
    }

    [Theory]
    [ClassData(typeof(ValidCategoriesData))]
    public async Task CreateWithCorrection_CategoryNotFound_ShouldThrowBadRequest(
        Guid userId,
        List<Guid> categoryIds,
        List<Category> categories)
    {
        var journal = new Journal(Guid.NewGuid(), userId, Faker.Lorem.Word());
        var clearedCategoryIds = GetClearedCategoryIds(categoryIds);
        var request = new ActivityWithCorrectionRequest(journal.Id, string.Empty, DateTime.MinValue, DateTime.MaxValue, categoryIds, [], []);

        JournalAccessServiceMock.SetupEnsureAccess(journal);
        SetupActivityGetByIds();
        SetupCategoryGetByIds(clearedCategoryIds, categories.Skip(1));

        await FluentActions
            .Awaiting(() => Service.CreateWithCorrectionAsync(request, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        JournalAccessServiceMock.VerifyEnsureAccessCalled(journal.Id, userId);
        VerifyActivityGetByIdsCalled(new List<Guid>());
        VerifyCategoryGetByIdsCalled(clearedCategoryIds);
    }

    [Theory]
    [ClassData(typeof(ValidCategoriesData))]
    public async Task CreateWithCorrection_CategoryNotLinkedToUser_ShouldThrowNotFound(
        Guid userId,
        List<Guid> categoryIds,
        List<Category> categories)
    {
        var journal = new Journal(Guid.NewGuid(), userId, Faker.Lorem.Word());
        var clearedCategoryIds = GetClearedCategoryIds(categoryIds);
        categories[0] = new Category(categories[0].Id, Guid.NewGuid(), Faker.Lorem.Word(), "#000000");
        var request = new ActivityWithCorrectionRequest(journal.Id, string.Empty, DateTime.MinValue, DateTime.MaxValue, categoryIds, [], []);

        JournalAccessServiceMock.SetupEnsureAccess(journal);
        SetupActivityGetByIds();
        SetupCategoryGetByIds(clearedCategoryIds, categories);

        await FluentActions
            .Awaiting(() => Service.CreateWithCorrectionAsync(request, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        JournalAccessServiceMock.VerifyEnsureAccessCalled(journal.Id, userId);
        VerifyActivityGetByIdsCalled(new List<Guid>());
        VerifyCategoryGetByIdsCalled(clearedCategoryIds);
    }

    [Fact]
    public async Task CreateWithCorrection_ActivityNotFound_ShouldThrowNotFound()
    {
        var journal = new Journal(Guid.NewGuid(), Guid.NewGuid(), Faker.Lorem.Word());
        var activities = new List<Activity>
        {
            new(Guid.NewGuid(), journal.Id, DateTime.MinValue, DateTime.MaxValue),
            new(Guid.NewGuid(), journal.Id, DateTime.UtcNow, DateTime.MaxValue)
        };

        var requestEndTime = DateTime.MaxValue.AddHours(-1);
        var resolutions = activities
            .Select(activity => new ActivityConflictResolution(activity.Id, new ShiftCorrection(requestEndTime, activity.EndTime)))
            .ToList();
        var request = new ActivityWithCorrectionRequest(journal.Id, string.Empty, DateTime.MinValue, requestEndTime, [], resolutions, []);

        JournalAccessServiceMock.SetupEnsureAccess(journal);
        SetupActivityGetByIds(activities.Skip(1));

        await FluentActions
            .Awaiting(() => Service.CreateWithCorrectionAsync(request, journal.UserId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        JournalAccessServiceMock.VerifyEnsureAccessCalled(journal.Id, journal.UserId);
        VerifyActivityGetByIdsCalled();
    }

    [Fact]
    public async Task CreateWithCorrection_ActivityNotLinkedToJournal_ShouldThrowNotFound()
    {
        var journal = new Journal(Guid.NewGuid(), Guid.NewGuid(), Faker.Lorem.Word());
        var activities = new List<Activity>
        {
            new(Guid.NewGuid(), journal.Id, DateTime.MinValue, DateTime.MaxValue),
            new(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, DateTime.MaxValue)
        };

        var requestEndTime = DateTime.MaxValue.AddHours(-1);
        var resolutions = activities
            .Select(activity => new ActivityConflictResolution(activity.Id, new ShiftCorrection(requestEndTime, activity.EndTime)))
            .ToList();
        var request = new ActivityWithCorrectionRequest(journal.Id, string.Empty, DateTime.MinValue, requestEndTime, [], resolutions, []);

        JournalAccessServiceMock.SetupEnsureAccess(journal);
        SetupActivityGetByIds(activities.Skip(1));

        await FluentActions
            .Awaiting(() => Service.CreateWithCorrectionAsync(request, journal.UserId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        JournalAccessServiceMock.VerifyEnsureAccessCalled(journal.Id, journal.UserId);
        VerifyActivityGetByIdsCalled();
    }

    private void SetupActivityGetByIds()
        => ActivityRepositoryMock.Setup(repository => repository.GetByIdsAsync(new List<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);

    private void SetupActivityGetByIds(IEnumerable<Activity> activities)
        => ActivityRepositoryMock
            .Setup(repository => repository.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(activities);

    private void SetupActivityGetByIds(IEnumerable<Guid> resolutionActivityIds, IEnumerable<Activity> activities)
        => ActivityRepositoryMock
            .Setup(repository => repository.GetByIdsAsync(
                It.Is<IEnumerable<Guid>>(ids => ids.OrderBy(id => id).SequenceEqual(resolutionActivityIds.OrderBy(id => id))),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(activities);

    private void SetupActivityGetByIds(List<ActivityConflictResolution> resolutions, IEnumerable<Activity> activities)
        => ActivityRepositoryMock
            .Setup(repository => repository.GetByIdsAsync(
                It.Is<IEnumerable<Guid>>(ids =>
                    ids.OrderBy(id => id).SequenceEqual(resolutions.Select(resolution => resolution.ActivityId).OrderBy(id => id))),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(activities);

    private void SetupActivityGetCategoryIdsByActivityId(IEnumerable<Guid> ids, IEnumerable<Guid> categoryIds)
        => ActivityRepositoryMock
            .Setup(repository => repository.GetCategoryIdsByActivityIdAsync(It.Is<Guid>(id => ids.Contains(id)), It.IsAny<CancellationToken>()))
            .ReturnsAsync(categoryIds);

    private void VerifyActivityGetByIdsCalled()
        => ActivityRepositoryMock.Verify(
            repository => repository.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()),
            Times.Once);

    private void VerifyActivityGetByIdsCalled(IEnumerable<Guid> resolutionActivityIds)
        => ActivityRepositoryMock.Verify(
            repository => repository.GetByIdsAsync(
                It.Is<IEnumerable<Guid>>(ids => ids.OrderBy(id => id).SequenceEqual(resolutionActivityIds.OrderBy(id => id))),
                It.IsAny<CancellationToken>()),
            Times.Once);

    private void VerifyActivityGetByIdsCalled(List<ActivityConflictResolution> resolutions)
        => ActivityRepositoryMock.Verify(
            repository => repository.GetByIdsAsync(
                It.Is<IEnumerable<Guid>>(ids => ids
                    .OrderBy(id => id)
                    .SequenceEqual(resolutions.Select(resolution => resolution.ActivityId).OrderBy(id => id))),
                It.IsAny<CancellationToken>()),
            Times.Once);
}
