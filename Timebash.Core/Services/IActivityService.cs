using Timebash.Core.DTOs.Requests;
using Timebash.Core.DTOs.Responses;
using Timebash.Core.Exceptions;

namespace Timebash.Core.Services;

/// <summary>
/// Provides operations for managing activities.
/// </summary>
public interface IActivityService
{
    /// <summary>
    /// Retrieves an activity by its unique identifier.
    /// </summary>
    /// <param name="id">The activity ID.</param>
    /// <param name="userId">The owner user ID.</param>
    /// <returns>The requested activity.</returns>
    /// <exception cref="BadRequestException">Thrown when <paramref name="id"/> is empty.</exception>
    /// <exception cref="NotFoundException">Thrown when the activity does not exist or does not belong to the user.</exception>
    Task<ActivityResponse> GetByIdAsync(Guid id, Guid userId);

    /// <summary>
    /// Returns all categories that belong to the specified activity.
    /// </summary>
    /// <param name="id">The activity ID.</param>
    /// <param name="userId">The owner user ID.</param>
    /// <returns>A collection of categories linked to the activity.</returns>
    /// <exception cref="BadRequestException">Thrown when <paramref name="id"/> is empty.</exception>
    /// <exception cref="NotFoundException">Thrown when the activity does not exist or does not belong to the user.</exception>
    Task<CategoriesListResponse> GetCategoriesByActivityIdAsync(Guid id, Guid userId);

    /// <summary>
    /// Creates a new activity.
    /// </summary>
    /// <remarks>
    /// This operation also updates the journal's UpdatedAt timestamp.
    /// </remarks>
    /// <param name="request">The activity data.</param>
    /// <param name="userId">The owner user ID.</param>
    /// <returns>The newly created activity.</returns>
    /// <exception cref="BadRequestException">Thrown when <paramref name="request.JournalId"/> is empty or all provided category IDs are empty.</exception>
    /// <exception cref="NotFoundException">Thrown when the journal or at least one of the categories from request does not exist or does not belong to the user.</exception>
    Task<ActivityResponse> CreateAsync(ActivityRequest request, Guid userId);

    /// <summary>
    /// Creates a new activity with auto-time correction for existing activities.
    /// </summary>
    /// <remarks>
    /// This operation updates the UpdatedAt timestamp of the journal and any affected activities.
    /// </remarks>
    /// <param name="request">The activity data with time correction options.</param>
    /// <param name="userId">The owner user ID.</param>
    /// <returns>The newly created activity and additional activities.</returns>
    /// <exception cref="BadRequestException">Thrown when <paramref name="request.JournalId"/> is empty, all provided category IDs are empty, or an invalid correction type is specified.</exception>
    /// <exception cref="NotFoundException">Thrown when the journal, a category, or a conflict resolution activity does not exist or does not belong to the user.</exception>
    Task<ActivityWithCorrectionResponse> CreateWithCorrectionAsync(ActivityWithCorrectionRequest request, Guid userId);

    /// <summary>
    /// Adds a category to the specified activity.
    /// </summary>
    /// <remarks>
    /// This operation updates the activity's UpdatedAt timestamp.
    /// </remarks>
    /// <param name="activityId">The activity ID.</param>
    /// <param name="categoryId">The category ID to add.</param>
    /// <param name="userId">The owner user ID.</param>
    /// <returns>True if any changes were applied, otherwise false.</returns>
    /// <exception cref="BadRequestException">Thrown when <paramref name="activityId"/> or <paramref name="categoryId"/> is empty.</exception>
    /// <exception cref="NotFoundException">Thrown when the activity or category does not exist or does not belong to the user.</exception>
    Task<bool> AddCategoryToActivityAsync(Guid activityId, Guid categoryId, Guid userId);

    /// <summary>
    /// Adds a collection of categories to the specified activity.
    /// </summary>
    /// <remarks>
    /// This operation updates the activity's UpdatedAt timestamp.
    /// </remarks>
    /// <param name="activityId">The activity ID.</param>
    /// <param name="request">A collection of category IDs to add.</param>
    /// <param name="userId">The owner user ID.</param>
    /// <returns>True if any changes were applied, otherwise false.</returns>
    /// <exception cref="BadRequestException">Thrown when <paramref name="activityId"/> is empty or all category IDs in <paramref name="request"/> are empty.</exception>
    /// <exception cref="NotFoundException">Thrown when the activity or at least one of the categories does not exist or does not belong to the user.</exception>
    Task<bool> AddCategoriesToActivityAsync(Guid activityId, ActivityCategoriesRequest request, Guid userId);

    /// <summary>
    /// Replaces an existing activity with the provided data.
    /// </summary>
    /// <remarks>
    /// This operation updates the activity's UpdatedAt timestamp.
    /// </remarks>
    /// <param name="id">The activity ID to update.</param>
    /// <param name="request">The new activity data.</param>
    /// <param name="userId">The owner user ID.</param>
    /// <returns>True if any changes were applied, otherwise false.</returns>
    /// <exception cref="BadRequestException">Thrown when <paramref name="id"/>, <paramref name="request.JournalId"/> is empty, or all category IDs in <paramref name="request"/> are empty.</exception>
    /// <exception cref="NotFoundException">Thrown when the activity or <paramref name="request.JournalId"/> or at least one of the categories from request does not exist or does not belong to the user.</exception>
    Task<bool> UpdateAsync(Guid id, ActivityRequest request, Guid userId);

    /// <summary>
    /// Replaces the categories of the specified activity with the provided data.
    /// </summary>
    /// <remarks>
    /// This operation updates the activity's UpdatedAt timestamp.
    /// </remarks>
    /// <param name="activityId">The activity ID.</param>
    /// <param name="request">A collection of category IDs to set.</param>
    /// <param name="userId">The owner user ID.</param>
    /// <returns>True if any changes were applied, otherwise false.</returns>
    /// <exception cref="BadRequestException">Thrown when <paramref name="activityId"/> is empty or all category IDs in <paramref name="request"/> are empty.</exception>
    /// <exception cref="NotFoundException">Thrown when the activity or at least one of the categories from request does not exist or does not belong to the user.</exception>
    Task<bool> UpdateActivityCategoriesAsync(Guid activityId, ActivityCategoriesRequest request, Guid userId);

    /// <summary>
    /// Deletes the specified activity.
    /// </summary>
    /// <param name="id">The activity ID to delete.</param>
    /// <param name="userId">The owner user ID.</param>
    /// <returns>A task representing the asynchronous delete operation.</returns>
    /// <exception cref="BadRequestException">Thrown when <paramref name="id"/> is empty.</exception>
    /// <exception cref="NotFoundException">Thrown when the activity does not exist or does not belong to the user.</exception>
    Task DeleteAsync(Guid id, Guid userId);

    /// <summary>
    /// Removes a category from the specified activity.
    /// </summary>
    /// <remarks>
    /// This operation updates the activity's UpdatedAt timestamp.
    /// </remarks>
    /// <param name="activityId">The activity ID.</param>
    /// <param name="categoryId">The category ID to remove.</param>
    /// <param name="userId">The owner user ID.</param>
    /// <returns>True if any changes were applied, otherwise false.</returns>
    /// <exception cref="BadRequestException">Thrown when <paramref name="activityId"/> or <paramref name="categoryId"/> is empty.</exception>
    /// <exception cref="NotFoundException">Thrown when the activity or category does not exist or does not belong to the user.</exception>
    Task<bool> RemoveCategoryFromActivityAsync(Guid activityId, Guid categoryId, Guid userId);
}
