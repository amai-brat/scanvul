using JetBrains.Annotations;
using ScanVul.Server.Domain.UserAggregate.ValueObjects;

namespace ScanVul.Server.Domain.UserAggregate.Entities;

public class User
{
    public int Id { get; set; }
    public string Name { get; private set; } = null!;
    public PasswordHash Password { get; private set; } = null!;
    public bool IsActive { get; private set; }

    [UsedImplicitly]
    private User() {}

    public User(string name, PasswordHash password)
    {
        Name = name;
        Password = password;
        IsActive = true;
    }
}