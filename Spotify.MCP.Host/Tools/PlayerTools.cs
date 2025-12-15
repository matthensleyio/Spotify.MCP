using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Spotify.MCP.Host.Models;
using Spotify.MCP.Host.Models.Output;
using Spotify.MCP.Host.Services;
using System.ComponentModel;
using System.Text.Json;

namespace Spotify.MCP.Host.Tools;

[McpServerToolType]
public class PlayerTools
{
    private readonly ISpotifyApiService _spotifyApi;
    private readonly ILogger<PlayerTools> _logger;

    public PlayerTools(ISpotifyApiService spotifyApi, ILogger<PlayerTools> logger)
    {
        _spotifyApi = spotifyApi;
        _logger = logger;
    }

    [McpServerTool(Name = "get_playback", Title = "Get Playback")]
    [Description("Get the current playback state for the user")]
    public async Task<string> GetCurrentPlaybackAsync(
        [Description("User access token with user-read-playback-state scope")] string accessToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return JsonSerializer.Serialize(ErrorResponse.FromMessage("User access token is required for this operation.", "MISSING_ACCESS_TOKEN"));
            }

            var playbackState = await _spotifyApi.GetCurrentPlaybackAsync(accessToken);
            if (playbackState == null)
            {
                return JsonSerializer.Serialize(ErrorResponse.FromMessage("No active playback session found.", "NO_PLAYBACK_SESSION"));
            }

            return JsonSerializer.Serialize(playbackState, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current playback");
            return JsonSerializer.Serialize(ErrorResponse.FromException(ex, "GET_PLAYBACK_ERROR"));
        }
    }

    [McpServerTool(Name = "pause_playback", Title = "Pause Playback")]
    [Description("Pause the user's current playback")]
    public async Task<string> PausePlaybackAsync(
        [Description("User access token with user-modify-playback-state scope")] string accessToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return JsonSerializer.Serialize(ErrorResponse.FromMessage("User access token is required for this operation.", "MISSING_ACCESS_TOKEN"));
            }

            await _spotifyApi.PausePlaybackAsync(accessToken);
            return JsonSerializer.Serialize(new { success = true, message = "Playback paused successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing playback");
            return JsonSerializer.Serialize(ErrorResponse.FromException(ex, "PAUSE_PLAYBACK_ERROR"));
        }
    }

    [McpServerTool(Name = "start_playback", Title = "Start Playback")]
    [Description("Start or resume the user's playback")]
    public async Task<string> StartPlaybackAsync(
        [Description("User access token with user-modify-playback-state scope")] string accessToken,
        [Description("Optional context URI (album, artist, or playlist URI)")] string? contextUri = null,
        [Description("Optional comma-separated list of track URIs to play")] string? uris = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return JsonSerializer.Serialize(ErrorResponse.FromMessage("User access token is required for this operation.", "MISSING_ACCESS_TOKEN"));
            }

            string[]? uriArray = null;
            if (!string.IsNullOrWhiteSpace(uris))
            {
                uriArray = uris.Split(',', StringSplitOptions.RemoveEmptyEntries)
                              .Select(uri => uri.Trim())
                              .ToArray();
            }

            await _spotifyApi.StartPlaybackAsync(accessToken, contextUri, uriArray);
            return JsonSerializer.Serialize(new { success = true, message = "Playback started successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting playback");
            return JsonSerializer.Serialize(ErrorResponse.FromException(ex, "START_PLAYBACK_ERROR"));
        }
    }

    [McpServerTool(Name = "skip_next", Title = "Skip Next")]
    [Description("Skip to the next track in the user's queue")]
    public async Task<string> SkipToNextAsync(
        [Description("User access token with user-modify-playback-state scope")] string accessToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return JsonSerializer.Serialize(ErrorResponse.FromMessage("User access token is required for this operation.", "MISSING_ACCESS_TOKEN"));
            }

            await _spotifyApi.SkipToNextAsync(accessToken);
            return JsonSerializer.Serialize(new { success = true, message = "Skipped to next track successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error skipping to next track");
            return JsonSerializer.Serialize(ErrorResponse.FromException(ex, "SKIP_NEXT_ERROR"));
        }
    }

    [McpServerTool(Name = "skip_previous", Title = "Skip Previous")]
    [Description("Skip to the previous track in the user's queue")]
    public async Task<string> SkipToPreviousAsync(
        [Description("User access token with user-modify-playback-state scope")] string accessToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return JsonSerializer.Serialize(ErrorResponse.FromMessage("User access token is required for this operation.", "MISSING_ACCESS_TOKEN"));
            }

            await _spotifyApi.SkipToPreviousAsync(accessToken);
            return JsonSerializer.Serialize(new { success = true, message = "Skipped to previous track successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error skipping to previous track");
            return JsonSerializer.Serialize(ErrorResponse.FromException(ex, "SKIP_PREVIOUS_ERROR"));
        }
    }
}