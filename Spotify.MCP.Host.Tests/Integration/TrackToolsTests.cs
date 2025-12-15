using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spotify.MCP.Host.Models.Output;
using Spotify.MCP.Host.Services;
using Spotify.MCP.Host.Tools;
using System.Text.Json;
using Xunit;

namespace Spotify.MCP.Host.Tests.Integration;

public class TrackToolsTests
{
    private readonly ServiceProvider _serviceProvider;

    public TrackToolsTests()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddUserSecrets<TrackToolsTests>()
            .Build());

        services.AddHttpClient();
        services.AddLogging();
        services.AddScoped<ISpotifyAuthService, SpotifyAuthService>();
        services.AddScoped<ISpotifyApiService, SpotifyApiService>();
        services.AddScoped<TrackTools>();

        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetTrackAsync_ValidTrackId_ReturnsValidEuclidData()
    {
        // Arrange
        var trackTools = _serviceProvider.GetRequiredService<TrackTools>();
        var validTrackId = "3hZIvVYZWsuFpdWdXQFgKx"; // Euclid by Sleep Token

        // Act
        var result = await trackTools.GetTrackAsync(validTrackId);

        // Assert
        Assert.False(result.StartsWith("Error"), $"Expected success, but got error: {result}");

        var track = JsonSerializer.Deserialize<Track>(result);
        Assert.NotNull(track);
        Assert.Equal(validTrackId, track.Id);
        Assert.True(track.Name.Contains("Euclid", StringComparison.OrdinalIgnoreCase), $"Expected track name to contain 'Euclid', but got: {track.Name}");
        Assert.Equal("track", track.Type);
        Assert.NotNull(track.Uri);
        Assert.True(track.Uri.StartsWith("spotify:track"), $"Expected track URI to start with 'spotify:track', but got: {track.Uri}");
        Assert.NotNull(track.Href);
        Assert.True(track.Href.StartsWith("https://api.spotify.com/v1/tracks/"), $"Expected track href to start with Spotify API URL, but got: {track.Href}");
        Assert.NotNull(track.ExternalUrls);
        Assert.True(track.ExternalUrls.ContainsKey("spotify"), $"Expected external URLs to contain 'spotify' key, but got keys: {string.Join(", ", track.ExternalUrls.Keys)}");
        Assert.NotNull(track.Artists);
        Assert.True(track.Artists.Count > 0, $"Track should have at least one artist, but got count: {track.Artists.Count}");
        Assert.NotNull(track.Album);
        Assert.True(track.DurationMs > 0, $"Track should have duration > 0, but got: {track.DurationMs}");
        Assert.True(track.TrackNumber > 0, $"Track should have track number > 0, but got: {track.TrackNumber}");
        Assert.True(track.Popularity >= 0, $"Track should have non-negative popularity, but got: {track.Popularity}");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetTrackAsync_InvalidTrackId_ReturnsErrorString()
    {
        // Arrange
        var trackTools = _serviceProvider.GetRequiredService<TrackTools>();
        var invalidTrackId = "invalid_id_format";

        // Act
        var result = await trackTools.GetTrackAsync(invalidTrackId);

        // Assert
        var error = JsonSerializer.Deserialize<ErrorResponse>(result);
        Assert.NotNull(error);
        Assert.True(error.error);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetTrackAsync_NonExistentTrackId_ReturnsNotFoundString()
    {
        // Arrange
        var trackTools = _serviceProvider.GetRequiredService<TrackTools>();
        // This ID is syntactically valid but highly unlikely to exist
        var nonExistentTrackId = "0000000000000000000000";

        // Act
        var result = await trackTools.GetTrackAsync(nonExistentTrackId);

        // Assert
        var error = JsonSerializer.Deserialize<ErrorResponse>(result);
        Assert.NotNull(error);
        Assert.True(error.error);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetTrackAsync_EmptyTrackId_ReturnsErrorString()
    {
        // Arrange
        var trackTools = _serviceProvider.GetRequiredService<TrackTools>();
        var emptyTrackId = "";

        // Act
        var result = await trackTools.GetTrackAsync(emptyTrackId);

        // Assert
        var error = JsonSerializer.Deserialize<ErrorResponse>(result);
        Assert.NotNull(error);
        Assert.True(error.error);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetTrackAsync_NullTrackId_ReturnsErrorString()
    {
        // Arrange
        var trackTools = _serviceProvider.GetRequiredService<TrackTools>();
        string? nullTrackId = null;

        // Act
        var result = await trackTools.GetTrackAsync(nullTrackId!);

        // Assert
        var error = JsonSerializer.Deserialize<ErrorResponse>(result);
        Assert.NotNull(error);
        Assert.True(error.error);
    }
}