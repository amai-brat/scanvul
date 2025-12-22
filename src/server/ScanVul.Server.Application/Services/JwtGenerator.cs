using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ScanVul.Server.Application.Options;
using ScanVul.Server.Domain.UserAggregate.Entities;
using ScanVul.Server.Domain.UserAggregate.Services;

namespace ScanVul.Server.Application.Services;

public class JwtGenerator(IOptionsMonitor<JwtOptions> monitor) : IJwtGenerator
{
    private readonly JwtOptions _jwtOptions = monitor.CurrentValue;
    
    public string GenerateToken(User user)
    {
        var claims = new List<Claim>
        {
            new (JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString()),
            new (JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new (JwtRegisteredClaimNames.Name, user.Name),
        };
            
        var now = DateTime.UtcNow;
        var jwt = new JwtSecurityToken(
            notBefore: now,
            claims: claims,
            expires: now.Add(TimeSpan.FromMinutes(_jwtOptions.AccessTokenLifetimeInMinutes)),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key)),
                SecurityAlgorithms.HmacSha256)
        );
        
        var token = new JwtSecurityTokenHandler().WriteToken(jwt);
            
        return token;
    }
}