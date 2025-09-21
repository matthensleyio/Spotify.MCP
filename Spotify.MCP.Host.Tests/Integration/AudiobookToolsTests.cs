using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spotify.MCP.Host.Models.Output;
using Spotify.MCP.Host.Services;
using Spotify.MCP.Host.Tools;
using System.Text.Json;
using Xunit;

namespace Spotify.MCP.Host.Tests.Integration;

public class AudiobookToolsTests
{
    private readonly ServiceProvider _serviceProvider;

    public AudiobookToolsTests()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddUserSecrets<AudiobookToolsTests>()
            .Build());

        services.AddHttpClient();
        services.AddLogging();
        services.AddScoped<ISpotifyAuthService, SpotifyAuthService>();
        services.AddScoped<ISpotifyApiService, SpotifyApiService>();
        services.AddScoped<AudiobookTools>();

        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetAudiobookAsync_ValidAudiobookId_ReturnsValidAudiobookData()
    {
        // Arrange
        var audiobookTools = _serviceProvider.GetRequiredService<AudiobookTools>();
        var validAudiobookId = "7iHfbu1YPACw6oZPAFJtqe"; // The Seven Husbands of Evelyn Hugo

        // Act
        var result = await audiobookTools.GetAudiobookAsync(validAudiobookId);

        // Assert
        Assert.False(result.StartsWith("Error"), $"Expected success, but got error: {result}");
        Assert.False(result.Contains("not found"), $"Expected audiobook to be found, but got: {result}");

        var audiobook = JsonSerializer.Deserialize<Audiobook>(result);
        Assert.NotNull(audiobook);
        Assert.Equal(validAudiobookId, audiobook.Id);
        Assert.NotNull(audiobook.Name);
        Assert.False(string.IsNullOrWhiteSpace(audiobook.Name), "Audiobook should have a name");
        Assert.Equal("audiobook", audiobook.Type);
        Assert.NotNull(audiobook.Uri);
        Assert.True(audiobook.Uri.StartsWith("spotify:"), $"Expected audiobook URI to start with 'spotify:', but got: {audiobook.Uri}");
        Assert.NotNull(audiobook.Href);
        Assert.True(audiobook.Href.StartsWith("https://api.spotify.com/v1/"), $"Expected audiobook href to start with Spotify API URL, but got: {audiobook.Href}");
        Assert.NotNull(audiobook.ExternalUrls);
        Assert.True(audiobook.ExternalUrls.ContainsKey("spotify"), $"Expected external URLs to contain 'spotify' key, but got keys: {string.Join(", ", audiobook.ExternalUrls.Keys)}");
        Assert.NotNull(audiobook.Authors);
        Assert.True(audiobook.Authors.Count > 0, $"Audiobook should have at least one author, but got count: {audiobook.Authors.Count}");
        Assert.NotNull(audiobook.Narrators);
        Assert.True(audiobook.Narrators.Count > 0, $"Audiobook should have at least one narrator, but got count: {audiobook.Narrators.Count}");
        Assert.NotNull(audiobook.Description);
        Assert.False(string.IsNullOrWhiteSpace(audiobook.Description), "Audiobook should have a description");
        Assert.NotNull(audiobook.Publisher);
        Assert.False(string.IsNullOrWhiteSpace(audiobook.Publisher), "Audiobook should have a publisher");
        Assert.NotNull(audiobook.Images);
        Assert.True(audiobook.Images.Count > 0, $"Audiobook should have images, but got count: {audiobook.Images.Count}");
        Assert.True(audiobook.TotalChapters > 0, $"Audiobook should have chapters, but got count: {audiobook.TotalChapters}");
        Assert.NotNull(audiobook.Languages);
        Assert.True(audiobook.Languages.Count > 0, $"Audiobook should have languages, but got count: {audiobook.Languages.Count}");
        Assert.NotNull(audiobook.MediaType);
        Assert.False(string.IsNullOrWhiteSpace(audiobook.MediaType), "Audiobook should have a media type");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetAudiobookAsync_InvalidAudiobookId_ReturnsErrorString()
    {
        // Arrange
        var audiobookTools = _serviceProvider.GetRequiredService<AudiobookTools>();
        var invalidAudiobookId = "invalid_id_format";

        // Act
        var result = await audiobookTools.GetAudiobookAsync(invalidAudiobookId);

        // Assert
        Assert.StartsWith("Error retrieving audiobook", result);
        Assert.Contains("Bad Request", result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetAudiobookAsync_NonExistentAudiobookId_ReturnsNotFoundString()
    {
        // Arrange
        var audiobookTools = _serviceProvider.GetRequiredService<AudiobookTools>();
        var nonExistentAudiobookId = "0000000000000000000000";

        // Act
        var result = await audiobookTools.GetAudiobookAsync(nonExistentAudiobookId);

        // Assert
        Assert.StartsWith("Error retrieving audiobook", result);
        Assert.Contains(nonExistentAudiobookId, result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetAudiobooksAsync_ValidAudiobookIds_ReturnsValidAudiobooksData()
    {
        // Arrange
        var audiobookTools = _serviceProvider.GetRequiredService<AudiobookTools>();
        var validAudiobookIds = "7iHfbu1YPACw6oZPAFJtqe,18yVqkdbdRvS24c0Ilj2ci"; // Multiple audiobooks

        // Act
        var result = await audiobookTools.GetAudiobooksAsync(validAudiobookIds);

        // Assert
        Assert.False(result.StartsWith("Error"), $"Expected success, but got error: {result}");

        var audiobooks = JsonSerializer.Deserialize<List<Audiobook>>(result);
        Assert.NotNull(audiobooks);
        Assert.True(audiobooks.Count > 0, "Should return at least one audiobook");

        foreach (var audiobook in audiobooks.Where(a => a != null))
        {
            Assert.Equal("audiobook", audiobook.Type);
            Assert.NotNull(audiobook.Id);
            Assert.NotNull(audiobook.Name);
            Assert.NotNull(audiobook.Uri);
            Assert.True(audiobook.Uri.StartsWith("spotify:"), $"Expected audiobook URI to start with 'spotify:', but got: {audiobook.Uri}");
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetAudiobooksAsync_EmptyIds_ReturnsErrorMessage()
    {
        // Arrange
        var audiobookTools = _serviceProvider.GetRequiredService<AudiobookTools>();
        var emptyIds = "";

        // Act
        var result = await audiobookTools.GetAudiobooksAsync(emptyIds);

        // Assert
        Assert.Equal("No audiobook IDs provided.", result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetAudiobooksAsync_TooManyIds_ReturnsErrorMessage()
    {
        // Arrange
        var audiobookTools = _serviceProvider.GetRequiredService<AudiobookTools>();
        var tooManyIds = string.Join(",", Enumerable.Range(1, 51).Select(i => $"id{i}"));

        // Act
        var result = await audiobookTools.GetAudiobooksAsync(tooManyIds);

        // Assert
        Assert.Equal("Maximum of 50 audiobook IDs allowed per request.", result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetAudiobookChaptersAsync_ValidAudiobookId_ReturnsChaptersData()
    {
        // Arrange
        var audiobookTools = _serviceProvider.GetRequiredService<AudiobookTools>();
        var validAudiobookId = "7iHfbu1YPACw6oZPAFJtqe";

        // Act
        var result = await audiobookTools.GetAudiobookChaptersAsync(validAudiobookId);

        // Assert
        Assert.False(result.StartsWith("Error"), $"Expected success, but got error: {result}");

        // Should return a valid JSON response (chapters or paginated response)
        Assert.False(string.IsNullOrWhiteSpace(result));
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetAudiobookChaptersAsync_InvalidLimit_ReturnsErrorMessage()
    {
        // Arrange
        var audiobookTools = _serviceProvider.GetRequiredService<AudiobookTools>();
        var validAudiobookId = "7iHfbu1YPACw6oZPAFJtqe";
        var invalidLimit = 0;

        // Act
        var result = await audiobookTools.GetAudiobookChaptersAsync(validAudiobookId, "US", invalidLimit);

        // Assert
        Assert.Equal("Limit must be between 1 and 50.", result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetAudiobookChaptersAsync_InvalidOffset_ReturnsErrorMessage()
    {
        // Arrange
        var audiobookTools = _serviceProvider.GetRequiredService<AudiobookTools>();
        var validAudiobookId = "7iHfbu1YPACw6oZPAFJtqe";
        var invalidOffset = -1;

        // Act
        var result = await audiobookTools.GetAudiobookChaptersAsync(validAudiobookId, "US", 20, invalidOffset);

        // Assert
        Assert.Equal("Offset must be non-negative.", result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetUserSavedAudiobooksAsync_EmptyAccessToken_ReturnsErrorMessage()
    {
        // Arrange
        var audiobookTools = _serviceProvider.GetRequiredService<AudiobookTools>();
        var emptyAccessToken = "";

        // Act
        var result = await audiobookTools.GetUserSavedAudiobooksAsync(emptyAccessToken);

        // Assert
        Assert.Equal("User access token is required for this operation.", result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetUserSavedAudiobooksAsync_InvalidLimit_ReturnsErrorMessage()
    {
        // Arrange
        var audiobookTools = _serviceProvider.GetRequiredService<AudiobookTools>();
        var invalidAccessToken = "invalid_token";
        var invalidLimit = 0;

        // Act
        var result = await audiobookTools.GetUserSavedAudiobooksAsync(invalidAccessToken, invalidLimit);

        // Assert
        Assert.Equal("Limit must be between 1 and 50.", result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task SaveAudiobooksForUserAsync_EmptyAccessToken_ReturnsErrorMessage()
    {
        // Arrange
        var audiobookTools = _serviceProvider.GetRequiredService<AudiobookTools>();
        var emptyAccessToken = "";
        var audiobookIds = "7iHfbu1YPACw6oZPAFJtqe";

        // Act
        var result = await audiobookTools.SaveAudiobooksForUserAsync(emptyAccessToken, audiobookIds);

        // Assert
        Assert.Equal("User access token is required for this operation.", result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task SaveAudiobooksForUserAsync_EmptyIds_ReturnsErrorMessage()
    {
        // Arrange
        var audiobookTools = _serviceProvider.GetRequiredService<AudiobookTools>();
        var invalidAccessToken = "invalid_token";
        var emptyIds = "";

        // Act
        var result = await audiobookTools.SaveAudiobooksForUserAsync(invalidAccessToken, emptyIds);

        // Assert
        Assert.Equal("No audiobook IDs provided.", result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task RemoveUserSavedAudiobooksAsync_EmptyAccessToken_ReturnsErrorMessage()
    {
        // Arrange
        var audiobookTools = _serviceProvider.GetRequiredService<AudiobookTools>();
        var emptyAccessToken = "";
        var audiobookIds = "7iHfbu1YPACw6oZPAFJtqe";

        // Act
        var result = await audiobookTools.RemoveUserSavedAudiobooksAsync(emptyAccessToken, audiobookIds);

        // Assert
        Assert.Equal("User access token is required for this operation.", result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task CheckUserSavedAudiobooksAsync_EmptyAccessToken_ReturnsErrorMessage()
    {
        // Arrange
        var audiobookTools = _serviceProvider.GetRequiredService<AudiobookTools>();
        var emptyAccessToken = "";
        var audiobookIds = "7iHfbu1YPACw6oZPAFJtqe";

        // Act
        var result = await audiobookTools.CheckUserSavedAudiobooksAsync(emptyAccessToken, audiobookIds);

        // Assert
        Assert.Equal("User access token is required for this operation.", result);
    }
}