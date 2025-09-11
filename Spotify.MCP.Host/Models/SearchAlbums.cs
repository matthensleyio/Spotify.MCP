namespace Spotify.MCP.Host.Models;

public record SearchAlbums(
    string Href,
    int Limit,
    string? Next,
    int Offset,
    string? Previous,
    int Total,
    List<Album> Items
);
