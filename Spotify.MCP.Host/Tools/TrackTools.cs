using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
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
                return JsonSerializer.Serialize(ErrorResponse.FromMessage($"Track with ID '{trackId}' not found.", "TRACK_NOT_FOUND"));
            }

            return JsonSerializer.Serialize(track, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting track {TrackId}", trackId);
            return JsonSerializer.Serialize(ErrorResponse.FromException(ex, "GET_TRACK_ERROR"));
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
                return JsonSerializer.Serialize(ErrorResponse.FromMessage("No track IDs provided.", "EMPTY_TRACK_IDS"));
            }

            if (ids.Length > 50)
            {
                return JsonSerializer.Serialize(ErrorResponse.FromMessage("Maximum of 50 track IDs allowed per request.", "TOO_MANY_TRACK_IDS"));
            }

            var tracks = await _spotifyApi.GetTracksAsync(ids, accessToken);
            return JsonSerializer.Serialize(tracks, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tracks {TrackIds}", trackIds);
            return JsonSerializer.Serialize(ErrorResponse.FromException(ex, "GET_TRACKS_ERROR"));
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
                return JsonSerializer.Serialize(ErrorResponse.FromMessage($"Audio features for track '{trackId}' not found.", "AUDIO_FEATURES_NOT_FOUND"));
            }

            return JsonSerializer.Serialize(audioFeatures, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audio features for track {TrackId}", trackId);
            return JsonSerializer.Serialize(ErrorResponse.FromException(ex, "GET_AUDIO_FEATURES_ERROR"));
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
                return JsonSerializer.Serialize(ErrorResponse.FromMessage("No track IDs provided.", "EMPTY_TRACK_IDS"));
            }

            if (ids.Length > 100)
            {
                return JsonSerializer.Serialize(ErrorResponse.FromMessage("Maximum of 100 track IDs allowed per request.", "TOO_MANY_TRACK_IDS"));
            }

            var audioFeaturesTasks = ids.Select(id => _spotifyApi.GetAudioFeaturesAsync(id, accessToken));
            var audioFeaturesResults = await Task.WhenAll(audioFeaturesTasks);
            var audioFeatures = audioFeaturesResults.Where(af => af != null).ToList();

            return JsonSerializer.Serialize(audioFeatures, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audio features for tracks {TrackIds}", trackIds);
            return JsonSerializer.Serialize(ErrorResponse.FromException(ex, "GET_MULTIPLE_AUDIO_FEATURES_ERROR"));
        }
    }
}