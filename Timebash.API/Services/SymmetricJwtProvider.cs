using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Timebash.API.Settings;
using Timebash.Core.Entities;

namespace Timebash.API.Services;

public class SymmetricJwtProvider(IOptions<JwtSettings> settings) : IJwtProvider
{
    private readonly JwtSettings _settings = settings.Value;

    public string GenerateToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
        };

        var signingCredentiels = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret)),
            SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
            _settings.Issuer,
            _settings.Audience,
            claims,
            null,
            DateTime.UtcNow.AddMinutes(_settings.ExpirationInMinutes),
            signingCredentiels
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
