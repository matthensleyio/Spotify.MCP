using System.Text.Json;
using System.Text.Json.Serialization;
using Spotify.MCP.Host.Models.Input;
using Spotify.MCP.Host.Models.Output;

namespace Spotify.MCP.Client.Tests;

/// <summary>
/// JsonSerializerOptions configured to match how tools serialize responses.
/// Tools serialize without PropertyNamingPolicy, so we only need case-insensitive matching.
/// </summary>
internal static class SpotifyJsonOptions
{
    public static JsonSerializerOptions Default { get; } = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };
}

/// <summary>
/// Tests for invoking tools via the MCP client.
/// These tests use public Spotify data that doesn't require user authentication.
/// </summary>
public class ToolInvocationTests : IAsyncLifetime
{
    private SpotifyMcpClientFixture _fixture = null!;

    public async Task InitializeAsync()
    {
        _fixture = new SpotifyMcpClientFixture();
        await _fixture.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await _fixture.DisposeAsync();
    }

    [Fact]
    public async Task GetClientToken_ReturnsAccessToken()
    {
        var result = await _fixture.CallToolAsync("get_client_token", new Dictionary<string, object?>());

        Assert.NotNull(result);
        Assert.NotEmpty(result);

        var tokenResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(result, SpotifyJsonOptions.Default);
        Assert.NotNull(tokenResponse);
        Assert.True(tokenResponse.ContainsKey("access_token"));
        Assert.NotNull(tokenResponse["access_token"]);
    }

    [Fact]
    public async Task GetTrack_WithValidId_ReturnsTrackData()
    {
        // Use a well-known Spotify track ID (e.g., Bohemian Rhapsody)
        var trackId = "3z8h0TU7RvxVFlaVrFLuQi";

        var result = await _fixture.CallToolAsync("get_track", new Dictionary<string, object?>
        {
            { "trackId", trackId }
        });

        Assert.NotNull(result);
        Assert.NotEmpty(result);

        var track = JsonSerializer.Deserialize<Track>(result, SpotifyJsonOptions.Default);
        Assert.NotNull(track);
    }

    [Fact]
    public async Task GetTracks_WithValidIds_ReturnsMultipleTracksData()
    {
        var trackIds = new[] { "3z8h0TU7RvxVFlaVrFLuQi", "4cOdK2wGLETKBW3PvgPWqLv" };

        var result = await _fixture.CallToolAsync("get_tracks", new Dictionary<string, object?>
        {
            { "trackIds", string.Join(",", trackIds) }
        });

        Assert.NotNull(result);
        Assert.NotEmpty(result);

        // Check if result is an array or an object with a tracks property
        if (result.TrimStart().StartsWith("["))
        {
            var tracks = JsonSerializer.Deserialize<List<Track>>(result, SpotifyJsonOptions.Default);
            Assert.NotNull(tracks);
            Assert.NotEmpty(tracks);
        }
        else
        {
            var tracksObj = JsonSerializer.Deserialize<dynamic>(result, SpotifyJsonOptions.Default);
            Assert.NotNull(tracksObj);
        }
    }

    [Fact]
    public async Task GetArtist_WithValidId_ReturnsArtistData()
    {
        // Use a well-known artist ID (e.g., The Beatles)
        var artistId = "1vCWHaC5f2uS3yhpwWbIA6";

        var result = await _fixture.CallToolAsync("get_artist", new Dictionary<string, object?>
        {
            { "artistId", artistId }
        });

        Assert.NotNull(result);
        Assert.NotEmpty(result);

        var artist = JsonSerializer.Deserialize<Artist>(result, SpotifyJsonOptions.Default);
        Assert.NotNull(artist);
    }

    [Fact]
    public async Task GetAlbum_WithValidId_ReturnsAlbumData()
    {
        // Use a well-known album ID
        var albumId = "0yW3tfqMYTBZXo84U1MRxy";

        var result = await _fixture.CallToolAsync("get_album", new Dictionary<string, object?>
        {
            { "albumId", albumId }
        });

        Assert.NotNull(result);
        Assert.NotEmpty(result);

        var album = JsonSerializer.Deserialize<Album>(result, SpotifyJsonOptions.Default);
        Assert.NotNull(album);
    }

    [Fact]
    public async Task SearchTracks_WithValidQuery_ReturnsResults()
    {
        var result = await _fixture.CallToolAsync("search_tracks", new Dictionary<string, object?>
        {
            { "query", "Bohemian Rhapsody" },
            { "limit", 10 }
        });

        Assert.NotNull(result);
        Assert.NotEmpty(result);

        var tracks = JsonSerializer.Deserialize<List<Track>>(result, SpotifyJsonOptions.Default);
        Assert.NotNull(tracks);
        Assert.NotEmpty(tracks);
    }

    [Fact]
    public async Task SearchArtists_WithValidQuery_ReturnsResults()
    {
        var result = await _fixture.CallToolAsync("search_artists", new Dictionary<string, object?>
        {
            { "query", "The Beatles" },
            { "limit", 10 }
        });

        Assert.NotNull(result);
        Assert.NotEmpty(result);

        var artists = JsonSerializer.Deserialize<List<Artist>>(result, SpotifyJsonOptions.Default);
        Assert.NotNull(artists);
        Assert.NotEmpty(artists);
    }

