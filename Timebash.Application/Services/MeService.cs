using Timebash.Application.Extensions;
using Timebash.Application.Helpers;
using Timebash.Core.Contracts;
using Timebash.Core.DTOs.Requests;
using Timebash.Core.DTOs.Responses;
using Timebash.Core.Exceptions;
using Timebash.Core.Repositories;
using Timebash.Core.Services;

namespace Timebash.Application.Services;

public class MeService(
    IUnitOfWork unitOfWork,
    IUserRepository userRepository,
    IUserSettingsRepository settingsRepository,
    IJournalRepository journalRepository,
    ICategoryRepository categoryRepository,
    IPasswordService passwordService) : IMeService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IUserSettingsRepository _settingsRepository = settingsRepository;
    private readonly IJournalRepository _journalRepository = journalRepository;
    private readonly ICategoryRepository _categoryRepository = categoryRepository;
    private readonly IPasswordService _passwordService = passwordService;

    public async Task<UserResponse> GetAsync(Guid userId, CancellationToken cancellationToken)
        => (await EntityAccessGuard.EnsureUserAccessAsync(_userRepository, userId, cancellationToken)).ToResponse();

    public async Task<JournalsListResponse> GetJournalsAsync(Guid userId, CancellationToken cancellationToken)
    {
        await EntityAccessGuard.ValidateUserExistsAsync(_userRepository, userId, cancellationToken);
        var journals = await _journalRepository.GetByUserIdAsync(userId, cancellationToken);
        return new ([.. journals.Select(journal => journal.ToResponse())]);
    }

    public async Task<CategoriesListResponse> GetCategoriesAsync(Guid userId, CancellationToken cancellationToken)
    {
        await EntityAccessGuard.ValidateUserExistsAsync(_userRepository, userId, cancellationToken);
        var categories = await _categoryRepository.GetByUserIdAsync(userId, cancellationToken);
        return new ([.. categories.Select(category => category.ToResponse())]);
    }

    public async Task<JournalResponse> GetDefaultJournalAsync(Guid userId, CancellationToken cancellationToken)
    {
        await EntityAccessGuard.ValidateUserExistsAsync(_userRepository, userId, cancellationToken);
        var userSettings = await _settingsRepository.GetByIdAsync(userId, cancellationToken) ?? throw new NotFoundException();
        var journal = await _journalRepository.GetByIdAsync(userSettings.DefaultJournalId, cancellationToken)
            ?? throw new NotFoundException();

        return journal.ToResponse();
    }

    public async Task<bool> UpdateNameAsync(UserNameUpdateRequest request, Guid userId, CancellationToken cancellationToken)
    {
        if (await _userRepository.ExistsByNameAsync(request.Name, cancellationToken)) 
            throw new ResourceConflictException(nameof(request.Name), "User with name already exists");

        var user = await EntityAccessGuard.EnsureUserAccessAsync(_userRepository, userId, cancellationToken);
        if (request.Name == user.Name) return false;

        user.Name = request.Name;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> UpdateEmailAsync(UserEmailUpdateRequest request, Guid userId, CancellationToken cancellationToken)
    {
        if (await _userRepository.ExistsByEmailAsync(request.Email, cancellationToken)) 
            throw new ResourceConflictException(nameof(request.Email), "User with email already exists");

        var user = await EntityAccessGuard.EnsureUserAccessAsync(_userRepository, userId, cancellationToken);
        if (request.Email == user.Email) return false;

        user.Email = request.Email;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> UpdatePasswordAsync(PasswordUpdateRequest request, Guid userId, CancellationToken cancellationToken)
    {
        var user = await EntityAccessGuard.EnsureUserAccessAsync(_userRepository, userId, cancellationToken);
        if (!_passwordService.VerifyPassword(user, request.CurrentPassword)) throw new UnauthorizedException("Current password is incorrect");
        if (request.CurrentPassword == request.NewPassword) return false;

        user.PasswordHash = _passwordService.HashPassword(user, request.NewPassword);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> UpdateDefaultJournalAsync(DefaultJournalUpdateRequest request, Guid userId, CancellationToken cancellationToken)
    {
        await EntityAccessGuard.ValidateUserExistsAsync(_userRepository, userId, cancellationToken);
        await EntityAccessGuard.ValidateJournalAccessAsync(_journalRepository, request.JournalId, userId, cancellationToken);

        var userSettings = await _settingsRepository.GetByIdAsync(userId, cancellationToken) ?? throw new NotFoundException();
        if (userSettings.DefaultJournalId == request.JournalId) return false;

        userSettings.DefaultJournalId = request.JournalId;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task DeleteAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await EntityAccessGuard.EnsureUserAccessAsync(_userRepository, userId, cancellationToken);

        await _settingsRepository.DeleteAsync(userId, cancellationToken);
        _userRepository.Delete(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
