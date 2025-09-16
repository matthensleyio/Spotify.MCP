using Spotify.MCP.Host.Models.Output;

namespace Spotify.MCP.Host.Models.Input;

public record SearchArtists(
    string Href,
    int Limit,
    string? Next,
    int Offset,
    string? Previous,
    int Total,
    List<Artist> Items
);
