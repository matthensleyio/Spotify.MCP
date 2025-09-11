using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Spotify.MCP.Host.Models;
using Spotify.MCP.Host.Services;
using System.ComponentModel;
using System.Text.Json;

namespace Spotify.MCP.Host.Tools;

[McpServerToolType]
public class ArtistTools
{
    private readonly ISpotifyApiService _spotifyApi;
    private readonly ILogger<ArtistTools> _logger;

    public ArtistTools(ISpotifyApiService spotifyApi, ILogger<ArtistTools> logger)
    {
        _spotifyApi = spotifyApi;
        _logger = logger;
    }

    [McpServerTool, Description("Get details about a specific artist by their Spotify ID")]
    public async Task<string> GetArtistAsync(
        [Description("Spotify artist ID")] string artistId,
        [Description("Optional access token for user-specific data")] string? accessToken = null)
    {
        try
        {
            var artist = await _spotifyApi.GetArtistAsync(artistId, accessToken);
            if (artist == null)
                return $"Artist with ID '{artistId}' not found.";

            return JsonSerializer.Serialize(artist, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting artist {ArtistId}", artistId);
            return $"Error retrieving artist: {ex.Message}";
        }
    }

    [McpServerTool, Description("Get all albums by a specific artist")]
    public async Task<string> GetArtistAlbumsAsync(
        [Description("Spotify artist ID")] string artistId,
        [Description("Optional access token for user-specific data")] string? accessToken = null)
    {
        try
        {
            var albums = await _spotifyApi.GetArtistAlbumsAsync(artistId, accessToken);
            return JsonSerializer.Serialize(albums, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting albums for artist {ArtistId}", artistId);
            return $"Error retrieving artist albums: {ex.Message}";
        }
    }

    [McpServerTool, Description("Get the top tracks for a specific artist")]
    public async Task<string> GetArtistTopTracksAsync(
        [Description("Spotify artist ID")] string artistId,
        [Description("Market/country code (e.g., 'US', 'GB', 'DE')")] string market = "US",
        [Description("Optional access token for user-specific data")] string? accessToken = null)
    {
        try
        {
            var topTracks = await _spotifyApi.GetArtistTopTracksAsync(artistId, market, accessToken);
            return JsonSerializer.Serialize(topTracks, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top tracks for artist {ArtistId}", artistId);
            return $"Error retrieving artist top tracks: {ex.Message}";
        }
    }
}