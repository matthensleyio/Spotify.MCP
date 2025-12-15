using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spotify.MCP.Host.Models.Output;
using Spotify.MCP.Host.Services;
using Spotify.MCP.Host.Tools;
using System.Text.Json;
using Xunit;

namespace Spotify.MCP.Host.Tests.Integration;

public class SearchToolsTests
{
    private readonly SearchTools _searchTools;

    public SearchToolsTests()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddHttpClient();
        serviceCollection.AddSingleton<ISpotifyApiService, SpotifyApiService>();
        serviceCollection.AddScoped<ISpotifyAuthService, SpotifyAuthService>();

        serviceCollection.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddUserSecrets<AlbumToolsTests>()
            .Build());

        var serviceProvider = serviceCollection.BuildServiceProvider();

        var logger = serviceProvider.GetRequiredService<ILogger<SearchTools>>();
        var spotifyApiService = serviceProvider.GetRequiredService<ISpotifyApiService>();

        _searchTools = new SearchTools(spotifyApiService, logger);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task SearchAsync_ValidQuery_ReturnsResults()
    {
        var result = await _searchTools.SearchAsync("Sleep Token");
        Assert.False(string.IsNullOrWhiteSpace(result));
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task SearchTracksAsync_ValidTrackName_ReturnsValidEuclidTrackData()
    {
        var result = await _searchTools.SearchTracksAsync("Euclid");
        Assert.False(string.IsNullOrWhiteSpace(result));

        var tracks = JsonSerializer.Deserialize<List<Track>>(result);
        Assert.NotNull(tracks);
        Assert.True(tracks.Count > 0, "Should return at least one track");

        var euclidTrack = tracks.FirstOrDefault(t => t.Name.Contains("Euclid", StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(euclidTrack);
        Assert.True(euclidTrack.Name.Contains("Euclid", StringComparison.OrdinalIgnoreCase));
        Assert.Equal("track", euclidTrack.Type);
        Assert.NotNull(euclidTrack.Id);
        Assert.NotNull(euclidTrack.Uri);
        Assert.True(euclidTrack.Uri.StartsWith("spotify:track:"));
        Assert.NotNull(euclidTrack.Href);
        Assert.True(euclidTrack.Href.StartsWith("https://api.spotify.com/v1/tracks/"));
        Assert.NotNull(euclidTrack.ExternalUrls);
        Assert.True(euclidTrack.ExternalUrls.ContainsKey("spotify"));
        Assert.NotNull(euclidTrack.Artists);
        Assert.True(euclidTrack.Artists.Count > 0, "Track should have at least one artist");
        Assert.NotNull(euclidTrack.Album);
        Assert.True(euclidTrack.DurationMs > 0, "Track should have duration > 0");
        Assert.True(euclidTrack.TrackNumber > 0, "Track should have track number > 0");
        Assert.True(euclidTrack.Popularity >= 0, "Track should have non-negative popularity");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task SearchArtistsAsync_ValidArtistName_ReturnsValidSleepTokenData()
    {
        var result = await _searchTools.SearchArtistsAsync("Sleep Token");
        Assert.False(string.IsNullOrWhiteSpace(result));

        var artists = JsonSerializer.Deserialize<List<Artist>>(result);
        Assert.NotNull(artists);
        Assert.True(artists.Count > 0, "Should return at least one artist");

        var sleepToken = artists.FirstOrDefault(a => a.Name.Equals("Sleep Token", StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(sleepToken);
        Assert.Equal("Sleep Token", sleepToken.Name);
        Assert.Equal("artist", sleepToken.Type);
        Assert.NotNull(sleepToken.Id);
        Assert.NotNull(sleepToken.Uri);
        Assert.True(sleepToken.Uri.StartsWith("spotify:artist:"));
        Assert.NotNull(sleepToken.Href);
        Assert.True(sleepToken.Href.StartsWith("https://api.spotify.com/v1/artists/"));
        Assert.NotNull(sleepToken.ExternalUrls);
        Assert.True(sleepToken.ExternalUrls.ContainsKey("spotify"));
        Assert.NotNull(sleepToken.Followers);
        Assert.True(sleepToken.Followers.Total > 0, "Sleep Token should have followers");
        Assert.NotNull(sleepToken.Genres);
        Assert.True(sleepToken.Genres.Count > 0, "Sleep Token should have genres");
        Assert.True(sleepToken.Popularity > 0, "Sleep Token should have popularity > 0");
        Assert.NotNull(sleepToken.Images);
        Assert.True(sleepToken.Images.Count > 0, "Sleep Token should have images");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task SearchAlbumsAsync_ValidAlbumName_ReturnsValidAlbumData()
    {
        var result = await _searchTools.SearchAlbumsAsync("Take Me Back To Eden");
        Assert.False(string.IsNullOrWhiteSpace(result));

        var albums = JsonSerializer.Deserialize<List<Album>>(result);
        Assert.NotNull(albums);
        Assert.True(albums.Count > 0, "Should return at least one album");

        var takeMeBackToEden = albums.FirstOrDefault(a => a.Name.Contains("Take Me Back To Eden", StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(takeMeBackToEden);
        Assert.True(takeMeBackToEden.Name.Contains("Take Me Back To Eden", StringComparison.OrdinalIgnoreCase));
        Assert.Equal("album", takeMeBackToEden.Type);
        Assert.NotNull(takeMeBackToEden.Id);
        Assert.NotNull(takeMeBackToEden.Uri);
        Assert.True(takeMeBackToEden.Uri.StartsWith("spotify:album:"));
        Assert.NotNull(takeMeBackToEden.Href);
        Assert.True(takeMeBackToEden.Href.StartsWith("https://api.spotify.com/v1/albums/"));
        Assert.NotNull(takeMeBackToEden.ExternalUrls);
        Assert.True(takeMeBackToEden.ExternalUrls.ContainsKey("spotify"));
        Assert.NotNull(takeMeBackToEden.Artists);
        Assert.True(takeMeBackToEden.Artists.Count > 0, "Album should have at least one artist");
        Assert.NotNull(takeMeBackToEden.ReleaseDate);
        Assert.NotNull(takeMeBackToEden.ReleaseDatePrecision);
        Assert.True(takeMeBackToEden.TotalTracks > 0, "Album should have tracks");
        Assert.NotNull(takeMeBackToEden.Images);
        Assert.True(takeMeBackToEden.Images.Count > 0, "Album should have images");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task SearchPlaylistsAsync_ValidPlaylistTile_ReturnsValidPlaylistData()
    {
        var result = await _searchTools.SearchPlaylistsAsync("This Is Sleep Token");
        Assert.False(string.IsNullOrWhiteSpace(result));

        var playlists = JsonSerializer.Deserialize<List<Playlist>>(result);
        Assert.NotNull(playlists);
        Assert.True(playlists.Count > 0, "Should return at least one playlist");

        // Test the first playlist (handle possible null names)
        var validPlaylist = playlists.First();
        Assert.NotNull(validPlaylist);
        Assert.Equal("playlist", validPlaylist.Type);
        Assert.NotNull(validPlaylist.Id);
        Assert.NotNull(validPlaylist.Uri);
        Assert.True(validPlaylist.Uri.StartsWith("spotify:playlist:"), $"Expected playlist URI to start with 'spotify:playlist:', but got: {validPlaylist.Uri}");
        Assert.NotNull(validPlaylist.Href);
        Assert.True(validPlaylist.Href.StartsWith("https://api.spotify.com/v1/playlists/"), $"Expected playlist href to start with Spotify API URL, but got: {validPlaylist.Href}");
        Assert.NotNull(validPlaylist.ExternalUrls);
        Assert.True(validPlaylist.ExternalUrls.ContainsKey("spotify"), $"Expected external URLs to contain 'spotify' key, but got keys: {string.Join(", ", validPlaylist.ExternalUrls.Keys)}");
        Assert.NotNull(validPlaylist.Owner);
        Assert.NotNull(validPlaylist.Tracks);
        Assert.True(validPlaylist.Tracks.Total >= 0, $"Playlist should have non-negative track count, but got total: {validPlaylist.Tracks.Total}");
        Assert.NotNull(validPlaylist.Images);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task SearchAudiobooksAsync_ValidAudioBookTitle_ReturnsValidAudiobookData()
    {
        var result = await _searchTools.SearchAudiobooksAsync("Kings of the Wyld");
        Assert.False(string.IsNullOrWhiteSpace(result));

        var audiobooks = JsonSerializer.Deserialize<List<Audiobook>>(result);
        Assert.NotNull(audiobooks);
        Assert.True(audiobooks.Count > 0, "Should return at least one audiobook");

        // Test the first audiobook
        var validAudiobook = audiobooks.First();
        Assert.NotNull(validAudiobook);
        Assert.Equal("audiobook", validAudiobook.Type);
        Assert.NotNull(validAudiobook.Id);
        Assert.NotNull(validAudiobook.Uri);
        Assert.True(validAudiobook.Uri.StartsWith("spotify:"), $"Expected audiobook URI to start with 'spotify:', but got: {validAudiobook.Uri}");
        Assert.NotNull(validAudiobook.Href);
        Assert.True(validAudiobook.Href.StartsWith("https://api.spotify.com/v1/"), $"Expected audiobook href to start with Spotify API URL, but got: {validAudiobook.Href}");
        Assert.NotNull(validAudiobook.ExternalUrls);
        Assert.True(validAudiobook.ExternalUrls.ContainsKey("spotify"), $"Expected external URLs to contain 'spotify' key, but got keys: {string.Join(", ", validAudiobook.ExternalUrls.Keys)}");
        Assert.NotNull(validAudiobook.Authors);
        Assert.True(validAudiobook.Authors.Count > 0, $"Audiobook should have at least one author, but got count: {validAudiobook.Authors.Count}");
        Assert.NotNull(validAudiobook.Narrators);
        Assert.True(validAudiobook.Narrators.Count > 0, $"Audiobook should have at least one narrator, but got count: {validAudiobook.Narrators.Count}");
        Assert.NotNull(validAudiobook.Description);
        Assert.NotNull(validAudiobook.Publisher);
        Assert.NotNull(validAudiobook.Images);
        Assert.True(validAudiobook.Images.Count > 0, $"Audiobook should have images, but got count: {validAudiobook.Images.Count}");
        Assert.True(validAudiobook.TotalChapters > 0, $"Audiobook should have chapters, but got count: {validAudiobook.TotalChapters}");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task SearchAsync_InvalidQuery_ReturnsErrorMessage()
    {
        var result = await _searchTools.SearchAsync("");
        var error = JsonSerializer.Deserialize<ErrorResponse>(result);
        Assert.NotNull(error);
        Assert.True(error.error);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task SearchAsync_InvalidLimit_ReturnsErrorMessage()
    {
        var result = await _searchTools.SearchAsync("test", limit: 0);
        var error = JsonSerializer.Deserialize<ErrorResponse>(result);
        Assert.NotNull(error);
        Assert.True(error.error);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task SearchAsync_InvalidOffset_ReturnsErrorMessage()
    {
        var result = await _searchTools.SearchAsync("test", offset: -1);
        var error = JsonSerializer.Deserialize<ErrorResponse>(result);
        Assert.NotNull(error);
        Assert.True(error.error);
    }
}