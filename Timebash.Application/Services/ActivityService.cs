using Timebash.Application.Extensions;
using Timebash.Application.Extensions.Requests;
using Timebash.Core.Contracts;
using Timebash.Core.DTOs.Requests;
using Timebash.Core.DTOs.Responses;
using Timebash.Core.DTOs.Shared;
using Timebash.Core.Exceptions;
using Timebash.Core.Repositories;
using Timebash.Core.Services;
using Timebash.Core.Services.Access;

namespace Timebash.Application.Services;

public class ActivityService(
    IUnitOfWork unitOfWork,
    IActivityRepository activityRepository,
    ICategoryRepository categoryRepository,
    IJournalRepository journalRepository,
    IActivityAccessService activityAccessService,
    IJournalAccessService journalAccessService,
    ICategoryAccessService categoryAccessService) : IActivityService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IActivityRepository _activityRepository = activityRepository;
    private readonly ICategoryRepository _categoryRepository = categoryRepository;
    private readonly IJournalRepository _journalRepository = journalRepository;
    private readonly IActivityAccessService _activityAccessService = activityAccessService;
    private readonly IJournalAccessService _journalAccessService = journalAccessService;
    private readonly ICategoryAccessService _categoryAccessService = categoryAccessService;

    public async Task<ActivityResponse> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken)
        => (await _activityAccessService.EnsureAccessAsync(id, userId, cancellationToken)).ToResponse();

    public async Task<CategoriesListResponse> GetCategoriesByActivityIdAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        await _activityAccessService.ValidateAccessAsync(id, userId, cancellationToken);
        var categories = await _activityRepository.GetCategoriesByActivityIdAsync(id, cancellationToken);
        return new ([.. categories.Select(category => category.ToResponse())]);
    }

    public async Task<ActivityResponse> CreateAsync(ActivityRequest request, Guid userId, CancellationToken cancellationToken)
    {
        var journal = await _journalAccessService.EnsureAccessAsync(request.JournalId, userId, cancellationToken);
        
        var activity = request.ToActivity(Guid.NewGuid());
        var clearedCategoryIds = (await ResolveCategoryIdsAsync(userId, request.CategoryIds, cancellationToken)).ToList();

        _activityRepository.Add(activity);
        if (clearedCategoryIds.Count > 0)
        {
            _activityRepository.AddCategoriesToActivity(activity.Id, clearedCategoryIds);
        }

        journal.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return activity.ToResponse();
    }

    public async Task<ActivityWithCorrectionResponse> CreateWithCorrectionAsync(
        ActivityWithCorrectionRequest request, 
        Guid userId, 
        CancellationToken cancellationToken)
    {
        var journal = await _journalAccessService.EnsureAccessAsync(request.JournalId, userId, cancellationToken);

        var activityIds = request.ConflictResolutions.Select(resolution => resolution.ActivityId).ToHashSet();
        var activities = (await _activityRepository.GetByIdsAsync(activityIds, cancellationToken)).ToList();
        if (activities.Count != activityIds.Count || activities.Any(activity => activity?.JournalId != request.JournalId)) 
            throw new NotFoundException();

        var newActivity = request.ToActivity(Guid.NewGuid());
        var clearedCategoryIds = (await ResolveCategoryIdsAsync(userId, request.CategoryIds, cancellationToken)).ToList();

        _activityRepository.Add(newActivity);
        if (clearedCategoryIds.Count > 0)
        {
            _activityRepository.AddCategoriesToActivity(newActivity.Id, clearedCategoryIds);
        }

        var activityDictionary = activities.ToDictionary(activity => activity.Id);
        var additionalActivities = new List<Activity>();
        foreach (var resolution in request.ConflictResolutions)
        {
            switch (resolution.Correction)
            {
                case ShiftCorrection shift:
                    var activity = activityDictionary[resolution.ActivityId];
                    activity.UpdateTimeRange(shift.NewStartTime, shift.NewEndTime);
                    activity.UpdatedAt = DateTime.UtcNow;
                    
                    break;
                case SplitCorrection split:
                    activity = activityDictionary[resolution.ActivityId];
                    activity.UpdateTimeRange(split.Parts[0].StartTime, split.Parts[0].EndTime);
                    activity.UpdatedAt = DateTime.UtcNow;

                    var categoryIds = await _activityRepository.GetCategoryIdsByActivityIdAsync(resolution.ActivityId, cancellationToken);

                    for (var i = 1; i < split.Parts.Count; i++)
                    {
                        var partActivity = new Activity(
                            Guid.NewGuid(),
                            activity.JournalId,
                            split.Parts[i].StartTime,
                            split.Parts[i].EndTime,
                            activity.Name);

                        additionalActivities.Add(partActivity);
                        _activityRepository.Add(partActivity);
                        if (categoryIds.Any())
                        {
                            _activityRepository.AddCategoriesToActivity(partActivity.Id, [.. categoryIds]);
                        }
                    }

                    break;
                case DeleteCorrection:
                    _activityRepository.Delete(activityDictionary[resolution.ActivityId]);
                    break;
                default:
                    throw new BadRequestException();
            }
        }

        foreach (var part in request.AdditionalPartIntervals)
        {
            var partActivity = new Activity(Guid.NewGuid(), newActivity.JournalId, part.StartTime, part.EndTime, newActivity.Name);

            additionalActivities.Add(partActivity);
            _activityRepository.Add(partActivity);
            
            if (clearedCategoryIds.Count > 0)
            {
                _activityRepository.AddCategoriesToActivity(partActivity.Id, clearedCategoryIds);
            }
        }

        journal.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ActivityWithCorrectionResponse(newActivity.ToResponse(), additionalActivities.Select(activity => activity.ToResponse()));
    }

    public async Task<bool> AddCategoryToActivityAsync(Guid activityId, Guid categoryId, Guid userId, CancellationToken cancellationToken)
    {
        var activity = await _activityAccessService.EnsureAccessAsync(activityId, userId, cancellationToken);
        await _categoryAccessService.ValidateAccessAsync(categoryId, userId, cancellationToken);
        if (await _activityRepository.IsCategoryLinkedAsync(activityId, categoryId, cancellationToken)) return false;

        _activityRepository.AddCategoryToActivity(activityId, categoryId);
        await ApplyUpdatedChangesAsync(activity, cancellationToken);

        return true;
    }

    public async Task<bool> AddCategoriesToActivityAsync(
        Guid activityId, 
        ActivityCategoriesRequest request, 
        Guid userId, 
        CancellationToken cancellationToken)
    {
        var activity = await _activityAccessService.EnsureAccessAsync(activityId, userId, cancellationToken);
        if (request.CategoryIds.Count == 0) return false;

        var newCategoryIds = await ResolveCategoryIdsAsync(
            userId, 
            request.CategoryIds, 
            await _activityRepository.GetCategoryIdsByActivityIdAsync(activityId, cancellationToken), cancellationToken);
        if (!newCategoryIds.Any()) return false;

        _activityRepository.AddCategoriesToActivity(activityId, newCategoryIds);
        await ApplyUpdatedChangesAsync(activity, cancellationToken);

        return true;
    }

    public async Task<bool> UpdateAsync(Guid id, ActivityRequest request, Guid userId, CancellationToken cancellationToken)
    {
        var activity = await _activityAccessService.EnsureAccessAsync(id, userId, cancellationToken);
        await _journalAccessService.ValidateAccessAsync(request.JournalId, userId, cancellationToken);

        var isActivityUpdated = activity.ApplyUpdate(request);
        var isCategoriesUpdated = await UpdateCategoriesAsync(activity.Id, request.CategoryIds, userId, cancellationToken);
        var isUpdated = isActivityUpdated || isCategoriesUpdated;
        if (isUpdated) await ApplyUpdatedChangesAsync(activity, cancellationToken);

        return isUpdated;
    }

    public async Task<bool> UpdateActivityCategoriesAsync(
        Guid activityId, 
        ActivityCategoriesRequest request, 
        Guid userId, 
        CancellationToken cancellationToken)
    {
        var activity = await _activityAccessService.EnsureAccessAsync(activityId, userId, cancellationToken);

        var isUpdated = await UpdateCategoriesAsync(activity.Id, request.CategoryIds, userId, cancellationToken);
        if (isUpdated) await ApplyUpdatedChangesAsync(activity, cancellationToken);

        return isUpdated;
    }

    public async Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        var activity = await _activityAccessService.EnsureAccessAsync(id, userId, cancellationToken);
        var journal = await _journalRepository.GetByIdAsync(activity.JournalId, cancellationToken) ?? throw new NotFoundException();

        _activityRepository.Delete(activity);
        journal.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> RemoveCategoryFromActivityAsync(Guid activityId, Guid categoryId, Guid userId, CancellationToken cancellationToken)
    {
        var activity = await _activityAccessService.EnsureAccessAsync(activityId, userId, cancellationToken);
        await _categoryAccessService.ValidateAccessAsync(categoryId, userId, cancellationToken);
        if (!await _activityRepository.IsCategoryLinkedAsync(activityId, categoryId, cancellationToken)) return false;

        await _activityRepository.RemoveCategoryFromActivityAsync(activityId, categoryId, cancellationToken);
        await ApplyUpdatedChangesAsync(activity, cancellationToken);
        return true;
    }

    private async Task ApplyUpdatedChangesAsync(Activity activity, CancellationToken cancellationToken)
    {
        activity.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static IEnumerable<Guid> GetClearCategoryIds(IEnumerable<Guid> categoryIds) => categoryIds.Where(id => id != Guid.Empty).Distinct();

    private async Task<bool> UpdateCategoriesAsync(Guid activityId, IEnumerable<Guid> categoryIds, Guid userId, CancellationToken cancellationToken)
    {
        var currentCategories = await _activityRepository.GetCategoryIdsByActivityIdAsync(activityId, cancellationToken);

        if (!categoryIds.Any())
        {
            if (!currentCategories.Any()) return false;
            await _activityRepository.ClearActivityCategoriesAsync(activityId, cancellationToken);
        }
        else
        {
            var newCategoryIds = await ResolveCategoryIdsAsync(userId, categoryIds, currentCategories, cancellationToken);
            if (!newCategoryIds.Any()) return false;

            _activityRepository.AddCategoriesToActivity(activityId, newCategoryIds);
        }

        return true;
    }

    private async Task<IEnumerable<Guid>> ResolveCategoryIdsAsync(Guid userId, IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        var clearedIds = GetClearCategoryIds(ids).ToList();
        if (clearedIds.Count == 0) return [];

        await CheckCategories(clearedIds, userId, cancellationToken);
        return clearedIds;
    }

    private async Task<IEnumerable<Guid>> ResolveCategoryIdsAsync(
        Guid userId, 
        IEnumerable<Guid> ids, 
        IEnumerable<Guid> exceptedIds, 
        CancellationToken cancellationToken)
    {
        var clearedIds = GetClearCategoryIds(ids);
        if (!clearedIds.Any()) throw new BadRequestException();

        var newIds = clearedIds.Except(exceptedIds).ToList();
        if (newIds.Count != 0) await CheckCategories(newIds, userId, cancellationToken);

        return newIds;
    }

    private async Task CheckCategories(List<Guid> categoryIds, Guid userId, CancellationToken cancellationToken)
    {
        var categories = (await _categoryRepository.GetByIdsAsync(categoryIds, cancellationToken)).ToList();
        if (categories.Count != categoryIds.Count) throw new BadRequestException();
        if (categories.Any(category => category.UserId != userId)) throw new NotFoundException();
    }
}
