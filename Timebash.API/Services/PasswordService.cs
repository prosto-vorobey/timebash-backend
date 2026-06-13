using Microsoft.AspNetCore.Identity;
using Timebash.Core.Entities;

namespace Timebash.API.Services;

public class PasswordService(IPasswordHasher<User> passwordHasher) : IPasswordService
{
    private readonly IPasswordHasher<User> _passwordHasher = passwordHasher;

    public bool VerifyPassword(User user, string password)
        => _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password)
                != PasswordVerificationResult.Failed;

    public string HashPassword(User user, string password) => _passwordHasher.HashPassword(user, password);
}
