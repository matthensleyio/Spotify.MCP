namespace Spotify.MCP.Host.Models;

public record SearchAudiobooks(
    string Href,
    int Limit,
    string? Next,
    int Offset,
    string? Previous,
    int Total,
    List<Audiobook> Items
);