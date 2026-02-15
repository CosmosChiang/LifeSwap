using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LifeSwap.Api.Domain;
using Microsoft.IdentityModel.Tokens;

namespace LifeSwap.Api.Services;

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly string _secret;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expirationMinutes;

    public JwtTokenService(IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        _secret = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret is not configured.");
        _issuer = jwtSettings["Issuer"] ?? "LifeSwap";
        _audience = jwtSettings["Audience"] ?? "LifeSwap";
        _expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "60");
    }

    public string GenerateToken(User user, IEnumerable<Role> roles)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new("EmployeeId", user.EmployeeId),
            new(ClaimTypes.Email, user.Email),
            new("DepartmentCode", user.DepartmentCode),
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role.Name));
        }

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_expirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public interface IJwtTokenService
{
    string GenerateToken(User user, IEnumerable<Role> roles);
}
