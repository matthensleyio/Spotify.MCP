using System.Text.Json.Serialization;

namespace Spotify.MCP.Host.Models;

public record Artist(
    string Id,
    string Name,
    string Uri,
    string Href,
    [property: JsonPropertyName("external_urls")] Dictionary<string, string> ExternalUrls,
    string Type,
    int? Popularity,
    List<string>? Genres,
    int? Followers,
    List<Image>? Images
);
