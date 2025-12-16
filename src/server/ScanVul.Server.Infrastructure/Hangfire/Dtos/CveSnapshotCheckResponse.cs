using System.Text.Json.Serialization;

namespace ScanVul.Server.Infrastructure.Hangfire.Dtos;

public class CveSnapshotCheckResponse
{
    [JsonPropertyName("data")]
    public CveSnapshotCheckResponseData Data { get; set; } = null!;
}

public class CveSnapshotCheckResponseData
{
    [JsonPropertyName("last_snapshot_at")]
    public DateTimeOffset LastSnapshotAt { get; set; }

    [JsonPropertyName("last_snapshot_link")]
    public string LastSnapshotLink { get; set; } = null!;
}