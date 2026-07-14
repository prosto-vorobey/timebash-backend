using Timebash.Core.DTOs.Requests;
using Timebash.Core.DTOs.Responses;
using Timebash.Core.Exceptions;

namespace Timebash.Core.Services;

/// <summary>
/// Provides operations for managing categories.
/// </summary>
public interface ICategoryService
{
    /// <summary>
    /// Retrieves a category by its unique identifier.
    /// </summary>
    /// <param name="id">The category ID.</param>
    /// <param name="userId">The owner user ID.</param>
    /// <param name="cancellationToken">A token to cancel the request if the client disconnects.</param>
    /// <returns>The requested category.</returns>
    /// <exception cref="BadRequestException">Thrown when <paramref name="id"/> is empty.</exception>
    /// <exception cref="NotFoundException">Thrown when the category does not exist or does not belong to the user.</exception>
    Task<CategoryResponse> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Returns all activities that belong to the specified category.
    /// </summary>
    /// <param name="id">The category ID.</param>
    /// <param name="userId">The owner user ID.</param>
    /// <param name="cancellationToken">A token to cancel the request if the client disconnects.</param>
    /// <returns>A collection of activities linked to the category.</returns>
    /// <exception cref="BadRequestException">Thrown when <paramref name="id"/> is empty.</exception>
    /// <exception cref="NotFoundException">Thrown when the category does not exist or does not belong to the user.</exception>
    Task<ActivitiesListResponse> GetActivitiesByCategoryIdAsync(Guid id, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new category.
    /// </summary>
    /// <param name="categoryRequest">The category data.</param>
    /// <param name="userId">The owner user ID.</param>
    /// <param name="cancellationToken">A token to cancel the request if the client disconnects.</param>
    /// <returns>The newly created category.</returns>
    Task<CategoryResponse> CreateAsync(CategoryRequest categoryRequest, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Replaces an existing category with the provided data.
    /// </summary>
    /// <param name="id">The category ID to update.</param>
    /// <param name="categoryRequest">The new category data.</param>
    /// <param name="userId">The owner user ID.</param>
    /// <param name="cancellationToken">A token to cancel the request if the client disconnects.</param>
    /// <returns>True if any changes were applied, otherwise false.</returns>
    /// <exception cref="BadRequestException">Thrown when <paramref name="id"/> is empty.</exception>
    /// <exception cref="NotFoundException">Thrown when the category does not exist or does not belong to the user.</exception>
    Task<bool> UpdateAsync(Guid id, CategoryRequest categoryRequest, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes the specified category.
    /// </summary>
    /// <param name="id">The category ID to delete.</param>
    /// <param name="userId">The owner user ID.</param>
    /// <param name="cancellationToken">A token to cancel the request if the client disconnects.</param>
    /// <returns>A task representing the asynchronous delete operation.</returns>
    /// <exception cref="BadRequestException">Thrown when <paramref name="id"/> is empty.</exception>
    /// <exception cref="NotFoundException">Thrown when the category does not exist or does not belong to the user.</exception>
    Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken);
}
