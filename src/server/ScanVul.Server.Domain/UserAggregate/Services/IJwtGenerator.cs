using ScanVul.Server.Domain.UserAggregate.Entities;

namespace ScanVul.Server.Domain.UserAggregate.Services;

public interface IJwtGenerator
{
    string GenerateToken(User user);
}