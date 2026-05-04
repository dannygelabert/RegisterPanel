using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using RegisterPanel.Application.Interfaces.Services;
using RegisterPanel.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace RegisterPanel.Infrastructure.Services;

public sealed class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (string Token, DateTimeOffset ExpiresAt) GenerateToken(ApplicationUser user, IList<string> roles)
    {
        string secret = _configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("Jwt:Secret is not configured.");
        string issuer   = _configuration["Jwt:Issuer"]   ?? "RegisterPanelPlatform";
        string audience = _configuration["Jwt:Audience"] ?? "RegisterPanelPlatformClients";
        int expirationMinutes = int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "60");

        SymmetricSecurityKey key         = new(Encoding.UTF8.GetBytes(secret));
        SigningCredentials   credentials = new(key, SecurityAlgorithms.HmacSha256);
        DateTimeOffset       expiresAt   = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes);

        List<Claim> claims =
        [
            new Claim(JwtRegisteredClaimNames.Sub,        user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email,      user.Email!),
            new Claim(JwtRegisteredClaimNames.GivenName,  user.FirstName),
            new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
            new Claim(JwtRegisteredClaimNames.Jti,        Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64),
        ];

        foreach (string role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        JwtSecurityToken token = new(
            issuer:             issuer,
            audience:           audience,
            claims:             claims,
            expires:            expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
