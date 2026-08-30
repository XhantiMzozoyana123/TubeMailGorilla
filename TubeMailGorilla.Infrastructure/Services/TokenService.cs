using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TubeMailGorilla.Domain;
using TubeMailGorilla.Domain.Entities;
using TubeMailGorilla.Domain.Interfaces;

namespace TubeMailGorilla.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;

    public TokenService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    public string GenerateToken(User user, IList<string> roles, IList<ClaimInfo>? claims = null)
    {
        var secret = _jwtSettings.Secret ?? "defaultSecret12345default";
        var issuer = _jwtSettings.Issuer ?? "TubeMailGorillaAPI";
        var audience = _jwtSettings.Audience ?? "TubeMailGorillaClient";
        var expiryMinutes = _jwtSettings.ExpiryMinutes == 0 ? 1440 : _jwtSettings.ExpiryMinutes;

        var claimList = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim("FullName", user.FullName ?? string.Empty)
        };

        foreach (var role in roles)
        {
            claimList.Add(new Claim(ClaimTypes.Role, role));
        }

        if (claims != null)
        {
            foreach (var claimInfo in claims)
            {
                claimList.Add(new Claim(claimInfo.Type, claimInfo.Value));
            }
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claimList,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
