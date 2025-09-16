using System.Text.Json.Serialization;

namespace Spotify.MCP.Host.Models.Output;

public record Album(
    string Id,
    string Name,
    string Uri,
    string Href,
    [property: JsonPropertyName("external_urls")] Dictionary<string, string> ExternalUrls,
    [property: JsonPropertyName("album_type")] string AlbumType,
    [property: JsonPropertyName("total_tracks")] int TotalTracks,
    [property: JsonPropertyName("release_date")] string ReleaseDate,
    [property: JsonPropertyName("release_date_precision")] string ReleaseDatePrecision,
    string Type,
    List<Artist> Artists,
    List<Image> Images,
    [property: JsonPropertyName("available_markets")] List<string>? AvailableMarkets
);
