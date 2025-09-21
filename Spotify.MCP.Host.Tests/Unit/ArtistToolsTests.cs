using Moq;
using Spotify.MCP.Host.Services;
using Spotify.MCP.Host.Tools;
using Xunit;
using Microsoft.Extensions.Logging;

using System.Text.Json;
using Spotify.MCP.Host.Models.Output;

namespace Spotify.MCP.Host.Tests.Unit;

public class ArtistToolsTests
{
    [Fact]
    public async Task GetArtistAsync_ReturnsNotFoundString_WhenArtistIsNull()
    {
        // Arrange
        var mockSpotifyApiService = new Mock<ISpotifyApiService>();
        mockSpotifyApiService.Setup(s => s.GetArtistAsync("non_existent_artist", null))
            .ReturnsAsync((Artist?)null);

        var mockLogger = new Mock<ILogger<ArtistTools>>();
        var artistTools = new ArtistTools(mockSpotifyApiService.Object, mockLogger.Object);

        // Act
        var result = await artistTools.GetArtistAsync("non_existent_artist");

        // Assert
        Assert.Contains("Error retrieving artist", result);
        Assert.Contains("non_existent_artist", result);
    }

    [Fact]
    public async Task GetArtistAsync_ReturnsSerializedArtist_OnSuccess()
    {
        // Arrange
        var expectedArtist = new Artist("valid_artist_id", "Test Artist", "spotify:artist:valid_artist_id", "https://api.spotify.com/v1/artists/valid_artist_id", new Dictionary<string, string>(), "artist", 100, new List<string>(), new Followers(null, 1000), new List<Image>());
        var mockSpotifyApiService = new Mock<ISpotifyApiService>();
        mockSpotifyApiService.Setup(s => s.GetArtistAsync("valid_artist_id", null))
            .ReturnsAsync(expectedArtist);

        var mockLogger = new Mock<ILogger<ArtistTools>>();
        var artistTools = new ArtistTools(mockSpotifyApiService.Object, mockLogger.Object);

        // Act
        var result = await artistTools.GetArtistAsync("valid_artist_id");

        // Assert
        var expectedJson = JsonSerializer.Serialize(expectedArtist, new JsonSerializerOptions { WriteIndented = true });
        Assert.Equal(expectedJson, result);
    }
}