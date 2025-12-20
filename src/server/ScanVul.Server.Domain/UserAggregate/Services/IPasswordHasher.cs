using ScanVul.Server.Domain.UserAggregate.ValueObjects;

namespace ScanVul.Server.Domain.UserAggregate.Services;

public interface IPasswordHasher
{
    public PasswordHash Hash(string input);

    public bool Verify(string input, PasswordHash password);
}