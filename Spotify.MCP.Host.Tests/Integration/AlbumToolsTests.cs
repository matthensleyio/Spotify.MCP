using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spotify.MCP.Host.Models.Output;
using Spotify.MCP.Host.Services;
using Spotify.MCP.Host.Tools;
using System.Text.Json;
using Xunit;

namespace Spotify.MCP.Host.Tests.Integration;

public class AlbumToolsTests
{
    private readonly ServiceProvider _serviceProvider;

    public AlbumToolsTests()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddUserSecrets<AlbumToolsTests>()
            .Build());

        services.AddLogging();
        services.AddHttpClient();
        services.AddScoped<ISpotifyAuthService, SpotifyAuthService>();
        services.AddScoped<ISpotifyApiService, SpotifyApiService>();
        services.AddScoped<AlbumTools>();

        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetAlbumAsync_ValidAlbumId_ReturnsValidTakeMeBackToEdenData()
    {
        // Arrange
        var albumTools = _serviceProvider.GetRequiredService<AlbumTools>();
        var validAlbumId = "1gjugH97doz3HktiEjx2vY"; // Take Me Back To Eden

        // Act
        var result = await albumTools.GetAlbumAsync(validAlbumId);

        // Assert
        Assert.False(result.StartsWith("Error"), $"Expected success, but got error: {result}");

        var album = JsonSerializer.Deserialize<Album>(result);
        Assert.NotNull(album);
        Assert.Equal(validAlbumId, album.Id);
        Assert.True(album.Name.Contains("Take Me Back To Eden", StringComparison.OrdinalIgnoreCase), $"Expected album name to contain 'Take Me Back To Eden', but got: {album.Name}");
        Assert.Equal("album", album.Type);
        Assert.NotNull(album.Uri);
        Assert.True(album.Uri.StartsWith("spotify:album"), $"Expected album URI to start with 'spotify:album', but got: {album.Uri}");
        Assert.NotNull(album.Href);
        Assert.True(album.Href.StartsWith("https://api.spotify.com/v1/albums/"), $"Expected album href to start with Spotify API URL, but got: {album.Href}");
        Assert.NotNull(album.ExternalUrls);
        Assert.True(album.ExternalUrls.ContainsKey("spotify"), $"Expected external URLs to contain 'spotify' key, but got keys: {string.Join(", ", album.ExternalUrls.Keys)}");
        Assert.NotNull(album.Artists);
        Assert.True(album.Artists.Count > 0, $"Album should have at least one artist, but got count: {album.Artists.Count}");
        Assert.NotNull(album.ReleaseDate);
        Assert.False(string.IsNullOrWhiteSpace(album.ReleaseDate), "Album should have a release date");
        Assert.NotNull(album.ReleaseDatePrecision);
        Assert.False(string.IsNullOrWhiteSpace(album.ReleaseDatePrecision), "Album should have release date precision");
        Assert.True(album.TotalTracks > 0, $"Album should have tracks, but got total: {album.TotalTracks}");
        Assert.NotNull(album.Images);
        Assert.True(album.Images.Count > 0, $"Album should have images, but got count: {album.Images.Count}");
        Assert.NotNull(album.AlbumType);
        Assert.False(string.IsNullOrWhiteSpace(album.AlbumType), "Album should have an album type");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetAlbumAsync_InvalidAlbumId_ReturnsErrorString()
    {
        // Arrange
        var albumTools = _serviceProvider.GetRequiredService<AlbumTools>();
        var invalidAlbumId = "invalid_id_format";

        // Act
        var result = await albumTools.GetAlbumAsync(invalidAlbumId);

        // Assert
        Assert.StartsWith("Error retrieving album", result);
        Assert.Contains("Bad Request", result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetAlbumAsync_NonExistentAlbumId_ReturnsNotFoundString()
    {
        // Arrange
        var albumTools = _serviceProvider.GetRequiredService<AlbumTools>();
        // This ID is syntactically valid but highly unlikely to exist
        var nonExistentAlbumId = "0000000000000000000000"; 

        // Act
        var result = await albumTools.GetAlbumAsync(nonExistentAlbumId);

        // Assert
        Assert.StartsWith("Error retrieving album", result);
        Assert.Contains(nonExistentAlbumId, result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetAlbumAsync_EmptyAlbumId_ReturnsErrorString()
    {
        // Arrange
        var albumTools = _serviceProvider.GetRequiredService<AlbumTools>();
        var emptyAlbumId = "";

        // Act
        var result = await albumTools.GetAlbumAsync(emptyAlbumId);

        // Assert
        Assert.StartsWith("Error retrieving album", result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetAlbumAsync_NullAlbumId_ReturnsErrorString()
    {
        // Arrange
        var albumTools = _serviceProvider.GetRequiredService<AlbumTools>();
        string? nullAlbumId = null;

        // Act
        var result = await albumTools.GetAlbumAsync(nullAlbumId!);

        // Assert
        Assert.StartsWith("Error retrieving album", result);
    }
}