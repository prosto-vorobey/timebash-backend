using Bogus;
using FluentAssertions;
using Moq;
using Timebash.Core.DTOs.Responses;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;
using Timebash.Core.Repositories;
using Timebash.Core.Services;
using Timebash.Tests.Unit.Application.Services.StatisticService.TestData;
using Service = Timebash.Application.Services.StatisticService;

namespace Timebash.Tests.Unit.Application.Services.StatisticService;

public class StatisticServiceTests
{
    private static readonly Faker _faker = new();
    private readonly Service _service;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IJournalRepository> _journalRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IActivityQueryService> _activityQueryServiceMock;

    public StatisticServiceTests()
    {
        _userRepositoryMock = new();
        _journalRepositoryMock = new();
        _categoryRepositoryMock = new();
        _activityQueryServiceMock = new();

        _service = new(
            _userRepositoryMock.Object,
            _journalRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            _activityQueryServiceMock.Object);
    }

    [Theory]
    [ClassData(typeof(UserAggregateStatisticData))]
    public async Task GetUserAggregateStatistic_WithoutDateRange_ShouldReturnAllStatistic(
        Guid userId,
        List<Activity> activities,
        long expectedTime,
        List<CategoryStatItem> expectedStats)
    {
        var user = new User(userId, _faker.Internet.UserName(), _faker.Internet.Email());
        var expected = new UserAggregateStatisticResponse(expectedTime, expectedStats);

        _userRepositoryMock.Setup(repository => repository.GetByIdAsync(user.Id)).ReturnsAsync(user);
        _activityQueryServiceMock.Setup(service => service.GetActivitiesForUserAsync(user.Id, null, null)).Returns(activities.ToAsyncEnumerable());

        var result = await _service.GetUserAggregateStatisticAsync(user.Id, null, null);
        result.Should().BeEquivalentTo(expected);

        _userRepositoryMock.Verify(repository => repository.GetByIdAsync(user.Id), Times.Once);
        _activityQueryServiceMock.Verify(service => service.GetActivitiesForUserAsync(user.Id, null, null), Times.Once);
    }

    [Fact]
    public async Task GetUserAggregateStatistic_EmptyId_ShouldThrowBadRequest()
        => await FluentActions
            .Awaiting(() => _service.GetUserAggregateStatisticAsync(Guid.Empty, null, null))
            .Should()
            .ThrowAsync<BadRequestException>();

    [Fact]
    public async Task GetUserAggregateStatistic_UserNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        _userRepositoryMock.Setup(repository => repository.GetByIdAsync(id)).ReturnsAsync((User?)null);

        await FluentActions
            .Awaiting(() => _service.GetUserAggregateStatisticAsync(id, null, null))
            .Should()
            .ThrowAsync<NotFoundException>();
    }

    [Theory]
    [ClassData(typeof(JournalStatisticData))]
    public async Task GetJournalStatistic_WithoutDateRange_ShouldReturnAllStatistic(
        Guid journalId,
        Guid userId,
        List<Activity> activities,
        long expectedTime,
        List<CategoryStatItem> expectedStats)
    {
        var journal = new Journal(journalId, userId, _faker.Lorem.Word());
        var expected = new JournalStatisticResponse(expectedTime, expectedStats);

        _journalRepositoryMock.Setup(repository => repository.GetByIdAsync(journal.Id)).ReturnsAsync(journal);
        _activityQueryServiceMock.Setup(service => service.GetActivitiesForJournalAsync(journal.Id, null, null)).Returns(activities.ToAsyncEnumerable());

        var result = await _service.GetJournalStatisticAsync(journal.Id, null, null, userId);
        result.Should().BeEquivalentTo(expected);

        _journalRepositoryMock.Verify(repository => repository.GetByIdAsync(journal.Id), Times.Once);
        _activityQueryServiceMock.Verify(service => service.GetActivitiesForJournalAsync(journal.Id, null, null), Times.Once);
    }

    [Fact]
    public async Task GetJournalStatistic_EmptyId_ShouldThrowBadRequest()
        => await FluentActions
            .Awaiting(() => _service.GetJournalStatisticAsync(Guid.Empty, null, null, Guid.NewGuid()))
            .Should()
            .ThrowAsync<BadRequestException>();

    [Fact]
    public async Task GetJournalStatistic_JournalNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        _journalRepositoryMock.Setup(repository => repository.GetByIdAsync(id)).ReturnsAsync((Journal?)null);

        await FluentActions
            .Awaiting(() => _service.GetJournalStatisticAsync(id, null, null, Guid.NewGuid()))
            .Should()
            .ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetCategoryStatistic_EmptyId_ShouldThrowBadRequest()
        => await FluentActions
            .Awaiting(() => _service.GetCategoryStatisticAsync(Guid.Empty, null, null, Guid.NewGuid()))
            .Should()
            .ThrowAsync<BadRequestException>();

    [Fact]
    public async Task GetCategoryStatistic_CategoryNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        _categoryRepositoryMock.Setup(repository => repository.GetByIdAsync(id)).ReturnsAsync((Category?)null);

        await FluentActions
            .Awaiting(() => _service.GetCategoryStatisticAsync(id, null, null, Guid.NewGuid()))
            .Should()
            .ThrowAsync<NotFoundException>();
    }
}
