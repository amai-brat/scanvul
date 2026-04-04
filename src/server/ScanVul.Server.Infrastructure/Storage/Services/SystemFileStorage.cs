using ScanVul.Server.Domain.Common;

namespace ScanVul.Server.Infrastructure.Storage.Services;

public class SystemFileStorage : IFileStorage
{
    public Task<Stream?> GetFileAsync(string prefix, string fileName, CancellationToken ct = default)
    {
        var filePath = Path.Combine(prefix, fileName);
        if (!File.Exists(filePath))
        {
            return Task.FromResult<Stream?>(null);
        }
        
        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return Task.FromResult<Stream?>(stream);
    }

    public async Task SaveFileAsync(string prefix, string filename, byte[] bytes, CancellationToken ct = default)
    {
        Directory.CreateDirectory(prefix);

        await File.WriteAllBytesAsync(Path.Combine(prefix, filename), bytes, ct);
    }
}