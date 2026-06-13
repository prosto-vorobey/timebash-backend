
using Timebash.Application.Extensions;
using Timebash.Application.Extensions.Requests;
using Timebash.Core.Contracts;
using Timebash.Core.DTOs.Requests;
using Timebash.Core.DTOs.Responses;
using Timebash.Core.Exceptions;
using Timebash.Core.Repositories;
using Timebash.Core.Services;

namespace Timebash.Application.Services;

public class AuthService(
    IUnitOfWork unitOfWork,
    IUserRepository userRepository,
    IJournalRepository journalRepository,
    IUserSettingsRepository settingsRepository,
    IPasswordService passwordService,
    IJwtProvider provider) : IAuthService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IJournalRepository _journalRepository = journalRepository;
    private readonly IUserSettingsRepository _settingsRepository = settingsRepository;
    private readonly IPasswordService _passwordService = passwordService;
    private readonly IJwtProvider _provider = provider;

    public async Task<UserResponse> RegisterAsync(RegisterRequest request)
    {
        if (await _userRepository.ExistsByNameAsync(request.Name))
            throw new ResourceConflictException(nameof(request.Name), "User with name already exists");
        if (await _userRepository.ExistsByEmailAsync(request.Email))
            throw new ResourceConflictException(nameof(request.Email), "User with email already exists");

        var user = request.ToUser(Guid.NewGuid());
        user.PasswordHash = _passwordService.HashPassword(user, request.Password);
        _userRepository.Add(user);

        var defaultJournal = new Journal(Guid.NewGuid(), user.Id, "Стандартный журнал");
        _journalRepository.Add(defaultJournal);
        _settingsRepository.Add(new UserSettings()
        {
            UserId = user.Id,
            DefaultJournalId = defaultJournal.Id
        });

        await _unitOfWork.SaveChangesAsync();

        return user.ToResponse();
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = (request.Login.Contains('@')
            ? await _userRepository.GetByEmailAsync(request.Login)
            : await _userRepository.GetByNameAsync(request.Login))
            ?? throw new UnauthorizedException();

        if (!_passwordService.VerifyPassword(user, request.Password)) throw new UnauthorizedException();

        return new LoginResponse(_provider.GenerateToken(user));
    }
}
