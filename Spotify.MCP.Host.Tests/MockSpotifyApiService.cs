using System.Threading.Tasks;
using System.Collections.Generic;
using Spotify.MCP.Host.Models.Output;
using Spotify.MCP.Host.Models.Input;
using Spotify.MCP.Host.Services;

namespace Spotify.MCP.Host.Tests;

public class MockSpotifyApiService : ISpotifyApiService
{
    public Task<Track?> GetTrackAsync(string trackId, string? accessToken = null)
    {
        return Task.FromResult<Track?>(new Track(
            trackId,
            "Mock Track",
            "mock:uri",
            "mock:href",
            new Dictionary<string, string>(),
            null,
            1,
            "track",
            50,
            180000,
            false,
            true,
            new List<Artist> { new Artist("1", "Mock Artist", "mock:uri", "mock:href", new Dictionary<string, string>(), "artist", 50, null, null, null) },
            new Album("1", "Mock Album", "mock:uri", "mock:href", new Dictionary<string, string>(), "album", 10, "2023-01-01", "day", "album", new List<Artist>(), new List<Image>(), null),
            null
        ));
    }

    public Task<List<Track>> GetTracksAsync(string[] trackIds, string? accessToken = null)
    {
        return Task.FromResult(new List<Track> { new Track(
            trackIds[0],
            "Mock Track",
            "mock:uri",
            "mock:href",
            new Dictionary<string, string>(),
            null,
            1,
            "track",
            50,
            180000,
            false,
            true,
            new List<Artist> { new Artist("1", "Mock Artist", "mock:uri", "mock:href", new Dictionary<string, string>(), "artist", 50, null, null, null) },
            new Album("1", "Mock Album", "mock:uri", "mock:href", new Dictionary<string, string>(), "album", 10, "2023-01-01", "day", "album", new List<Artist>(), new List<Image>(), null),
            null
        ) });
    }

    public Task<AudioFeatures?> GetAudioFeaturesAsync(string trackId, string? accessToken = null)
    {
        return Task.FromResult<AudioFeatures?>(null);
    }

    public Task<Artist?> GetArtistAsync(string artistId, string? accessToken = null)
    {
        return Task.FromResult<Artist?>(new Artist(
            artistId,
            "Mock Artist",
            "mock:uri",
            "mock:href",
            new Dictionary<string, string>(),
            "artist",
            50,
            null,
            null,
            null
        ));
    }

    public Task<List<Album>> GetArtistAlbumsAsync(string artistId, string? accessToken = null)
    {
        return Task.FromResult(new List<Album> { new Album(
            "1",
            "Mock Album",
            "mock:uri",
            "mock:href",
            new Dictionary<string, string>(),
            "album",
            10,
            "2023-01-01",
            "day",
            "album",
            new List<Artist>(),
            new List<Image>(),
            null
        ) });
    }

    public Task<List<Track>> GetArtistTopTracksAsync(string artistId, string market = "US", string? accessToken = null)
    {
        return Task.FromResult(new List<Track> { new Track(
            "1",
            "Mock Top Track",
            "mock:uri",
            "mock:href",
            new Dictionary<string, string>(),
            null,
            1,
            "track",
            50,
            180000,
            false,
            true,
            new List<Artist> { new Artist("1", "Mock Artist", "mock:uri", "mock:href", new Dictionary<string, string>(), "artist", 50, null, null, null) },
            new Album("1", "Mock Album", "mock:uri", "mock:href", new Dictionary<string, string>(), "album", 10, "2023-01-01", "day", "album", new List<Artist>(), new List<Image>(), null),
            null
        ) });
    }

    public Task<Album?> GetAlbumAsync(string albumId, string? accessToken = null)
    {
        return Task.FromResult<Album?>(new Album(
            albumId,
            "Mock Album",
            "mock:uri",
            "mock:href",
            new Dictionary<string, string>(),
            "album",
            10,
            "2023-01-01",
            "day",
            "album",
            new List<Artist>(),
            new List<Image>(),
            null
        ));
    }

