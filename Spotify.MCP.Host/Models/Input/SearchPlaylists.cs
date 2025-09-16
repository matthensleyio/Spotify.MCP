using Spotify.MCP.Host.Models.Output;

namespace Spotify.MCP.Host.Models.Input;

public record SearchPlaylists(
    string Href,
    int Limit,
    string? Next,
    int Offset,
    string? Previous,
    int Total,
    List<Playlist> Items
);
