namespace ScanVul.Server.Domain.Common;

public interface IFileStorage
{
    Task<Stream?> GetFileAsync(string prefix, string fileName, CancellationToken ct = default);
    Task SaveFileAsync(string prefix, string filename, byte[] bytes, CancellationToken ct = default);
}