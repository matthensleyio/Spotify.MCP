using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Spotify.MCP.Host.Models;
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

    [McpServerTool, Description("Get the current playback state for the user")]
    public async Task<string> GetCurrentPlaybackAsync(
        [Description("User access token with user-read-playback-state scope")] string accessToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(accessToken))
                return "User access token is required for this operation.";

            var playbackState = await _spotifyApi.GetCurrentPlaybackAsync(accessToken);
            if (playbackState == null)
                return "No active playback session found.";

            return JsonSerializer.Serialize(playbackState, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current playback");
            return $"Error retrieving current playback: {ex.Message}";
        }
    }

    [McpServerTool, Description("Pause the user's current playback")]
    public async Task<string> PausePlaybackAsync(
        [Description("User access token with user-modify-playback-state scope")] string accessToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(accessToken))
                return "User access token is required for this operation.";

            await _spotifyApi.PausePlaybackAsync(accessToken);
            return "Playback paused successfully.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing playback");
            return $"Error pausing playback: {ex.Message}";
        }
    }

    [McpServerTool, Description("Start or resume the user's playback")]
    public async Task<string> StartPlaybackAsync(
        [Description("User access token with user-modify-playback-state scope")] string accessToken,
        [Description("Optional context URI (album, artist, or playlist URI)")] string? contextUri = null,
        [Description("Optional comma-separated list of track URIs to play")] string? uris = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(accessToken))
                return "User access token is required for this operation.";

            string[]? uriArray = null;
            if (!string.IsNullOrWhiteSpace(uris))
            {
                uriArray = uris.Split(',', StringSplitOptions.RemoveEmptyEntries)
                              .Select(uri => uri.Trim())
                              .ToArray();
            }

            await _spotifyApi.StartPlaybackAsync(accessToken, contextUri, uriArray);
            return "Playback started successfully.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting playback");
            return $"Error starting playback: {ex.Message}";
        }
    }

    [McpServerTool, Description("Skip to the next track in the user's queue")]
    public async Task<string> SkipToNextAsync(
        [Description("User access token with user-modify-playback-state scope")] string accessToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(accessToken))
                return "User access token is required for this operation.";

            await _spotifyApi.SkipToNextAsync(accessToken);
            return "Skipped to next track successfully.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error skipping to next track");
            return $"Error skipping to next track: {ex.Message}";
        }
    }

    [McpServerTool, Description("Skip to the previous track in the user's queue")]
    public async Task<string> SkipToPreviousAsync(
        [Description("User access token with user-modify-playback-state scope")] string accessToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(accessToken))
                return "User access token is required for this operation.";

            await _spotifyApi.SkipToPreviousAsync(accessToken);
            return "Skipped to previous track successfully.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error skipping to previous track");
            return $"Error skipping to previous track: {ex.Message}";
        }
    }
}