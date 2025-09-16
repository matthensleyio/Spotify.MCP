using System.Text.Json.Serialization;

namespace Spotify.MCP.Host.Models.Output;

public record Audiobook(
    string Id,
    string Name,
    string Description,
    string Uri,
    string Href,
    [property: JsonPropertyName("external_urls")] Dictionary<string, string> ExternalUrls,
    [property: JsonPropertyName("html_description")] string HtmlDescription,
    List<Image> Images,
    List<string> Languages,
    [property: JsonPropertyName("media_type")] string MediaType,
    List<Narrator> Narrators,
    string Publisher,
    string Type,
    [property: JsonPropertyName("total_chapters")] int TotalChapters,
    List<Author> Authors,
    [property: JsonPropertyName("available_markets")] List<string> AvailableMarkets,
    List<Copyright> Copyrights,
    [property: JsonPropertyName("edition")] string? Edition,
    [property: JsonPropertyName("explicit")] bool Explicit
);
