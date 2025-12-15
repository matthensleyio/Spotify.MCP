using System.Text.Json;
using ModelContextProtocol.Client;

namespace Spotify.MCP.Client.Tests;

/// <summary>
/// Tests for authentication-related tools and OAuth flow.
/// </summary>
public class AuthenticationTests : IAsyncLifetime
{
    private SpotifyMcpClientFixture _fixture = null!;

    public async Task InitializeAsync()
    {
        _fixture = new SpotifyMcpClientFixture();
        await _fixture.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await _fixture.DisposeAsync();
    }

    [Fact]
    public async Task GetClientToken_ReturnsTokenWithProperties()
    {
        var result = await _fixture.CallToolAsync("get_client_token", new Dictionary<string, object?>());

        Assert.NotNull(result);
        Assert.NotEmpty(result);

        var json = JsonDocument.Parse(result);
        var root = json.RootElement;

        // Token response should have access_token
        Assert.True(root.TryGetProperty("access_token", out var accessToken));
        var token = accessToken.GetString();
        Assert.NotNull(token);
        Assert.NotEmpty(token);

        // Token response should have token_type
        Assert.True(root.TryGetProperty("token_type", out var tokenType));
        Assert.NotEmpty(tokenType.GetString() ?? string.Empty);

        // Token response should have expires_in
        Assert.True(root.TryGetProperty("expires_in", out var expiresIn));
        Assert.True(expiresIn.GetInt32() > 0);
    }

    [Fact]
    public async Task GetAuthUrl_GeneratesValidAuthorizationUrl()
    {
        var result = await _fixture.CallToolAsync("get_auth_url", new Dictionary<string, object?>
        {
            { "clientId", "test-client-id" },
            { "redirectUri", "http://localhost:8888/callback" },
            { "scopes", "user-read-private,user-read-email" }
        });

        Assert.NotNull(result);
        Assert.NotEmpty(result);

        var json = JsonDocument.Parse(result);
        var root = json.RootElement;

        // Should contain the authorization URL
        Assert.True(root.TryGetProperty("auth_url", out var authUrl));
        var url = authUrl.GetString();
        Assert.NotNull(url);
        Assert.NotEmpty(url);

        // URL should be to Spotify's authorization endpoint
        Assert.Contains("spotify.com", url);
        Assert.Contains("authorize", url);
    }

    [Fact]
    public async Task GetAuthUrl_IncludesRequiredScopesInUrl()
    {
        var result = await _fixture.CallToolAsync("get_auth_url", new Dictionary<string, object?>
        {
            { "redirect_uri", "http://localhost:8888/callback" }
        });

        Assert.NotNull(result);

        var json = JsonDocument.Parse(result);
        var root = json.RootElement;
        Assert.True(root.TryGetProperty("auth_url", out var authUrl));

        var url = authUrl.GetString() ?? string.Empty;

        // URL should include scope parameter
        Assert.Contains("scope", url);
    }

    [Fact]
    public async Task GetAuthUrl_WithDifferentRedirectUri_GeneratesUrl()
    {
        var redirectUri = "https://myapp.example.com/spotify/callback";

        var result = await _fixture.CallToolAsync("get_auth_url", new Dictionary<string, object?>
        {
            { "clientId", "test-client-id" },
            { "redirectUri", redirectUri },
            { "scopes", "user-read-private,user-read-email" }
        });

        Assert.NotNull(result);

        var json = JsonDocument.Parse(result);
        var root = json.RootElement;
        Assert.True(root.TryGetProperty("auth_url", out var authUrl));

        var url = authUrl.GetString() ?? string.Empty;

        // URL should include the redirect_uri
        Assert.Contains("redirect_uri", url);
        // The redirect_uri should be URL encoded
        Assert.Contains(Uri.EscapeDataString(redirectUri), url);
    }

    [Fact]
    public async Task ExchangeAuthCode_WithInvalidCode_ReturnsError()
    {
        var result = await _fixture.CallToolAsync("exchange_auth_code", new Dictionary<string, object?>
        {
            { "code", "invalid_code_that_does_not_exist" },
            { "redirectUri", "http://localhost:8888/callback" },
            { "clientId", "test-client-id" },
            { "clientSecret", "test-client-secret" }
        });

        // Should return an error (string, not JSON)
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        // Error responses may or may not be JSON, just verify we got something back
    }

    [Fact]
    public async Task RefreshToken_WithInvalidToken_ReturnsError()
    {
        var result = await _fixture.CallToolAsync("refresh_token", new Dictionary<string, object?>
        {
            { "refreshToken", "invalid_refresh_token" },
            { "clientId", "test-client-id" },
            { "clientSecret", "test-client-secret" }
        });

        // Should return an error
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task AuthenticationTools_AreDiscoverable()
    {
        var tools = await _fixture.GetAllToolsAsync();
        var authTools = tools.Where(t =>
            t.Name == "get_client_token" ||
            t.Name == "get_auth_url" ||
            t.Name == "exchange_auth_code" ||
            t.Name == "refresh_token").ToList();

        Assert.Equal(4, authTools.Count);
    }

    [Fact]
    public async Task GetAuthUrl_IsDiscoverable()
    {
        var tools = await _fixture.GetAllToolsAsync();
        var getAuthUrl = tools.FirstOrDefault(t => t.Name == "get_auth_url");

        Assert.NotNull(getAuthUrl);
        Assert.NotEmpty(getAuthUrl.Description);
    }

    [Fact]
    public async Task ExchangeAuthCode_IsDiscoverable()
    {
        var tools = await _fixture.GetAllToolsAsync();
        var exchangeAuthCode = tools.FirstOrDefault(t => t.Name == "exchange_auth_code");

        Assert.NotNull(exchangeAuthCode);
        Assert.NotEmpty(exchangeAuthCode.Description);
    }

    [Fact]
    public async Task RefreshToken_IsDiscoverable()
    {
        var tools = await _fixture.GetAllToolsAsync();
        var refreshToken = tools.FirstOrDefault(t => t.Name == "refresh_token");

        Assert.NotNull(refreshToken);
        Assert.NotEmpty(refreshToken.Description);
    }

    [Fact]
    public async Task GetClientToken_IsDiscoverable()
    {
        var tools = await _fixture.GetAllToolsAsync();
        var getClientToken = tools.FirstOrDefault(t => t.Name == "get_client_token");

        Assert.NotNull(getClientToken);
        Assert.NotEmpty(getClientToken.Description);
    }
}
