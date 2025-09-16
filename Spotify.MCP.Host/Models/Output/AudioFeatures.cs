using System.Text.Json.Serialization;

namespace Spotify.MCP.Host.Models.Output;

public record AudioFeatures(
    string Id,
    string Uri,
    [property: JsonPropertyName("track_href")] string TrackHref,
    string Type,
    float Acousticness,
    float Danceability,
    float Energy,
    float Instrumentalness,
    float Liveness,
    float Loudness,
    float Speechiness,
    float Valence,
    float Tempo,
    [property: JsonPropertyName("duration_ms")] int DurationMs,
    [property: JsonPropertyName("time_signature")] int TimeSignature,
    int Key,
    int Mode
);
