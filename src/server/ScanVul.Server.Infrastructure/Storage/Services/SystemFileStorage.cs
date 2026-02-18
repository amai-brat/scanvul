using ScanVul.Server.Domain.Common;

namespace ScanVul.Server.Infrastructure.Storage.Services;

public class SystemFileStorage : IFileStorage
{
    public async Task SaveFileAsync(string prefix, string filename, byte[] bytes, CancellationToken ct = default)
    {
        Directory.CreateDirectory(prefix);

        await File.WriteAllBytesAsync(Path.Combine(prefix, filename), bytes, ct);
    }
}