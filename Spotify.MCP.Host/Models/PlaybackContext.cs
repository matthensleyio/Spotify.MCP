using System.Text.Json.Serialization;

namespace Spotify.MCP.Host.Models;

public record PlaybackContext(
    string Type,
    string Href,
    [property: JsonPropertyName("external_urls")] Dictionary<string, string> ExternalUrls,
    string Uri
);
