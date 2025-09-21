using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Spotify.MCP.Host.Models;
using Spotify.MCP.Host.Models.Output;
using Spotify.MCP.Host.Services;
using System.ComponentModel;
using System.Text.Json;

namespace Spotify.MCP.Host.Tools;

[McpServerToolType]
public class TrackTools
{
    private readonly ISpotifyApiService _spotifyApi;
    private readonly ILogger<TrackTools> _logger;

    public TrackTools(ISpotifyApiService spotifyApi, ILogger<TrackTools> logger)
    {
        _spotifyApi = spotifyApi;
        _logger = logger;
    }

    [McpServerTool(Name = "get_track", Title = "Get Track")]
    [Description("Get details about a specific track by its Spotify ID")]
    public async Task<string> GetTrackAsync(
        [Description("Spotify track ID")] string trackId,
        [Description("Optional access token for user-specific data")] string? accessToken = null)
    {
        try
        {
            var track = await _spotifyApi.GetTrackAsync(trackId, accessToken);
            if (track == null)
            {
                return $"Error retrieving track: Track with ID '{trackId}' not found.";
            }

            return JsonSerializer.Serialize(track, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting track {TrackId}", trackId);
            return $"Error retrieving track <{trackId}>: {ex.Message}";
        }
    }

    [McpServerTool(Name = "get_tracks", Title = "Get Tracks")]
    [Description("Get details about multiple tracks by their Spotify IDs")]
    public async Task<string> GetTracksAsync(
        [Description("Comma-separated list of Spotify track IDs")] string trackIds,
        [Description("Optional access token for user-specific data")] string? accessToken = null)
    {
        try
        {
            var ids = trackIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                              .Select(id => id.Trim())
                              .ToArray();

            if (ids.Length == 0)
            {
                return "No track IDs provided.";
            }

            if (ids.Length > 50)
            {
                return "Maximum of 50 track IDs allowed per request.";
            }

            var tracks = await _spotifyApi.GetTracksAsync(ids, accessToken);
            return JsonSerializer.Serialize(tracks, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tracks {TrackIds}", trackIds);
            return $"Error retrieving tracks <{trackIds}>: {ex.Message}";
        }
    }

    [McpServerTool(Name = "get_audio_features", Title = "Get Audio Features")]
    [Description("Get audio features for a track (acousticness, danceability, energy, etc.)")]
    public async Task<string> GetAudioFeaturesAsync(
        [Description("Spotify track ID")] string trackId,
        [Description("Optional access token for user-specific data")] string? accessToken = null)
    {
        try
        {
            var audioFeatures = await _spotifyApi.GetAudioFeaturesAsync(trackId, accessToken);
            if (audioFeatures == null)
            {
                return $"Error retrieving track: Audio features for track '{trackId}' not found.";
            }

            return JsonSerializer.Serialize(audioFeatures, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audio features for track {TrackId}", trackId);
            return $"Error retrieving track <{trackId}>: {ex.Message}";
        }
    }

    [McpServerTool(Name = "get_multiple_audio_features", Title = "Get Multiple Audio Features")]
    [Description("Get audio features for multiple tracks")]
    public async Task<string> GetMultipleAudioFeaturesAsync(
        [Description("Comma-separated list of Spotify track IDs")] string trackIds,
        [Description("Optional access token for user-specific data")] string? accessToken = null)
    {
        try
        {
            var ids = trackIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                              .Select(id => id.Trim())
                              .ToArray();

            if (ids.Length == 0)
            {
                return "No track IDs provided.";
            }

            if (ids.Length > 100)
            {
                return "Maximum of 100 track IDs allowed per request.";
            }

            var audioFeaturesTasks = ids.Select(id => _spotifyApi.GetAudioFeaturesAsync(id, accessToken));
            var audioFeaturesResults = await Task.WhenAll(audioFeaturesTasks);
            var audioFeatures = audioFeaturesResults.Where(af => af != null).ToList();

            return JsonSerializer.Serialize(audioFeatures, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audio features for tracks {TrackIds}", trackIds);
            return $"Error retrieving track audio features <{trackIds}>: {ex.Message}";
        }
    }
}