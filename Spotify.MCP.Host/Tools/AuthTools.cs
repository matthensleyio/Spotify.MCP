using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Spotify.MCP.Host.Models.Output;
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

    [McpServerTool(Name = "get_auth_url", Title = "Get Auth URL")]
    [Description("Generate a Spotify authorization URL for OAuth flow")]
    public async Task<string> GetAuthorizationUrlAsync(
        [Description("Spotify client ID")] string clientId,
        [Description("Redirect URI registered in your Spotify app")] string redirectUri,
        [Description("Comma-separated list of scopes (e.g., 'user-read-private,user-read-email,playlist-read-private')")] string scopes)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                return JsonSerializer.Serialize(ErrorResponse.FromMessage("Client ID is required."));
            }

            if (string.IsNullOrWhiteSpace(redirectUri))
            {
                return JsonSerializer.Serialize(ErrorResponse.FromMessage("Redirect URI is required."));
            }

            if (string.IsNullOrWhiteSpace(scopes))
            {
                return JsonSerializer.Serialize(ErrorResponse.FromMessage("At least one scope is required."));
            }

            var scopeArray = scopes.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                  .Select(s => s.Trim())
                                  .ToArray();

            var authUrl = await _authService.GetAuthorizationUrlAsync(clientId, redirectUri, scopeArray);
            return JsonSerializer.Serialize(new { auth_url = authUrl });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating authorization URL");
            return JsonSerializer.Serialize(ErrorResponse.FromException(ex, "AUTH_URL_ERROR"));
        }
    }

    [McpServerTool(Name = "exchange_auth_code", Title = "Exchange Auth Code")]
    [Description("Exchange authorization code for access tokens")]
    public async Task<string> ExchangeAuthorizationCodeAsync(
        [Description("Authorization code received from the callback")] string code,
        [Description("Redirect URI used in the authorization request")] string redirectUri,
        [Description("Spotify client ID")] string clientId,
        [Description("Spotify client secret")] string clientSecret)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return JsonSerializer.Serialize(ErrorResponse.FromMessage("Authorization code is required."));
            }

            if (string.IsNullOrWhiteSpace(redirectUri))
            {
                return JsonSerializer.Serialize(ErrorResponse.FromMessage("Redirect URI is required."));
            }

            if (string.IsNullOrWhiteSpace(clientId))
            {
                return JsonSerializer.Serialize(ErrorResponse.FromMessage("Client ID is required."));
            }

            if (string.IsNullOrWhiteSpace(clientSecret))
            {
                return JsonSerializer.Serialize(ErrorResponse.FromMessage("Client secret is required."));
            }

            var tokenResponse = await _authService.ExchangeAuthorizationCodeAsync(code, redirectUri, clientId, clientSecret);
            return JsonSerializer.Serialize(tokenResponse, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exchanging authorization code");
            return JsonSerializer.Serialize(ErrorResponse.FromException(ex, "EXCHANGE_AUTH_ERROR"));
        }
    }

    [McpServerTool(Name = "refresh_token", Title = "Refresh Token")]
    [Description("Refresh an expired access token using refresh token")]
    public async Task<string> RefreshAccessTokenAsync(
        [Description("Refresh token obtained during initial authorization")] string refreshToken,
        [Description("Spotify client ID")] string clientId,
        [Description("Spotify client secret")] string clientSecret)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return JsonSerializer.Serialize(ErrorResponse.FromMessage("Refresh token is required."));
            }

            if (string.IsNullOrWhiteSpace(clientId))
            {
                return JsonSerializer.Serialize(ErrorResponse.FromMessage("Client ID is required."));
            }

            if (string.IsNullOrWhiteSpace(clientSecret))
            {
                return JsonSerializer.Serialize(ErrorResponse.FromMessage("Client secret is required."));
            }

            var tokenResponse = await _authService.RefreshTokenAsync(refreshToken, clientId, clientSecret);
            return JsonSerializer.Serialize(tokenResponse, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing access token");
            return JsonSerializer.Serialize(ErrorResponse.FromException(ex, "REFRESH_TOKEN_ERROR"));
        }
    }

    [McpServerTool(Name = "get_client_token", Title = "Get Client Token")]
    [Description("Get an access token using client credentials flow (for public data only)")]
    public async Task<string> GetClientCredentialsTokenAsync()
    {
        try
        {
            var accessToken = await _authService.GetClientCredentialsTokenAsync();
            return JsonSerializer.Serialize(new { access_token = accessToken, token_type = "Bearer" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting client credentials token");
            return JsonSerializer.Serialize(ErrorResponse.FromException(ex, "CLIENT_TOKEN_ERROR"));
        }
    }
}