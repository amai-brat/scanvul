namespace ScanVul.Server.Domain.UserAggregate.ValueObjects;

public record PasswordHash
{
    public string Value { get; set; }

    public PasswordHash(string hashedPassword)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hashedPassword);
        
        Value =  hashedPassword;
    }
}