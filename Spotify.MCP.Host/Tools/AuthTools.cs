using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Spotify.MCP.Host.Services;
using System.ComponentModel;
using System.Text.Json;

namespace Spotify.MCP.Host.Tools;

[McpServerToolType]
public class AuthTools
{
    private readonly ISpotifyAuthService _authService;
    private readonly ILogger<AuthTools> _logger;

    public AuthTools(ISpotifyAuthService authService, ILogger<AuthTools> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [McpServerTool, Description("Generate a Spotify authorization URL for OAuth flow")]
    public async Task<string> GetAuthorizationUrlAsync(
        [Description("Spotify client ID")] string clientId,
        [Description("Redirect URI registered in your Spotify app")] string redirectUri,
        [Description("Comma-separated list of scopes (e.g., 'user-read-private,user-read-email,playlist-read-private')")] string scopes)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(clientId))
                return "Client ID is required.";

            if (string.IsNullOrWhiteSpace(redirectUri))
                return "Redirect URI is required.";

            if (string.IsNullOrWhiteSpace(scopes))
                return "At least one scope is required.";

            var scopeArray = scopes.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                  .Select(s => s.Trim())
                                  .ToArray();

            var authUrl = await _authService.GetAuthorizationUrlAsync(clientId, redirectUri, scopeArray);
            return $"Authorization URL: {authUrl}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating authorization URL");
            return $"Error generating authorization URL: {ex.Message}";
        }
    }

    [McpServerTool, Description("Exchange authorization code for access tokens")]
    public async Task<string> ExchangeAuthorizationCodeAsync(
        [Description("Authorization code received from the callback")] string code,
        [Description("Redirect URI used in the authorization request")] string redirectUri,
        [Description("Spotify client ID")] string clientId,
        [Description("Spotify client secret")] string clientSecret)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(code))
                return "Authorization code is required.";

            if (string.IsNullOrWhiteSpace(redirectUri))
                return "Redirect URI is required.";

            if (string.IsNullOrWhiteSpace(clientId))
                return "Client ID is required.";

            if (string.IsNullOrWhiteSpace(clientSecret))
                return "Client secret is required.";

            var tokenResponse = await _authService.ExchangeAuthorizationCodeAsync(code, redirectUri, clientId, clientSecret);
            return JsonSerializer.Serialize(tokenResponse, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exchanging authorization code");
            return $"Error exchanging authorization code: {ex.Message}";
        }
    }

    [McpServerTool, Description("Refresh an expired access token using refresh token")]
    public async Task<string> RefreshAccessTokenAsync(
        [Description("Refresh token obtained during initial authorization")] string refreshToken,
        [Description("Spotify client ID")] string clientId,
        [Description("Spotify client secret")] string clientSecret)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return "Refresh token is required.";

            if (string.IsNullOrWhiteSpace(clientId))
                return "Client ID is required.";

            if (string.IsNullOrWhiteSpace(clientSecret))
                return "Client secret is required.";

            var tokenResponse = await _authService.RefreshTokenAsync(refreshToken, clientId, clientSecret);
            return JsonSerializer.Serialize(tokenResponse, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing access token");
            return $"Error refreshing access token: {ex.Message}";
        }
    }

    [McpServerTool, Description("Get an access token using client credentials flow (for public data only)")]
    public async Task<string> GetClientCredentialsTokenAsync()
    {
        try
        {
            var accessToken = await _authService.GetClientCredentialsTokenAsync();
            return $"Access Token: {accessToken}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting client credentials token");
            return $"Error getting client credentials token: {ex.Message}";
        }
    }
}