    public Task<List<Track>> GetAlbumTracksAsync(string albumId, string? accessToken = null)
    {
        return Task.FromResult(new List<Track> { new Track(
            "1",
            "Mock Album Track",
            "mock:uri",
            "mock:href",
            new Dictionary<string, string>(),
            null,
            1,
            "track",
            50,
            180000,
            false,
            true,
            new List<Artist> { new Artist("1", "Mock Artist", "mock:uri", "mock:href", new Dictionary<string, string>(), "artist", 50, null, null, null) },
            new Album("1", "Mock Album", "mock:uri", "mock:href", new Dictionary<string, string>(), "album", 10, "2023-01-01", "day", "album", new List<Artist>(), new List<Image>(), null),
            null
        ) });
    }

    public Task<SearchResponse> SearchAsync(string query, string[] types, int limit = 20, int offset = 0, string? accessToken = null)
    {
        return Task.FromResult(new SearchResponse(
            new SearchTracks("mock:href", limit, null, offset, null, 1, new List<Track> { new Track(
                "1",
                "Mock Track",
                "mock:uri",
                "mock:href",
                new Dictionary<string, string>(),
                null,
                1,
                "track",
                50,
                180000,
                false,
                true,
                new List<Artist> { new Artist("1", "Mock Artist", "mock:uri", "mock:href", new Dictionary<string, string>(), "artist", 50, null, null, null) },
                new Album("1", "Mock Album", "mock:uri", "mock:href", new Dictionary<string, string>(), "album", 10, "2023-01-01", "day", "album", new List<Artist>(), new List<Image>(), null),
                null
            ) }),
            null,
            null,
            null,
            null
        ));
    }

    public Task<User?> GetCurrentUserAsync(string accessToken)
    {
        return Task.FromResult<User?>(null);
    }

    public Task<List<Playlist>> GetUserPlaylistsAsync(string accessToken)
    {
        return Task.FromResult(new List<Playlist>());
    }

    public Task<Playlist?> GetPlaylistAsync(string playlistId, string? accessToken = null)
    {
        return Task.FromResult<Playlist?>(null);
    }

    public Task<PlaybackState?> GetCurrentPlaybackAsync(string accessToken)
    {
        return Task.FromResult<PlaybackState?>(null);
    }

    public Task PausePlaybackAsync(string accessToken)
    {
        return Task.CompletedTask;
    }

    public Task StartPlaybackAsync(string accessToken, string? contextUri = null, string[]? uris = null)
    {
        return Task.CompletedTask;
    }

    public Task SkipToNextAsync(string accessToken)
    {
        return Task.CompletedTask;
    }

    public Task SkipToPreviousAsync(string accessToken)
    {
        return Task.CompletedTask;
    }

    public Task<Audiobook?> GetAudiobookAsync(string audiobookId, string market = "US", string? accessToken = null)
    {
        return Task.FromResult<Audiobook?>(null);
    }

    public Task<List<Audiobook>> GetAudiobooksAsync(string[] audiobookIds, string market = "US", string? accessToken = null)
    {
        return Task.FromResult(new List<Audiobook>());
    }

    public Task<List<Chapter>> GetAudiobookChaptersAsync(string audiobookId, string market = "US", int limit = 20, int offset = 0, string? accessToken = null)
    {
        return Task.FromResult(new List<Chapter>());
    }

    public Task<List<Audiobook>> GetUserSavedAudiobooksAsync(string accessToken, int limit = 20, int offset = 0)
    {
        return Task.FromResult(new List<Audiobook>());
    }

    public Task SaveAudiobooksForUserAsync(string accessToken, string[] audiobookIds)
    {
        return Task.CompletedTask;
    }

    public Task RemoveUserSavedAudiobooksAsync(string accessToken, string[] audiobookIds)
    {
        return Task.CompletedTask;
    }

    public Task<List<bool>> CheckUserSavedAudiobooksAsync(string accessToken, string[] audiobookIds)
    {
        return Task.FromResult(new List<bool> { true });
    }
}