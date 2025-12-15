using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spotify.MCP.Host.Models.Output;
using Spotify.MCP.Host.Services;
using Spotify.MCP.Host.Tools;
using System.Text.Json;
using Xunit;

namespace Spotify.MCP.Host.Tests.Integration;

public class ArtistToolsTests
{
    private readonly ServiceProvider _serviceProvider;

    public ArtistToolsTests()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddUserSecrets<ArtistToolsTests>()
            .Build());

        services.AddHttpClient();
        services.AddLogging();
        services.AddScoped<ISpotifyAuthService, SpotifyAuthService>();
        services.AddScoped<ISpotifyApiService, SpotifyApiService>();
        services.AddScoped<ArtistTools>();

        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetArtistAsync_ValidArtistId_ReturnsValidSleepTokenData()
    {
        // Arrange
        var artistTools = _serviceProvider.GetRequiredService<ArtistTools>();
        var validArtistId = "2n2RSaZqBuUUukhbLlpnE6"; // Sleep Token

        // Act
        var result = await artistTools.GetArtistAsync(validArtistId);

        // Assert
        Assert.False(result.StartsWith("Error"), $"Expected success, but got error: {result}");

        var artist = JsonSerializer.Deserialize<Artist>(result);
        Assert.NotNull(artist);
        Assert.Equal(validArtistId, artist.Id);
        Assert.Equal("Sleep Token", artist.Name);
        Assert.Equal("artist", artist.Type);
        Assert.NotNull(artist.Uri);
        Assert.True(artist.Uri.StartsWith("spotify:artist"), $"Expected artist URI to start with 'spotify:artist', but got: {artist.Uri}");
        Assert.NotNull(artist.Href);
        Assert.True(artist.Href.StartsWith("https://api.spotify.com/v1/artists/"), $"Expected artist href to start with Spotify API URL, but got: {artist.Href}");
        Assert.NotNull(artist.ExternalUrls);
        Assert.True(artist.ExternalUrls.ContainsKey("spotify"), $"Expected external URLs to contain 'spotify' key, but got keys: {string.Join(", ", artist.ExternalUrls.Keys)}");
        Assert.NotNull(artist.Followers);
        Assert.True(artist.Followers.Total > 0, $"Sleep Token should have followers, but got total: {artist.Followers.Total}");
        Assert.NotNull(artist.Genres);
        Assert.True(artist.Genres.Count > 0, $"Sleep Token should have genres, but got count: {artist.Genres.Count}");
        Assert.True(artist.Popularity > 0, $"Sleep Token should have popularity > 0, but got: {artist.Popularity}");
        Assert.NotNull(artist.Images);
        Assert.True(artist.Images.Count > 0, $"Sleep Token should have images, but got count: {artist.Images.Count}");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetArtistAsync_InvalidArtistId_ReturnsErrorString()
    {
        // Arrange
        var artistTools = _serviceProvider.GetRequiredService<ArtistTools>();
        var invalidArtistId = "invalid_id_format";

        // Act
        var result = await artistTools.GetArtistAsync(invalidArtistId);

        // Assert
        var error = JsonSerializer.Deserialize<ErrorResponse>(result);
        Assert.NotNull(error);
        Assert.True(error.error);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetArtistAsync_NonExistentArtistId_ReturnsNotFoundString()
    {
        // Arrange
        var artistTools = _serviceProvider.GetRequiredService<ArtistTools>();
        // This ID is syntactically valid but highly unlikely to exist
        var nonExistentArtistId = "0000000000000000000000";

        // Act
        var result = await artistTools.GetArtistAsync(nonExistentArtistId);

        // Assert
        var error = JsonSerializer.Deserialize<ErrorResponse>(result);
        Assert.NotNull(error);
        Assert.True(error.error);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetArtistAsync_EmptyArtistId_ReturnsErrorString()
    {
        // Arrange
        var artistTools = _serviceProvider.GetRequiredService<ArtistTools>();
        var emptyArtistId = "";

        // Act
        var result = await artistTools.GetArtistAsync(emptyArtistId);

        // Assert
        var error = JsonSerializer.Deserialize<ErrorResponse>(result);
        Assert.NotNull(error);
        Assert.True(error.error);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetArtistAsync_NullArtistId_ReturnsErrorString()
    {
        // Arrange
        var artistTools = _serviceProvider.GetRequiredService<ArtistTools>();
        string? nullArtistId = null;

        // Act
        var result = await artistTools.GetArtistAsync(nullArtistId!);

        // Assert
        var error = JsonSerializer.Deserialize<ErrorResponse>(result);
        Assert.NotNull(error);
        Assert.True(error.error);
    }
}