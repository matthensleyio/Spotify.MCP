namespace Spotify.MCP.Host.Models;

public record SearchArtists(
    string Href,
    int Limit,
    string? Next,
    int Offset,
    string? Previous,
    int Total,
    List<Artist> Items
);
