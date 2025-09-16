using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Spotify.MCP.Host.Models.Output;
using Spotify.MCP.Host.Models.Input;
using Spotify.MCP.Host.Services;
using System.ComponentModel;
using System.Text.Json;

namespace Spotify.MCP.Host.Tools;

[McpServerToolType]
public class SearchTools
{
    private readonly ISpotifyApiService _spotifyApi;
    private readonly ILogger<SearchTools> _logger;

    public SearchTools(ISpotifyApiService spotifyApi, ILogger<SearchTools> logger)
    {
        _spotifyApi = spotifyApi;
        _logger = logger;
    }

    [McpServerTool, Description("Search for tracks, albums, artists, playlists, or audiobooks on Spotify")]
    public async Task<string> SearchAsync(
        [Description("Search query string")] string query,
        [Description("Comma-separated list of item types to search for: track, album, artist, playlist, audiobook")] string types = "track,album,artist,playlist",
        [Description("Maximum number of results to return (1-50)")] int limit = 20,
        [Description("Index of the first result to return (for pagination)")] int offset = 0,
        [Description("Optional access token for user-specific data")] string? accessToken = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
                return "Search query cannot be empty.";

            if (limit < 1 || limit > 50)
                return "Limit must be between 1 and 50.";

            if (offset < 0)
                return "Offset must be non-negative.";

            var typeArray = types.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                 .Select(t => t.Trim().ToLowerInvariant())
                                 .ToArray();

            var validTypes = new[] { "track", "album", "artist", "playlist", "audiobook" };
            var invalidTypes = typeArray.Except(validTypes).ToArray();
            if (invalidTypes.Any())
                return $"Invalid search types: {string.Join(", ", invalidTypes)}. Valid types are: {string.Join(", ", validTypes)}";

            var searchResponse = await _spotifyApi.SearchAsync(query, typeArray, limit, offset, accessToken);
            return JsonSerializer.Serialize(searchResponse, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching for '{Query}'", query);
            return $"Error performing search: {ex.Message}";
        }
    }

    [McpServerTool, Description("Search specifically for tracks on Spotify")]
    public async Task<string> SearchTracksAsync(
        [Description("Search query string")] string query,
        [Description("Maximum number of results to return (1-50)")] int limit = 20,
        [Description("Index of the first result to return (for pagination)")] int offset = 0,
        [Description("Optional access token for user-specific data")] string? accessToken = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
                return "Search query cannot be empty.";

            if (limit < 1 || limit > 50)
                return "Limit must be between 1 and 50.";

            var searchResponse = await _spotifyApi.SearchAsync(query, new[] { "track" }, limit, offset, accessToken);
            return JsonSerializer.Serialize(searchResponse.Tracks?.Items ?? new List<Track>(), 
                new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching for tracks '{Query}'", query);
            return $"Error performing track search: {ex.Message}";
        }
    }

    [McpServerTool, Description("Search specifically for artists on Spotify")]
    public async Task<string> SearchArtistsAsync(
        [Description("Search query string")] string query,
        [Description("Maximum number of results to return (1-50)")] int limit = 20,
        [Description("Index of the first result to return (for pagination)")] int offset = 0,
        [Description("Optional access token for user-specific data")] string? accessToken = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
                return "Search query cannot be empty.";

            if (limit < 1 || limit > 50)
                return "Limit must be between 1 and 50.";

            var searchResponse = await _spotifyApi.SearchAsync(query, new[] { "artist" }, limit, offset, accessToken);
            return JsonSerializer.Serialize(searchResponse.Artists?.Items ?? new List<Artist>(), 
                new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching for artists '{Query}'", query);
            return $"Error performing artist search: {ex.Message}";
        }
    }

    [McpServerTool, Description("Search specifically for albums on Spotify")]
    public async Task<string> SearchAlbumsAsync(
        [Description("Search query string")] string query,
        [Description("Maximum number of results to return (1-50)")] int limit = 20,
        [Description("Index of the first result to return (for pagination)")] int offset = 0,
        [Description("Optional access token for user-specific data")] string? accessToken = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
                return "Search query cannot be empty.";

            if (limit < 1 || limit > 50)
                return "Limit must be between 1 and 50.";

            var searchResponse = await _spotifyApi.SearchAsync(query, new[] { "album" }, limit, offset, accessToken);
            return JsonSerializer.Serialize(searchResponse.Albums?.Items ?? new List<Album>(), 
                new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching for albums '{Query}'", query);
            return $"Error performing album search: {ex.Message}";
        }
    }

    [McpServerTool, Description("Search specifically for playlists on Spotify")]
    public async Task<string> SearchPlaylistsAsync(
        [Description("Search query string")] string query,
        [Description("Maximum number of results to return (1-50)")] int limit = 20,
        [Description("Index of the first result to return (for pagination)")] int offset = 0,
        [Description("Optional access token for user-specific data")] string? accessToken = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
                return "Search query cannot be empty.";

            if (limit < 1 || limit > 50)
                return "Limit must be between 1 and 50.";

            var searchResponse = await _spotifyApi.SearchAsync(query, new[] { "playlist" }, limit, offset, accessToken);
            return JsonSerializer.Serialize(searchResponse.Playlists?.Items ?? new List<Playlist>(), 
                new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching for playlists '{Query}'", query);
            return $"Error performing playlist search: {ex.Message}";
        }
    }

    [McpServerTool, Description("Search specifically for audiobooks on Spotify")]
    public async Task<string> SearchAudiobooksAsync(
        [Description("Search query string")] string query,
        [Description("Maximum number of results to return (1-50)")] int limit = 20,
        [Description("Index of the first result to return (for pagination)")] int offset = 0,
        [Description("Optional access token for user-specific data")] string? accessToken = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
                return "Search query cannot be empty.";

            if (limit < 1 || limit > 50)
                return "Limit must be between 1 and 50.";

            var searchResponse = await _spotifyApi.SearchAsync(query, new[] { "audiobook" }, limit, offset, accessToken);
            return JsonSerializer.Serialize(searchResponse.Audiobooks?.Items ?? new List<Audiobook>(), 
                new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching for audiobooks '{Query}'", query);
            return $"Error performing audiobook search: {ex.Message}";
        }
    }
}