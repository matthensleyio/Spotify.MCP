using System.Text.Json.Serialization;

namespace Spotify.MCP.Host.Models.Output;

public record PlaybackContext(
    string Type,
    string Href,
    [property: JsonPropertyName("external_urls")] Dictionary<string, string> ExternalUrls,
    string Uri
);
