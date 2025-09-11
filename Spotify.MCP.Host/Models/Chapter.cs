using System.Text.Json.Serialization;

namespace Spotify.MCP.Host.Models;

public record Chapter(
    string Id,
    string Name,
    string Description,
    string Uri,
    string Href,
    [property: JsonPropertyName("external_urls")] Dictionary<string, string> ExternalUrls,
    [property: JsonPropertyName("html_description")] string HtmlDescription,
    List<Image> Images,
    [property: JsonPropertyName("chapter_number")] int ChapterNumber,
    [property: JsonPropertyName("duration_ms")] int DurationMs,
    [property: JsonPropertyName("audio_preview_url")] string? AudioPreviewUrl,
    bool Explicit,
    [property: JsonPropertyName("is_playable")] bool IsPlayable,
    List<string> Languages,
    [property: JsonPropertyName("release_date")] string ReleaseDate,
    [property: JsonPropertyName("release_date_precision")] string ReleaseDatePrecision,
    [property: JsonPropertyName("resume_point")] ResumePoint? ResumePoint,
    string Type,
    [property: JsonPropertyName("available_markets")] List<string> AvailableMarkets
);
