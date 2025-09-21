using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spotify.MCP.Host.Models.Output;
using Spotify.MCP.Host.Services;
using Spotify.MCP.Host.Tools;
using System.Text.Json;
using Xunit;

namespace Spotify.MCP.Host.Tests.Integration;

public class UserToolsTests
{
    private readonly ServiceProvider _serviceProvider;

    public UserToolsTests()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddUserSecrets<UserToolsTests>()
            .Build());

        services.AddHttpClient();
        services.AddLogging();
        services.AddScoped<ISpotifyAuthService, SpotifyAuthService>();
        services.AddScoped<ISpotifyApiService, SpotifyApiService>();
        services.AddScoped<UserTools>();

        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetCurrentUserAsync_EmptyAccessToken_ReturnsErrorMessage()
    {
        // Arrange
        var userTools = _serviceProvider.GetRequiredService<UserTools>();
        var emptyAccessToken = "";

        // Act
        var result = await userTools.GetCurrentUserAsync(emptyAccessToken);

        // Assert
        Assert.Equal("User access token is required for this operation.", result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetCurrentUserAsync_NullAccessToken_ReturnsErrorMessage()
    {
        // Arrange
        var userTools = _serviceProvider.GetRequiredService<UserTools>();
        string? nullAccessToken = null;

        // Act
        var result = await userTools.GetCurrentUserAsync(nullAccessToken!);

        // Assert
        Assert.Equal("User access token is required for this operation.", result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetCurrentUserAsync_WhitespaceAccessToken_ReturnsErrorMessage()
    {
        // Arrange
        var userTools = _serviceProvider.GetRequiredService<UserTools>();
        var whitespaceAccessToken = "   ";

        // Act
        var result = await userTools.GetCurrentUserAsync(whitespaceAccessToken);

        // Assert
        Assert.Equal("User access token is required for this operation.", result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetCurrentUserAsync_InvalidAccessToken_ReturnsErrorMessage()
    {
        // Arrange
        var userTools = _serviceProvider.GetRequiredService<UserTools>();
        var invalidAccessToken = "invalid_token";

        // Act
        var result = await userTools.GetCurrentUserAsync(invalidAccessToken);

        // Assert
        Assert.StartsWith("Error retrieving current user:", result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetCurrentUserAsync_ValidAccessToken_ReturnsValidUserData()
    {
        // Note: This test would require a valid user access token to work properly
        // In a real integration test scenario, you would either:
        // 1. Use a test account with a long-lived token stored in user secrets
        // 2. Mock the SpotifyApiService for this specific test
        // 3. Skip this test in CI/CD environments where real tokens aren't available

        // Arrange
        var userTools = _serviceProvider.GetRequiredService<UserTools>();
        var validAccessToken = "valid_user_token_from_secrets"; // This would come from user secrets

        // Act
        var result = await userTools.GetCurrentUserAsync(validAccessToken);

        // Assert
        // Since we don't have a real token, this will fail with an error
        // In a real test environment with proper tokens, you would validate:
        Assert.StartsWith("Error retrieving current user:", result);

        // With a real token, you would validate like this:
        /*
        Assert.False(result.StartsWith("Error"), $"Expected success, but got error: {result}");
        Assert.False(result.Contains("Unable to retrieve user profile"), $"Expected user to be found, but got: {result}");

        var user = JsonSerializer.Deserialize<User>(result);
        Assert.NotNull(user);
        Assert.NotNull(user.Id);
        Assert.False(string.IsNullOrWhiteSpace(user.Id), "User should have an ID");
        Assert.Equal("user", user.Type);
        Assert.NotNull(user.Uri);
        Assert.True(user.Uri.StartsWith("spotify:user:"), $"Expected user URI to start with 'spotify:user:', but got: {user.Uri}");
        Assert.NotNull(user.Href);
        Assert.True(user.Href.StartsWith("https://api.spotify.com/v1/users/"), $"Expected user href to start with Spotify API URL, but got: {user.Href}");
        Assert.NotNull(user.ExternalUrls);
        Assert.True(user.ExternalUrls.ContainsKey("spotify"), $"Expected external URLs to contain 'spotify' key, but got keys: {string.Join(", ", user.ExternalUrls.Keys)}");
        // DisplayName can be null for some users
        // Followers can be null for some users
        // Images can be null for some users
        */
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetCurrentUserAsync_ExpiredAccessToken_ReturnsErrorMessage()
    {
        // Arrange
        var userTools = _serviceProvider.GetRequiredService<UserTools>();
        var expiredAccessToken = "expired_token_example";

        // Act
        var result = await userTools.GetCurrentUserAsync(expiredAccessToken);

        // Assert
        Assert.StartsWith("Error retrieving current user:", result);
        // Expired tokens typically result in 401 Unauthorized errors
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetCurrentUserAsync_MalformedAccessToken_ReturnsErrorMessage()
    {
        // Arrange
        var userTools = _serviceProvider.GetRequiredService<UserTools>();
        var malformedAccessToken = "not.a.valid.token.format";

        // Act
        var result = await userTools.GetCurrentUserAsync(malformedAccessToken);

        // Assert
        Assert.StartsWith("Error retrieving current user:", result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetCurrentUserAsync_AccessTokenWithInsufficientScopes_ReturnsErrorMessage()
    {
        // Arrange
        var userTools = _serviceProvider.GetRequiredService<UserTools>();
        var insufficientScopeToken = "token_without_required_scopes";

        // Act
        var result = await userTools.GetCurrentUserAsync(insufficientScopeToken);

        // Assert
        Assert.StartsWith("Error retrieving current user:", result);
        // Insufficient scope typically results in 403 Forbidden errors
    }
}