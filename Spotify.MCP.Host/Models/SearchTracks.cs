namespace Spotify.MCP.Host.Models;

public record SearchTracks(
    string Href,
    int Limit,
    string? Next,
    int Offset,
    string? Previous,
    int Total,
    List<Track> Items
);
