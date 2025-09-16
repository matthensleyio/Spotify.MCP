namespace Spotify.MCP.Host.Models.Input;

public record SearchResponse(
    SearchTracks? Tracks,
    SearchAlbums? Albums,
    SearchArtists? Artists,
    SearchPlaylists? Playlists,
    SearchAudiobooks? Audiobooks
);
