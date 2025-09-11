using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Spotify.MCP.Host.Models;
using Spotify.MCP.Host.Services;
using System.ComponentModel;
using System.Text.Json;

namespace Spotify.MCP.Host.Tools;

[McpServerToolType]
public class AudiobookTools
{
    private readonly ISpotifyApiService _spotifyApi;
    private readonly ILogger<AudiobookTools> _logger;

    public AudiobookTools(ISpotifyApiService spotifyApi, ILogger<AudiobookTools> logger)
    {
        _spotifyApi = spotifyApi;
        _logger = logger;
    }

    [McpServerTool, Description("Get details about a specific audiobook by its Spotify ID")]
    public async Task<string> GetAudiobookAsync(
        [Description("Spotify audiobook ID")] string audiobookId,
        [Description("Market/country code (e.g., 'US', 'GB', 'CA')")] string market = "US",
        [Description("Optional access token for user-specific data")] string? accessToken = null)
    {
        try
        {
            var audiobook = await _spotifyApi.GetAudiobookAsync(audiobookId, market, accessToken);
            if (audiobook == null)
                return $"Audiobook with ID '{audiobookId}' not found in market '{market}'.";

            return JsonSerializer.Serialize(audiobook, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audiobook {AudiobookId}", audiobookId);
            return $"Error retrieving audiobook: {ex.Message}";
        }
    }

    [McpServerTool, Description("Get details about multiple audiobooks by their Spotify IDs")]
    public async Task<string> GetAudiobooksAsync(
        [Description("Comma-separated list of Spotify audiobook IDs")] string audiobookIds,
        [Description("Market/country code (e.g., 'US', 'GB', 'CA')")] string market = "US",
        [Description("Optional access token for user-specific data")] string? accessToken = null)
    {
        try
        {
            var ids = audiobookIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                  .Select(id => id.Trim())
                                  .ToArray();

            if (ids.Length == 0)
                return "No audiobook IDs provided.";

            if (ids.Length > 50)
                return "Maximum of 50 audiobook IDs allowed per request.";

            var audiobooks = await _spotifyApi.GetAudiobooksAsync(ids, market, accessToken);
            return JsonSerializer.Serialize(audiobooks, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audiobooks {AudiobookIds}", audiobookIds);
            return $"Error retrieving audiobooks: {ex.Message}";
        }
    }

    [McpServerTool, Description("Get all chapters from a specific audiobook")]
    public async Task<string> GetAudiobookChaptersAsync(
        [Description("Spotify audiobook ID")] string audiobookId,
        [Description("Market/country code (e.g., 'US', 'GB', 'CA')")] string market = "US",
        [Description("Maximum number of chapters to return (1-50)")] int limit = 20,
        [Description("Index of the first chapter to return (for pagination)")] int offset = 0,
        [Description("Optional access token for user-specific data")] string? accessToken = null)
    {
        try
        {
            if (limit < 1 || limit > 50)
                return "Limit must be between 1 and 50.";

            if (offset < 0)
                return "Offset must be non-negative.";

            var chapters = await _spotifyApi.GetAudiobookChaptersAsync(audiobookId, market, limit, offset, accessToken);
            return JsonSerializer.Serialize(chapters, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting chapters for audiobook {AudiobookId}", audiobookId);
            return $"Error retrieving audiobook chapters: {ex.Message}";
        }
    }

    [McpServerTool, Description("Get the current user's saved audiobooks (requires user access token)")]
    public async Task<string> GetUserSavedAudiobooksAsync(
        [Description("User access token with user-library-read scope")] string accessToken,
        [Description("Maximum number of audiobooks to return (1-50)")] int limit = 20,
        [Description("Index of the first audiobook to return (for pagination)")] int offset = 0)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(accessToken))
                return "User access token is required for this operation.";

            if (limit < 1 || limit > 50)
                return "Limit must be between 1 and 50.";

            if (offset < 0)
                return "Offset must be non-negative.";

            var audiobooks = await _spotifyApi.GetUserSavedAudiobooksAsync(accessToken, limit, offset);
            return JsonSerializer.Serialize(audiobooks, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user saved audiobooks");
            return $"Error retrieving user saved audiobooks: {ex.Message}";
        }
    }

    [McpServerTool, Description("Save audiobooks to the current user's library (requires user access token)")]
    public async Task<string> SaveAudiobooksForUserAsync(
        [Description("User access token with user-library-modify scope")] string accessToken,
        [Description("Comma-separated list of Spotify audiobook IDs to save")] string audiobookIds)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(accessToken))
                return "User access token is required for this operation.";

            var ids = audiobookIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                  .Select(id => id.Trim())
                                  .ToArray();

            if (ids.Length == 0)
                return "No audiobook IDs provided.";

            if (ids.Length > 50)
                return "Maximum of 50 audiobook IDs allowed per request.";

            await _spotifyApi.SaveAudiobooksForUserAsync(accessToken, ids);
            return $"Successfully saved {ids.Length} audiobook(s) to user's library.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving audiobooks for user");
            return $"Error saving audiobooks: {ex.Message}";
        }
    }

    [McpServerTool, Description("Remove audiobooks from the current user's library (requires user access token)")]
    public async Task<string> RemoveUserSavedAudiobooksAsync(
        [Description("User access token with user-library-modify scope")] string accessToken,
        [Description("Comma-separated list of Spotify audiobook IDs to remove")] string audiobookIds)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(accessToken))
                return "User access token is required for this operation.";

            var ids = audiobookIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                  .Select(id => id.Trim())
                                  .ToArray();

            if (ids.Length == 0)
                return "No audiobook IDs provided.";

            if (ids.Length > 50)
                return "Maximum of 50 audiobook IDs allowed per request.";

            await _spotifyApi.RemoveUserSavedAudiobooksAsync(accessToken, ids);
            return $"Successfully removed {ids.Length} audiobook(s) from user's library.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing audiobooks from user library");
            return $"Error removing audiobooks: {ex.Message}";
        }
    }

    [McpServerTool, Description("Check if audiobooks are saved in the current user's library (requires user access token)")]
    public async Task<string> CheckUserSavedAudiobooksAsync(
        [Description("User access token with user-library-read scope")] string accessToken,
        [Description("Comma-separated list of Spotify audiobook IDs to check")] string audiobookIds)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(accessToken))
                return "User access token is required for this operation.";

            var ids = audiobookIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                  .Select(id => id.Trim())
                                  .ToArray();

            if (ids.Length == 0)
                return "No audiobook IDs provided.";

            if (ids.Length > 50)
                return "Maximum of 50 audiobook IDs allowed per request.";

            var results = await _spotifyApi.CheckUserSavedAudiobooksAsync(accessToken, ids);
            
            var checkResults = ids.Zip(results, (id, saved) => new { AudiobookId = id, IsSaved = saved });
            return JsonSerializer.Serialize(checkResults, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking user saved audiobooks");
            return $"Error checking saved audiobooks: {ex.Message}";
        }
    }
}