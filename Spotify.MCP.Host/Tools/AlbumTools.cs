using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Spotify.MCP.Host.Models;
using Spotify.MCP.Host.Services;
using System.ComponentModel;
using System.Text.Json;

namespace Spotify.MCP.Host.Tools;

[McpServerToolType]
public class AlbumTools
{
    private readonly ISpotifyApiService _spotifyApi;
    private readonly ILogger<AlbumTools> _logger;

    public AlbumTools(ISpotifyApiService spotifyApi, ILogger<AlbumTools> logger)
    {
        _spotifyApi = spotifyApi;
        _logger = logger;
    }

    [McpServerTool, Description("Get details about a specific album by its Spotify ID")]
    public async Task<string> GetAlbumAsync(
        [Description("Spotify album ID")] string albumId,
        [Description("Optional access token for user-specific data")] string? accessToken = null)
    {
        try
        {
            var album = await _spotifyApi.GetAlbumAsync(albumId, accessToken);
            if (album == null)
                return $"Album with ID '{albumId}' not found.";

            return JsonSerializer.Serialize(album, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting album {AlbumId}", albumId);
            return $"Error retrieving album: {ex.Message}";
        }
    }

    [McpServerTool, Description("Get all tracks from a specific album")]
    public async Task<string> GetAlbumTracksAsync(
        [Description("Spotify album ID")] string albumId,
        [Description("Optional access token for user-specific data")] string? accessToken = null)
    {
        try
        {
            var tracks = await _spotifyApi.GetAlbumTracksAsync(albumId, accessToken);
            return JsonSerializer.Serialize(tracks, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tracks for album {AlbumId}", albumId);
            return $"Error retrieving album tracks: {ex.Message}";
        }
    }
}