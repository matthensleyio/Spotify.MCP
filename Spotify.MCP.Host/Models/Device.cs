using System.Text.Json.Serialization;

namespace Spotify.MCP.Host.Models;

public record Device(
    string Id,
    [property: JsonPropertyName("is_active")] bool IsActive,
    [property: JsonPropertyName("is_private_session")] bool IsPrivateSession,
    [property: JsonPropertyName("is_restricted")] bool IsRestricted,
    string Name,
    string Type,
    [property: JsonPropertyName("volume_percent")] int? VolumePercent,
    [property: JsonPropertyName("supports_volume")] bool SupportsVolume
);
