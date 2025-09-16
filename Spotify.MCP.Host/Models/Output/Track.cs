using System.Text.Json.Serialization;

namespace Spotify.MCP.Host.Models.Output;

public record Track(
    string Id,
    string Name,
    string Uri,
    string Href,
    [property: JsonPropertyName("external_urls")] Dictionary<string, string> ExternalUrls,
    [property: JsonPropertyName("preview_url")] string? PreviewUrl,
    [property: JsonPropertyName("track_number")] int TrackNumber,
    string Type,
    int Popularity,
    [property: JsonPropertyName("duration_ms")] int DurationMs,
    bool Explicit,
    [property: JsonPropertyName("is_playable")] bool? IsPlayable,
    List<Artist> Artists,
    Album Album,
    [property: JsonPropertyName("available_markets")] List<string>? AvailableMarkets
);
