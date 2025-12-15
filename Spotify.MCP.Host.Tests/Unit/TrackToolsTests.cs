using Moq;
using Spotify.MCP.Host.Services;
using Spotify.MCP.Host.Tools;
using Xunit;
using Microsoft.Extensions.Logging;
using System.Text.Json;

using System.Text.Json;
using Spotify.MCP.Host.Models.Output;

namespace Spotify.MCP.Host.Tests.Unit;

public class TrackToolsTests
{
    [Fact]
    public async Task GetTrackAsync_ReturnsErrorString_OnDeserializationException()
    {
        // Arrange
        var mockSpotifyApiService = new Mock<ISpotifyApiService>();
        mockSpotifyApiService.Setup(s => s.GetTrackAsync("malformed_track_id", null))
            .ThrowsAsync(new JsonException("Deserialization failed due to unexpected format."));

        var mockLogger = new Mock<ILogger<TrackTools>>();
        var trackTools = new TrackTools(mockSpotifyApiService.Object, mockLogger.Object);

        // Act
        var result = await trackTools.GetTrackAsync("malformed_track_id");

        // Assert
        var error = JsonSerializer.Deserialize<ErrorResponse>(result);
        Assert.NotNull(error);
        Assert.True(error.error);
        Assert.Contains("Deserialization", error.message);
    }

    [Fact]
    public async Task GetTrackAsync_ReturnsSerializedTrack_OnSuccess()
    {
        // Arrange
        var expectedArtist = new Artist("artist_id", "Artist Name", "spotify:artist:artist_id", "https://api.spotify.com/v1/artists/artist_id", new Dictionary<string, string>(), "artist", 50, new List<string>(), new Followers(null, 500), new List<Image>());
        var expectedAlbum = new Album("album_id", "Album Name", "spotify:album:album_id", "https://api.spotify.com/v1/albums/album_id", new Dictionary<string, string>(), "album", 10, "2023-01-01", "day", "album", new List<Artist> { expectedArtist }, new List<Image>(), new List<string>());
        var expectedTrack = new Track("valid_track_id", "Test Track", "spotify:track:valid_track_id", "https://api.spotify.com/v1/tracks/valid_track_id", new Dictionary<string, string>(), null, 1, "track", 70, 200000, false, true, new List<Artist> { expectedArtist }, expectedAlbum, new List<string>());
        var mockSpotifyApiService = new Mock<ISpotifyApiService>();
        mockSpotifyApiService.Setup(s => s.GetTrackAsync("valid_track_id", null))
            .ReturnsAsync(expectedTrack);

        var mockLogger = new Mock<ILogger<TrackTools>>();
        var trackTools = new TrackTools(mockSpotifyApiService.Object, mockLogger.Object);

        // Act
        var result = await trackTools.GetTrackAsync("valid_track_id");

        // Assert
        var expectedJson = JsonSerializer.Serialize(expectedTrack, new JsonSerializerOptions { WriteIndented = true });
        Assert.Equal(expectedJson, result);
    }
}