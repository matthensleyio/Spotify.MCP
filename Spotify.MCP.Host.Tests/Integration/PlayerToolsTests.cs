using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spotify.MCP.Host.Services;
using Spotify.MCP.Host.Tools;
using Xunit;

namespace Spotify.MCP.Host.Tests.Integration;

public class PlayerToolsTests
{
    private readonly ServiceProvider _serviceProvider;

    public PlayerToolsTests()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddUserSecrets<PlayerToolsTests>()
            .Build());

        services.AddHttpClient();
        services.AddLogging();
        services.AddScoped<ISpotifyAuthService, SpotifyAuthService>();
        services.AddScoped<ISpotifyApiService, SpotifyApiService>();
        services.AddScoped<PlayerTools>();

        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetCurrentPlaybackAsync_EmptyAccessToken_ReturnsErrorMessage()
    {
        // Arrange
        var playerTools = _serviceProvider.GetRequiredService<PlayerTools>();
        var emptyAccessToken = "";

        // Act
        var result = await playerTools.GetCurrentPlaybackAsync(emptyAccessToken);

        // Assert
        Assert.Equal("User access token is required for this operation.", result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetCurrentPlaybackAsync_NullAccessToken_ReturnsErrorMessage()
    {
        // Arrange
        var playerTools = _serviceProvider.GetRequiredService<PlayerTools>();
        string? nullAccessToken = null;

        // Act
        var result = await playerTools.GetCurrentPlaybackAsync(nullAccessToken!);

        // Assert
        Assert.Equal("User access token is required for this operation.", result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetCurrentPlaybackAsync_WhitespaceAccessToken_ReturnsErrorMessage()
    {
        // Arrange
        var playerTools = _serviceProvider.GetRequiredService<PlayerTools>();
        var whitespaceAccessToken = "   ";

        // Act
        var result = await playerTools.GetCurrentPlaybackAsync(whitespaceAccessToken);

        // Assert
        Assert.Equal("User access token is required for this operation.", result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetCurrentPlaybackAsync_InvalidAccessToken_ReturnsErrorMessage()
    {
        // Arrange
        var playerTools = _serviceProvider.GetRequiredService<PlayerTools>();
        var invalidAccessToken = "invalid_token";

        // Act
        var result = await playerTools.GetCurrentPlaybackAsync(invalidAccessToken);

        // Assert
        Assert.StartsWith("Error retrieving current playback:", result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task PausePlaybackAsync_EmptyAccessToken_ReturnsErrorMessage()
    {
        // Arrange
        var playerTools = _serviceProvider.GetRequiredService<PlayerTools>();
        var emptyAccessToken = "";

        // Act
        var result = await playerTools.PausePlaybackAsync(emptyAccessToken);

        // Assert
        Assert.Equal("User access token is required for this operation.", result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task PausePlaybackAsync_InvalidAccessToken_ReturnsErrorMessage()
    {
        // Arrange
        var playerTools = _serviceProvider.GetRequiredService<PlayerTools>();
        var invalidAccessToken = "invalid_token";

        // Act
        var result = await playerTools.PausePlaybackAsync(invalidAccessToken);

        // Assert
        Assert.StartsWith("Error pausing playback:", result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task StartPlaybackAsync_EmptyAccessToken_ReturnsErrorMessage()
    {
        // Arrange
        var playerTools = _serviceProvider.GetRequiredService<PlayerTools>();
        var emptyAccessToken = "";

        // Act
        var result = await playerTools.StartPlaybackAsync(emptyAccessToken);

        // Assert
        Assert.Equal("User access token is required for this operation.", result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task StartPlaybackAsync_InvalidAccessToken_ReturnsErrorMessage()
    {
        // Arrange
        var playerTools = _serviceProvider.GetRequiredService<PlayerTools>();
        var invalidAccessToken = "invalid_token";

        // Act
        var result = await playerTools.StartPlaybackAsync(invalidAccessToken);

        // Assert
        Assert.StartsWith("Error starting playback:", result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task StartPlaybackAsync_WithContextUri_InvalidAccessToken_ReturnsErrorMessage()
    {
        // Arrange
        var playerTools = _serviceProvider.GetRequiredService<PlayerTools>();
        var invalidAccessToken = "invalid_token";
        var contextUri = "spotify:album:1gjugH97doz3HktiEjx2vY";

        // Act
        var result = await playerTools.StartPlaybackAsync(invalidAccessToken, contextUri);

        // Assert
        Assert.StartsWith("Error starting playback:", result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task StartPlaybackAsync_WithUris_InvalidAccessToken_ReturnsErrorMessage()
    {
        // Arrange
        var playerTools = _serviceProvider.GetRequiredService<PlayerTools>();
        var invalidAccessToken = "invalid_token";
        var uris = "spotify:track:3hZIvVYZWsuFpdWdXQFgKx,spotify:track:4f3c6lFeJNRJrb6LuCB2l8";

        // Act
        var result = await playerTools.StartPlaybackAsync(invalidAccessToken, null, uris);

        // Assert
        Assert.StartsWith("Error starting playback:", result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task SkipToNextAsync_EmptyAccessToken_ReturnsErrorMessage()
    {
        // Arrange
        var playerTools = _serviceProvider.GetRequiredService<PlayerTools>();
        var emptyAccessToken = "";

        // Act
        var result = await playerTools.SkipToNextAsync(emptyAccessToken);

        // Assert
        Assert.Equal("User access token is required for this operation.", result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task SkipToNextAsync_InvalidAccessToken_ReturnsErrorMessage()
    {
        // Arrange
        var playerTools = _serviceProvider.GetRequiredService<PlayerTools>();
        var invalidAccessToken = "invalid_token";

        // Act
        var result = await playerTools.SkipToNextAsync(invalidAccessToken);

        // Assert
        Assert.StartsWith("Error skipping to next track:", result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task SkipToPreviousAsync_EmptyAccessToken_ReturnsErrorMessage()
    {
        // Arrange
        var playerTools = _serviceProvider.GetRequiredService<PlayerTools>();
        var emptyAccessToken = "";

        // Act
        var result = await playerTools.SkipToPreviousAsync(emptyAccessToken);

        // Assert
        Assert.Equal("User access token is required for this operation.", result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task SkipToPreviousAsync_InvalidAccessToken_ReturnsErrorMessage()
    {
        // Arrange
        var playerTools = _serviceProvider.GetRequiredService<PlayerTools>();
        var invalidAccessToken = "invalid_token";

        // Act
        var result = await playerTools.SkipToPreviousAsync(invalidAccessToken);

        // Assert
        Assert.StartsWith("Error skipping to previous track:", result);
    }
}