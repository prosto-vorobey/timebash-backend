using Timebash.Application.Extensions;
using Timebash.Application.Extensions.Requests;
using Timebash.Application.Helpers;
using Timebash.Core.DTOs.Requests;
using Timebash.Core.DTOs.Responses;
using Timebash.Core.Services;
using Timebash.Core.Contracts;
using Timebash.Core.Repositories;

namespace Timebash.Application.Services;

public class CategoryService(IUnitOfWork unitOfWork, ICategoryRepository repository) : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICategoryRepository _repository = repository;

    public async Task<CategoryResponse> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken)
        => (await EntityAccessGuard.EnsureCategoryAccessAsync(_repository, id, userId, cancellationToken)).ToResponse();

    public async Task<ActivitiesListResponse> GetActivitiesByCategoryIdAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        await EntityAccessGuard.ValidateCategoryAccessAsync(_repository, id, userId, cancellationToken);
        var activities = await _repository.GetActivitiesByCategoryIdAsync(id, cancellationToken);

        return new ([.. activities.Select(activity => activity.ToResponse())]);
    }

    public async Task<CategoryResponse> CreateAsync(CategoryRequest categoryRequest, Guid userId, CancellationToken cancellationToken)
    {
        var category = categoryRequest.ToCategory(Guid.NewGuid(), userId);

        _repository.Add(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return category.ToResponse();
    }

    public async Task<bool> UpdateAsync(Guid id, CategoryRequest categoryRequest, Guid userId, CancellationToken cancellationToken)
    {
        var category = await EntityAccessGuard.EnsureCategoryAccessAsync(_repository, id, userId, cancellationToken);
        if (!category.ApplyUpdate(categoryRequest)) return false;

        category.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        var category = await EntityAccessGuard.EnsureCategoryAccessAsync(_repository, id, userId, cancellationToken);
        _repository.Delete(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
