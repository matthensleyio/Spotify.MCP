namespace Spotify.MCP.Host.Models;

public record SearchPlaylists(
    string Href,
    int Limit,
    string? Next,
    int Offset,
    string? Previous,
    int Total,
    List<Playlist> Items
);
