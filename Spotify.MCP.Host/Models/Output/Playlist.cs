using System.Text.Json.Serialization;

namespace Spotify.MCP.Host.Models.Output;

public record Playlist(
    string Id,
    string Name,
    string Description,
    string Uri,
    string Href,
    [property: JsonPropertyName("external_urls")] Dictionary<string, string> ExternalUrls,
    bool Public,
    bool Collaborative,
    string Type,
    User Owner,
    List<Image> Images,
    PlaylistTracks Tracks,
    [property: JsonPropertyName("snapshot_id")] string SnapshotId
);
