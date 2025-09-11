using System.Text.Json.Serialization;

namespace Spotify.MCP.Host.Models;

public record User(
    string Id,
    [property: JsonPropertyName("display_name")] string? DisplayName,
    string Uri,
    string Href,
    [property: JsonPropertyName("external_urls")] Dictionary<string, string> ExternalUrls,
    string Type,
    int? Followers,
    List<Image>? Images
);
