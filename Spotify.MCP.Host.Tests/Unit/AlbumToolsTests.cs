using Moq;
using Spotify.MCP.Host.Services;
using Spotify.MCP.Host.Tools;
using Xunit;
using Microsoft.Extensions.Logging;
using System.Net;
using Spotify.MCP.Host.Models.Output;
using System.Text.Json;

namespace Spotify.MCP.Host.Tests.Unit;

public class AlbumToolsTests
{
    [Fact]
    public async Task GetAlbumAsync_ReturnsErrorString_OnHttpRequestException()
    {
        // Arrange
        var mockSpotifyApiService = new Mock<ISpotifyApiService>();
        mockSpotifyApiService.Setup(s => s.GetAlbumAsync("invalid_id", null))
            .ThrowsAsync(new HttpRequestException("Bad Request", null, HttpStatusCode.BadRequest));

        var mockLogger = new Mock<ILogger<AlbumTools>>();
        var albumTools = new AlbumTools(mockSpotifyApiService.Object, mockLogger.Object);

        // Act
        var result = await albumTools.GetAlbumAsync("invalid_id");

        // Assert
        var error = JsonSerializer.Deserialize<ErrorResponse>(result);
        Assert.NotNull(error);
        Assert.True(error.error);
        Assert.Contains("Bad Request", error.message);
    }

    [Fact]
    public async Task GetAlbumAsync_ReturnsSerializedAlbum_OnSuccess()
    {
        // Arrange
        var expectedAlbum = new Album("valid_album_id", "Test Album", "spotify:album:valid_album_id", "https://api.spotify.com/v1/albums/valid_album_id", new Dictionary<string, string>(), "album", 10, "2023-01-01", "day", "album", new List<Artist>(), new List<Image>(), new List<string>());
        var mockSpotifyApiService = new Mock<ISpotifyApiService>();
        mockSpotifyApiService.Setup(s => s.GetAlbumAsync("valid_album_id", null))
            .ReturnsAsync(expectedAlbum);

        var mockLogger = new Mock<ILogger<AlbumTools>>();
        var albumTools = new AlbumTools(mockSpotifyApiService.Object, mockLogger.Object);

        // Act
        var result = await albumTools.GetAlbumAsync("valid_album_id");

        // Assert
        var expectedJson = JsonSerializer.Serialize(expectedAlbum, new JsonSerializerOptions { WriteIndented = true });
        Assert.Equal(expectedJson, result);
    }
}