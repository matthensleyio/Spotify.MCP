using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Spotify.MCP.Host.Models;
using Spotify.MCP.Host.Services;
using System.ComponentModel;
using System.Text.Json;

namespace Spotify.MCP.Host.Tools;

[McpServerToolType]
public class PlaylistTools
{
    private readonly ISpotifyApiService _spotifyApi;
    private readonly ILogger<PlaylistTools> _logger;

    public PlaylistTools(ISpotifyApiService spotifyApi, ILogger<PlaylistTools> logger)
    {
        _spotifyApi = spotifyApi;
        _logger = logger;
    }

    [McpServerTool(Name = "get_playlist", Title = "Get Playlist")]
    [Description("Get details about a specific playlist by its Spotify ID")]
    public async Task<string> GetPlaylistAsync(
        [Description("Spotify playlist ID")] string playlistId,
        [Description("Optional access token for user-specific data")] string? accessToken = null)
    {
        try
        {
            var playlist = await _spotifyApi.GetPlaylistAsync(playlistId, accessToken);
            if (playlist == null)
            {
                return $"Error retrieving playlist: Playlist with ID '{playlistId}' not found.";
            }

            return JsonSerializer.Serialize(playlist, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting playlist {PlaylistId}", playlistId);
            return $"Error retrieving playlist <{playlistId}>: {ex.Message}";
        }
    }

    [McpServerTool(Name = "get_user_playlists", Title = "Get User Playlists")]
    [Description("Get all playlists for the current user (requires user access token)")]
    public async Task<string> GetUserPlaylistsAsync(
        [Description("User access token with playlist-read-private and playlist-read-collaborative scopes")] string accessToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return "User access token is required for this operation.";
            }

            var playlists = await _spotifyApi.GetUserPlaylistsAsync(accessToken);
            return JsonSerializer.Serialize(playlists, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user playlists");
            return $"Error retrieving user playlists: {ex.Message}";
        }
    }
}