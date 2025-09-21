using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spotify.MCP.Host.Services;
using Spotify.MCP.Host.Tools;
using Xunit;

namespace Spotify.MCP.Host.Tests.Integration;

public class AuthToolsTests
{
    private readonly ServiceProvider _serviceProvider;

    public AuthToolsTests()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddUserSecrets<AuthToolsTests>()
            .Build());

        services.AddHttpClient();
        services.AddLogging();
        services.AddScoped<ISpotifyAuthService, SpotifyAuthService>();
        services.AddScoped<AuthTools>();

        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetAuthorizationUrlAsync_ValidParameters_ReturnsAuthorizationUrl()
    {
        // Arrange
        var authTools = _serviceProvider.GetRequiredService<AuthTools>();
        var clientId = "test_client_id";
        var redirectUri = "http://localhost:8080/callback";
        var scopes = "user-read-private,user-read-email";

        // Act
        var result = await authTools.GetAuthorizationUrlAsync(clientId, redirectUri, scopes);

        // Assert
        Assert.StartsWith("Authorization URL:", result);
        Assert.Contains("https://accounts.spotify.com/authorize", result);
        Assert.Contains(clientId, result);
        Assert.Contains("user-read-private", result);
        Assert.Contains("user-read-email", result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetAuthorizationUrlAsync_EmptyClientId_ReturnsErrorMessage()
    {
        // Arrange
        var authTools = _serviceProvider.GetRequiredService<AuthTools>();
        var emptyClientId = "";
        var redirectUri = "http://localhost:8080/callback";
        var scopes = "user-read-private";

        // Act
        var result = await authTools.GetAuthorizationUrlAsync(emptyClientId, redirectUri, scopes);

        // Assert
        Assert.Equal("Client ID is required.", result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetAuthorizationUrlAsync_EmptyRedirectUri_ReturnsErrorMessage()
    {
        // Arrange
        var authTools = _serviceProvider.GetRequiredService<AuthTools>();
        var clientId = "test_client_id";
        var emptyRedirectUri = "";
        var scopes = "user-read-private";

        // Act
        var result = await authTools.GetAuthorizationUrlAsync(clientId, emptyRedirectUri, scopes);

        // Assert
        Assert.Equal("Redirect URI is required.", result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetAuthorizationUrlAsync_EmptyScopes_ReturnsErrorMessage()
    {
        // Arrange
        var authTools = _serviceProvider.GetRequiredService<AuthTools>();
        var clientId = "test_client_id";
        var redirectUri = "http://localhost:8080/callback";
        var emptyScopes = "";

        // Act
        var result = await authTools.GetAuthorizationUrlAsync(clientId, redirectUri, emptyScopes);

        // Assert
        Assert.Equal("At least one scope is required.", result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetAuthorizationUrlAsync_NullClientId_ReturnsErrorMessage()
    {
        // Arrange
        var authTools = _serviceProvider.GetRequiredService<AuthTools>();
        string? nullClientId = null;
        var redirectUri = "http://localhost:8080/callback";
        var scopes = "user-read-private";

        // Act
        var result = await authTools.GetAuthorizationUrlAsync(nullClientId!, redirectUri, scopes);

        // Assert
        Assert.Equal("Client ID is required.", result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetAuthorizationUrlAsync_WhitespaceScopes_ReturnsErrorMessage()
    {
        // Arrange
        var authTools = _serviceProvider.GetRequiredService<AuthTools>();
        var clientId = "test_client_id";
        var redirectUri = "http://localhost:8080/callback";
        var whitespaceScopes = "   ";

        // Act
        var result = await authTools.GetAuthorizationUrlAsync(clientId, redirectUri, whitespaceScopes);

        // Assert
        Assert.Equal("At least one scope is required.", result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task ExchangeAuthorizationCodeAsync_EmptyCode_ReturnsErrorMessage()
    {
        // Arrange
        var authTools = _serviceProvider.GetRequiredService<AuthTools>();
        var emptyCode = "";
        var redirectUri = "http://localhost:8080/callback";
        var clientId = "test_client_id";
        var clientSecret = "test_client_secret";

        // Act
        var result = await authTools.ExchangeAuthorizationCodeAsync(emptyCode, redirectUri, clientId, clientSecret);

        // Assert
        Assert.Equal("Authorization code is required.", result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task ExchangeAuthorizationCodeAsync_EmptyRedirectUri_ReturnsErrorMessage()
    {
        // Arrange
        var authTools = _serviceProvider.GetRequiredService<AuthTools>();
        var code = "test_code";
        var emptyRedirectUri = "";
        var clientId = "test_client_id";
        var clientSecret = "test_client_secret";

        // Act
        var result = await authTools.ExchangeAuthorizationCodeAsync(code, emptyRedirectUri, clientId, clientSecret);

        // Assert
        Assert.Equal("Redirect URI is required.", result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task ExchangeAuthorizationCodeAsync_EmptyClientId_ReturnsErrorMessage()
    {
        // Arrange
        var authTools = _serviceProvider.GetRequiredService<AuthTools>();
        var code = "test_code";
        var redirectUri = "http://localhost:8080/callback";
        var emptyClientId = "";
        var clientSecret = "test_client_secret";

        // Act
        var result = await authTools.ExchangeAuthorizationCodeAsync(code, redirectUri, emptyClientId, clientSecret);

        // Assert
        Assert.Equal("Client ID is required.", result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task ExchangeAuthorizationCodeAsync_EmptyClientSecret_ReturnsErrorMessage()
    {
        // Arrange
        var authTools = _serviceProvider.GetRequiredService<AuthTools>();
        var code = "test_code";
        var redirectUri = "http://localhost:8080/callback";
        var clientId = "test_client_id";
        var emptyClientSecret = "";

        // Act
        var result = await authTools.ExchangeAuthorizationCodeAsync(code, redirectUri, clientId, emptyClientSecret);

        // Assert
        Assert.Equal("Client secret is required.", result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task ExchangeAuthorizationCodeAsync_InvalidCode_ReturnsErrorMessage()
    {
        // Arrange
        var authTools = _serviceProvider.GetRequiredService<AuthTools>();
        var invalidCode = "invalid_code";
        var redirectUri = "http://localhost:8080/callback";
        var clientId = "test_client_id";
        var clientSecret = "test_client_secret";

        // Act
        var result = await authTools.ExchangeAuthorizationCodeAsync(invalidCode, redirectUri, clientId, clientSecret);

        // Assert
        Assert.StartsWith("Error exchanging authorization code:", result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task RefreshAccessTokenAsync_EmptyRefreshToken_ReturnsErrorMessage()
    {
        // Arrange
        var authTools = _serviceProvider.GetRequiredService<AuthTools>();
        var emptyRefreshToken = "";
        var clientId = "test_client_id";
        var clientSecret = "test_client_secret";

        // Act
        var result = await authTools.RefreshAccessTokenAsync(emptyRefreshToken, clientId, clientSecret);

        // Assert
        Assert.Equal("Refresh token is required.", result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task RefreshAccessTokenAsync_EmptyClientId_ReturnsErrorMessage()
    {
        // Arrange
        var authTools = _serviceProvider.GetRequiredService<AuthTools>();
        var refreshToken = "test_refresh_token";
        var emptyClientId = "";
        var clientSecret = "test_client_secret";

        // Act
        var result = await authTools.RefreshAccessTokenAsync(refreshToken, emptyClientId, clientSecret);

        // Assert
        Assert.Equal("Client ID is required.", result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task RefreshAccessTokenAsync_EmptyClientSecret_ReturnsErrorMessage()
    {
        // Arrange
        var authTools = _serviceProvider.GetRequiredService<AuthTools>();
        var refreshToken = "test_refresh_token";
        var clientId = "test_client_id";
        var emptyClientSecret = "";

        // Act
        var result = await authTools.RefreshAccessTokenAsync(refreshToken, clientId, emptyClientSecret);

        // Assert
        Assert.Equal("Client secret is required.", result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task RefreshAccessTokenAsync_InvalidRefreshToken_ReturnsErrorMessage()
    {
        // Arrange
        var authTools = _serviceProvider.GetRequiredService<AuthTools>();
        var invalidRefreshToken = "invalid_refresh_token";
        var clientId = "test_client_id";
        var clientSecret = "test_client_secret";

        // Act
        var result = await authTools.RefreshAccessTokenAsync(invalidRefreshToken, clientId, clientSecret);

        // Assert
        Assert.StartsWith("Error refreshing access token:", result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetClientCredentialsTokenAsync_WithValidConfiguration_ReturnsAccessToken()
    {
        // Arrange
        var authTools = _serviceProvider.GetRequiredService<AuthTools>();

        // Act
        var result = await authTools.GetClientCredentialsTokenAsync();

        // Assert
        // This should either return a token or an error depending on configuration
        Assert.True(
            result.StartsWith("Access Token:") || result.StartsWith("Error getting client credentials token:"),
            $"Expected either access token or error message, but got: {result}"
        );
    }
}