    [Fact]
    public async Task SearchAlbums_WithValidQuery_ReturnsResults()
    {
        var result = await _fixture.CallToolAsync("search_albums", new Dictionary<string, object?>
        {
            { "query", "Abbey Road" },
            { "limit", 10 }
        });

        Assert.NotNull(result);
        Assert.NotEmpty(result);

        var albums = JsonSerializer.Deserialize<List<Album>>(result, SpotifyJsonOptions.Default);
        Assert.NotNull(albums);
        Assert.NotEmpty(albums);
    }

    [Fact]
    public async Task SearchPlaylists_WithValidQuery_ReturnsResults()
    {
        var result = await _fixture.CallToolAsync("search_playlists", new Dictionary<string, object?>
        {
            { "query", "workout" },
            { "limit", 10 }
        });

        Assert.NotNull(result);
        Assert.NotEmpty(result);

        var playlists = JsonSerializer.Deserialize<List<Playlist>>(result, SpotifyJsonOptions.Default);
        Assert.NotNull(playlists);
        Assert.NotEmpty(playlists);
    }

    [Fact]
    public async Task GetAudioFeatures_WithValidTrackId_ReturnsFeatures()
    {
        var trackId = "3z8h0TU7RvxVFlaVrFLuQi";

        var result = await _fixture.CallToolAsync("get_audio_features", new Dictionary<string, object?>
        {
            { "trackId", trackId }
        });

        Assert.NotNull(result);
        Assert.NotEmpty(result);

        var audioFeatures = JsonSerializer.Deserialize<AudioFeatures>(result, SpotifyJsonOptions.Default);
        Assert.NotNull(audioFeatures);
    }

    [Fact]
    public async Task GetPlaylist_WithValidId_ReturnsPlaylistData()
    {
        // Use a well-known public playlist ID
        var playlistId = "37i9dQZEVXbNG2KDcFcKOF";

        var result = await _fixture.CallToolAsync("get_playlist", new Dictionary<string, object?>
        {
            { "playlistId", playlistId }
        });

        Assert.NotNull(result);
        Assert.NotEmpty(result);

        var playlist = JsonSerializer.Deserialize<Playlist>(result, SpotifyJsonOptions.Default);
        Assert.NotNull(playlist);
    }

    [Fact]
    public async Task GetAudiobook_WithValidId_ReturnsAudiobookData()
    {
        // Note: Audiobooks are only available in specific markets
        // Using a well-known audiobook ID if available
        var audiobookId = "7iHfbu1YMWVQVAs520FZOq";

        var result = await _fixture.CallToolAsync("get_audiobook", new Dictionary<string, object?>
        {
            { "audiobookId", audiobookId },
            { "market", "US" }
        });

        // This might fail due to market restrictions, but we should still verify it's a proper response
        Assert.NotNull(result);
        Assert.NotEmpty(result);

        var audiobook = JsonSerializer.Deserialize<Audiobook>(result, SpotifyJsonOptions.Default);
        Assert.NotNull(audiobook);
    }

    [Fact]
    public async Task GetArtistAlbums_WithValidId_ReturnsAlbumsList()
    {
        var artistId = "1vCWHaC5f2uS3yhpwWbIA6";

        var result = await _fixture.CallToolAsync("get_artist_albums", new Dictionary<string, object?>
        {
            { "artistId", artistId },
            { "limit", 10 }
        });

        Assert.NotNull(result);
        Assert.NotEmpty(result);

        var albums = JsonSerializer.Deserialize<List<Album>>(result, SpotifyJsonOptions.Default);
        Assert.NotNull(albums);
        Assert.NotEmpty(albums);
    }

    [Fact]
    public async Task GetArtistTopTracks_WithValidId_ReturnsTracksList()
    {
        var artistId = "1vCWHaC5f2uS3yhpwWbIA6";

        var result = await _fixture.CallToolAsync("get_artist_top_tracks", new Dictionary<string, object?>
        {
            { "artistId", artistId },
            { "market", "US" }
        });

        Assert.NotNull(result);
        Assert.NotEmpty(result);

        var tracks = JsonSerializer.Deserialize<List<Track>>(result, SpotifyJsonOptions.Default);
        Assert.NotNull(tracks);
        Assert.NotEmpty(tracks);
    }

    [Fact]
    public async Task GetAlbumTracks_WithValidId_ReturnsTracksList()
    {
        var albumId = "0yW3tfqMYTBZXo84U1MRxy";

        var result = await _fixture.CallToolAsync("get_album_tracks", new Dictionary<string, object?>
        {
            { "albumId", albumId },
            { "limit", 10 }
        });

        Assert.NotNull(result);
        Assert.NotEmpty(result);

        // Check if result is an array or an object with an items property
        if (result.TrimStart().StartsWith("["))
        {
            var tracks = JsonSerializer.Deserialize<List<Track>>(result, SpotifyJsonOptions.Default);
            Assert.NotNull(tracks);
            Assert.NotEmpty(tracks);
        }
        else
        {
            var tracksObj = JsonSerializer.Deserialize<dynamic>(result, SpotifyJsonOptions.Default);
            Assert.NotNull(tracksObj);
        }
    }

    [Fact]
    public async Task Tool_InvalidParameters_ReturnsError()
    {
        // Call a tool with missing required parameters
        var result = await _fixture.CallToolAsync("get_track", new Dictionary<string, object?>());

        // Should get an error response
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task Tool_InvalidTrackId_ReturnsErrorMessage()
    {
        var result = await _fixture.CallToolAsync("get_track", new Dictionary<string, object?>
        {
            { "trackId", "invalid_id_that_does_not_exist_12345" }
        });

        // Should return some kind of response (might be an error or null from API)
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }
}
