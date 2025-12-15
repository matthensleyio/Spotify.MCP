using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Spotify.MCP.Host.Models;
using Spotify.MCP.Host.Models.Output;
using Spotify.MCP.Host.Services;
using System.ComponentModel;
using System.Text.Json;

namespace Spotify.MCP.Host.Tools;

[McpServerToolType]
public class UserTools
{
    private readonly ISpotifyApiService _spotifyApi;
    private readonly ILogger<UserTools> _logger;

    public UserTools(ISpotifyApiService spotifyApi, ILogger<UserTools> logger)
    {
        _spotifyApi = spotifyApi;
        _logger = logger;
    }

    [McpServerTool(Name = "get_current_user", Title = "Get Current User")]
    [Description("Get the current user's profile information")]
    public async Task<string> GetCurrentUserAsync(
        [Description("User access token with user-read-private and user-read-email scopes")] string accessToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return JsonSerializer.Serialize(ErrorResponse.FromMessage("User access token is required for this operation.", "MISSING_ACCESS_TOKEN"));
            }

            var user = await _spotifyApi.GetCurrentUserAsync(accessToken);
            if (user == null)
            {
                return JsonSerializer.Serialize(ErrorResponse.FromMessage("Unable to retrieve user profile.", "USER_PROFILE_NOT_FOUND"));
            }

            return JsonSerializer.Serialize(user, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user");
            return JsonSerializer.Serialize(ErrorResponse.FromException(ex, "GET_CURRENT_USER_ERROR"));
        }
    }
}