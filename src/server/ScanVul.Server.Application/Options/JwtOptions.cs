namespace ScanVul.Server.Application.Options;

public class JwtOptions
{
    public required string Key { get; set; }
    public required int AccessTokenLifetimeInMinutes { get; set; }

    private const int KeyMinLength = 36;
    public static void Validate(JwtOptions? options)
    {
        if (options is null)
            throw new ArgumentNullException(nameof(options));
        
        if (string.IsNullOrWhiteSpace(options.Key) || options.Key.Length < KeyMinLength)
            throw new ArgumentException($"{nameof(options.Key)} must have at least {KeyMinLength} characters");

        if (options.AccessTokenLifetimeInMinutes < 1)
            throw new ArgumentException($"{nameof(options.AccessTokenLifetimeInMinutes)} must have at least 1 minute");
    }
}