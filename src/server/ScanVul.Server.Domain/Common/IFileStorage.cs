namespace ScanVul.Server.Domain.Common;

public interface IFileStorage
{
    Task SaveFileAsync(string prefix, string filename, byte[] bytes, CancellationToken ct = default);
}