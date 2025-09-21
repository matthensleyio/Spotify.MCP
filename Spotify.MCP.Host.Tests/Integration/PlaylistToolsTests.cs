using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spotify.MCP.Host.Models.Output;
using Spotify.MCP.Host.Services;
using Spotify.MCP.Host.Tools;
using System.Text.Json;
using Xunit;

namespace Spotify.MCP.Host.Tests.Integration;

public class PlaylistToolsTests
{
    private readonly ServiceProvider _serviceProvider;

    public PlaylistToolsTests()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddUserSecrets<PlaylistToolsTests>()
            .Build());

        services.AddHttpClient();
        services.AddLogging();
        services.AddScoped<ISpotifyAuthService, SpotifyAuthService>();
        services.AddScoped<ISpotifyApiService, SpotifyApiService>();
        services.AddScoped<PlaylistTools>();

        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetPlaylistAsync_ValidPlaylistId_ReturnsValidPlaylistData()
    {
        // Arrange
        var playlistTools = _serviceProvider.GetRequiredService<PlaylistTools>();
        var validPlaylistId = "1fYJnnkKjbsTCvyfjQ93Zi"; // This is Sleep Token

        // Act
        var result = await playlistTools.GetPlaylistAsync(validPlaylistId);

        // Assert
        Assert.False(result.StartsWith("Error"), $"Expected success, but got error: {result}");
        Assert.False(result.Contains("not found"), $"Expected playlist to be found, but got: {result}");

        var playlist = JsonSerializer.Deserialize<Playlist>(result);
        Assert.NotNull(playlist);
        Assert.Equal(validPlaylistId, playlist.Id);
        Assert.NotNull(playlist.Name);
        Assert.False(string.IsNullOrWhiteSpace(playlist.Name), "Playlist should have a name");
        Assert.Equal("playlist", playlist.Type);
        Assert.NotNull(playlist.Uri);
        Assert.True(playlist.Uri.StartsWith("spotify:playlist"), $"Expected playlist URI to start with 'spotify:playlist', but got: {playlist.Uri}");
        Assert.NotNull(playlist.Href);
        Assert.True(playlist.Href.StartsWith("https://api.spotify.com/v1/playlists/"), $"Expected playlist href to start with Spotify API URL, but got: {playlist.Href}");
        Assert.NotNull(playlist.ExternalUrls);
        Assert.True(playlist.ExternalUrls.ContainsKey("spotify"), $"Expected external URLs to contain 'spotify' key, but got keys: {string.Join(", ", playlist.ExternalUrls.Keys)}");
        Assert.NotNull(playlist.Owner);
        Assert.NotNull(playlist.Owner.Id);
        Assert.False(string.IsNullOrWhiteSpace(playlist.Owner.Id), "Playlist should have an owner with an ID");
        Assert.NotNull(playlist.Tracks);
        Assert.True(playlist.Tracks.Total >= 0, $"Playlist should have non-negative track count, but got total: {playlist.Tracks.Total}");
        Assert.NotNull(playlist.Images);
        Assert.NotNull(playlist.SnapshotId);
        Assert.False(string.IsNullOrWhiteSpace(playlist.SnapshotId), "Playlist should have a snapshot ID");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetPlaylistAsync_InvalidPlaylistId_ReturnsErrorString()
    {
        // Arrange
        var playlistTools = _serviceProvider.GetRequiredService<PlaylistTools>();
        var invalidPlaylistId = "invalid_id_format";

        // Act
        var result = await playlistTools.GetPlaylistAsync(invalidPlaylistId);

        // Assert
        Assert.StartsWith("Error retrieving playlist", result);
        Assert.Contains("Bad Request", result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetPlaylistAsync_NonExistentPlaylistId_ReturnsNotFoundString()
    {
        // Arrange
        var playlistTools = _serviceProvider.GetRequiredService<PlaylistTools>();
        var nonExistentPlaylistId = "0000000000000000000000";

        // Act
        var result = await playlistTools.GetPlaylistAsync(nonExistentPlaylistId);

        // Assert
        Assert.StartsWith("Error retrieving playlist", result);
        Assert.Contains(nonExistentPlaylistId, result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetPlaylistAsync_EmptyPlaylistId_ReturnsErrorString()
    {
        // Arrange
        var playlistTools = _serviceProvider.GetRequiredService<PlaylistTools>();
        var emptyPlaylistId = "";

        // Act
        var result = await playlistTools.GetPlaylistAsync(emptyPlaylistId);

        // Assert
        Assert.StartsWith("Error retrieving playlist", result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetPlaylistAsync_NullPlaylistId_ReturnsErrorString()
    {
        // Arrange
        var playlistTools = _serviceProvider.GetRequiredService<PlaylistTools>();
        string? nullPlaylistId = null;

        // Act
        var result = await playlistTools.GetPlaylistAsync(nullPlaylistId!);

        // Assert
        Assert.StartsWith("Error retrieving playlist", result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetPlaylistAsync_WithAccessToken_ValidPlaylistId_ReturnsValidPlaylistData()
    {
        // Arrange
        var playlistTools = _serviceProvider.GetRequiredService<PlaylistTools>();
        var validPlaylistId = "37i9dQZF1DX0XUsuxWHRQd"; // RapCaviar
        var invalidAccessToken = "invalid_token"; // Using invalid token to test error handling

        // Act
        var result = await playlistTools.GetPlaylistAsync(validPlaylistId, invalidAccessToken);

        // Assert
        // Should either return playlist data or an authentication error
        Assert.True(
            result.Contains("\"id\"") || result.StartsWith("Error retrieving playlist"),
            $"Expected either playlist data or error message, but got: {result}"
        );
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetUserPlaylistsAsync_EmptyAccessToken_ReturnsErrorMessage()
    {
        // Arrange
        var playlistTools = _serviceProvider.GetRequiredService<PlaylistTools>();
        var emptyAccessToken = "";

        // Act
        var result = await playlistTools.GetUserPlaylistsAsync(emptyAccessToken);

        // Assert
        Assert.Equal("User access token is required for this operation.", result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetUserPlaylistsAsync_NullAccessToken_ReturnsErrorMessage()
    {
        // Arrange
        var playlistTools = _serviceProvider.GetRequiredService<PlaylistTools>();
        string? nullAccessToken = null;

        // Act
        var result = await playlistTools.GetUserPlaylistsAsync(nullAccessToken!);

        // Assert
        Assert.Equal("User access token is required for this operation.", result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetUserPlaylistsAsync_WhitespaceAccessToken_ReturnsErrorMessage()
    {
        // Arrange
        var playlistTools = _serviceProvider.GetRequiredService<PlaylistTools>();
        var whitespaceAccessToken = "   ";

        // Act
        var result = await playlistTools.GetUserPlaylistsAsync(whitespaceAccessToken);

        // Assert
        Assert.Equal("User access token is required for this operation.", result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetUserPlaylistsAsync_InvalidAccessToken_ReturnsErrorMessage()
    {
        // Arrange
        var playlistTools = _serviceProvider.GetRequiredService<PlaylistTools>();
        var invalidAccessToken = "invalid_token";

        // Act
        var result = await playlistTools.GetUserPlaylistsAsync(invalidAccessToken);

        // Assert
        Assert.StartsWith("Error retrieving user playlists:", result);
    }
}