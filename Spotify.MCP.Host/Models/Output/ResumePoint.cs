using System.Text.Json.Serialization;

namespace Spotify.MCP.Host.Models.Output;

public record ResumePoint(
    [property: JsonPropertyName("fully_played")] bool FullyPlayed,
    [property: JsonPropertyName("resume_position_ms")] int ResumePositionMs
);
