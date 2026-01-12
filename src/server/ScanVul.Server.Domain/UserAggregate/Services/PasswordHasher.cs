using System.Security.Cryptography;
using ScanVul.Server.Domain.UserAggregate.ValueObjects;

namespace ScanVul.Server.Domain.UserAggregate.Services;

public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 50000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;
    
    private const char SegmentDelimiter = ':';

    public PasswordHash Hash(string input)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            input,
            salt,
            Iterations,
            Algorithm,
            KeySize
        );
        return new PasswordHash(string.Join(
            SegmentDelimiter,
            Convert.ToHexString(hash),
            Convert.ToHexString(salt),
            Iterations,
            Algorithm
        ));
    }

    public bool Verify(string input, PasswordHash password)
    {
        var segments = password.Value.Split(SegmentDelimiter);
        var hash = Convert.FromHexString(segments[0]);
        var salt = Convert.FromHexString(segments[1]);
        var iterations = int.Parse(segments[2]);
        var algorithm = new HashAlgorithmName(segments[3]);
        var inputHash = Rfc2898DeriveBytes.Pbkdf2(
            input,
            salt,
            iterations,
            algorithm,
            hash.Length
        );
        return CryptographicOperations.FixedTimeEquals(inputHash, hash);
    }
}