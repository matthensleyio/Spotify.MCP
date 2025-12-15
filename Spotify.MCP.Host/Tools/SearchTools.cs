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

    [McpServerTool(Name = "search", Title = "Search")]
    [Description("Search for tracks, albums, artists, playlists, or audiobooks on Spotify")]
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
            {
                return JsonSerializer.Serialize(ErrorResponse.FromMessage("Search query cannot be empty.", "EMPTY_QUERY"));
            }

            if (limit < 1 || limit > 50)
            {
                return JsonSerializer.Serialize(ErrorResponse.FromMessage("Limit must be between 1 and 50.", "INVALID_LIMIT"));
            }

            if (offset < 0)
            {
                return JsonSerializer.Serialize(ErrorResponse.FromMessage("Offset must be non-negative.", "INVALID_OFFSET"));
            }

            var typeArray = types.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                 .Select(t => t.Trim().ToLowerInvariant())
                                 .ToArray();

            var validTypes = new[] { "track", "album", "artist", "playlist", "audiobook" };
            var invalidTypes = typeArray.Except(validTypes).ToArray();
            if (invalidTypes.Any())
            {
                return JsonSerializer.Serialize(ErrorResponse.FromMessage(
                    $"Invalid search types: {string.Join(", ", invalidTypes)}. Valid types are: {string.Join(", ", validTypes)}",
                    "INVALID_TYPES"));
            }

            var searchResponse = await _spotifyApi.SearchAsync(query, typeArray, limit, offset, accessToken);
            return JsonSerializer.Serialize(searchResponse, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing search for query '{Query}'", query);
            return JsonSerializer.Serialize(ErrorResponse.FromException(ex, "SEARCH_ERROR"));
        }
    }

    [McpServerTool(Name = "search_tracks", Title = "Search Tracks")]
    [Description("Search specifically for tracks on Spotify")]
    public async Task<string> SearchTracksAsync(
        [Description("Search query string")] string query,
        [Description("Maximum number of results to return (1-50)")] int limit = 20,
        [Description("Index of the first result to return (for pagination)")] int offset = 0,
        [Description("Optional access token for user-specific data")] string? accessToken = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return JsonSerializer.Serialize(ErrorResponse.FromMessage("Search query cannot be empty.", "EMPTY_QUERY"));
            }

            if (limit < 1 || limit > 50)
            {
                return JsonSerializer.Serialize(ErrorResponse.FromMessage("Limit must be between 1 and 50.", "INVALID_LIMIT"));
            }

            var searchResponse = await _spotifyApi.SearchAsync(query, new[] { "track" }, limit, offset, accessToken);
            var filteredTracks = (searchResponse.Tracks?.Items ?? new List<Track>()).Where(t => t != null).ToList();
            return JsonSerializer.Serialize(filteredTracks,
                new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching for tracks '{Query}'", query);
            return JsonSerializer.Serialize(ErrorResponse.FromException(ex, "SEARCH_TRACKS_ERROR"));
        }
    }

    [McpServerTool(Name = "search_artists", Title = "Search Artists")]
    [Description("Search specifically for artists on Spotify")]
    public async Task<string> SearchArtistsAsync(
        [Description("Search query string")] string query,
        [Description("Maximum number of results to return (1-50)")] int limit = 20,
        [Description("Index of the first result to return (for pagination)")] int offset = 0,
        [Description("Optional access token for user-specific data")] string? accessToken = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return JsonSerializer.Serialize(ErrorResponse.FromMessage("Search query cannot be empty.", "EMPTY_QUERY"));
            }

            if (limit < 1 || limit > 50)
            {
                return JsonSerializer.Serialize(ErrorResponse.FromMessage("Limit must be between 1 and 50.", "INVALID_LIMIT"));
            }

            var searchResponse = await _spotifyApi.SearchAsync(query, new[] { "artist" }, limit, offset, accessToken);
            var filteredArtists = (searchResponse.Artists?.Items ?? new List<Artist>()).Where(a => a != null).ToList();
            return JsonSerializer.Serialize(filteredArtists,
                new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching for artists '{Query}'", query);
            return JsonSerializer.Serialize(ErrorResponse.FromException(ex, "SEARCH_ARTISTS_ERROR"));
        }
    }

    [McpServerTool(Name = "search_albums", Title = "Search Albums")]
    [Description("Search specifically for albums on Spotify")]
    public async Task<string> SearchAlbumsAsync(
        [Description("Search query string")] string query,
        [Description("Maximum number of results to return (1-50)")] int limit = 20,
        [Description("Index of the first result to return (for pagination)")] int offset = 0,
        [Description("Optional access token for user-specific data")] string? accessToken = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return JsonSerializer.Serialize(ErrorResponse.FromMessage("Search query cannot be empty.", "EMPTY_QUERY"));
            }

            if (limit < 1 || limit > 50)
            {
                return JsonSerializer.Serialize(ErrorResponse.FromMessage("Limit must be between 1 and 50.", "INVALID_LIMIT"));
            }

            var searchResponse = await _spotifyApi.SearchAsync(query, new[] { "album" }, limit, offset, accessToken);
            var filteredAlbums = (searchResponse.Albums?.Items ?? new List<Album>()).Where(a => a != null).ToList();
            return JsonSerializer.Serialize(filteredAlbums,
                new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching for albums '{Query}'", query);
            return JsonSerializer.Serialize(ErrorResponse.FromException(ex, "SEARCH_ALBUMS_ERROR"));
        }
    }

    [McpServerTool(Name = "search_playlists", Title = "Search Playlists")]
    [Description("Search specifically for playlists on Spotify")]
    public async Task<string> SearchPlaylistsAsync(
        [Description("Search query string")] string query,
        [Description("Maximum number of results to return (1-50)")] int limit = 20,
        [Description("Index of the first result to return (for pagination)")] int offset = 0,
        [Description("Optional access token for user-specific data")] string? accessToken = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return JsonSerializer.Serialize(ErrorResponse.FromMessage("Search query cannot be empty.", "EMPTY_QUERY"));
            }

            if (limit < 1 || limit > 50)
            {
                return JsonSerializer.Serialize(ErrorResponse.FromMessage("Limit must be between 1 and 50.", "INVALID_LIMIT"));
            }

            var searchResponse = await _spotifyApi.SearchAsync(query, new[] { "playlist" }, limit, offset, accessToken);
            var filteredPlaylists = (searchResponse.Playlists?.Items ?? new List<Playlist>()).Where(p => p != null).ToList();
            return JsonSerializer.Serialize(filteredPlaylists,
                new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching for playlists '{Query}'", query);
            return JsonSerializer.Serialize(ErrorResponse.FromException(ex, "SEARCH_PLAYLISTS_ERROR"));
        }
    }

    [McpServerTool(Name = "search_audiobooks", Title = "Search Audiobooks")]
    [Description("Search specifically for audiobooks on Spotify")]
    public async Task<string> SearchAudiobooksAsync(
        [Description("Search query string")] string query,
        [Description("Maximum number of results to return (1-50)")] int limit = 20,
        [Description("Index of the first result to return (for pagination)")] int offset = 0,
        [Description("Optional access token for user-specific data")] string? accessToken = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return JsonSerializer.Serialize(ErrorResponse.FromMessage("Search query cannot be empty.", "EMPTY_QUERY"));
            }

            if (limit < 1 || limit > 50)
            {
                return JsonSerializer.Serialize(ErrorResponse.FromMessage("Limit must be between 1 and 50.", "INVALID_LIMIT"));
            }

            var searchResponse = await _spotifyApi.SearchAsync(query, new[] { "audiobook" }, limit, offset, accessToken);
            var filteredAudiobooks = (searchResponse.Audiobooks?.Items ?? new List<Audiobook>()).Where(a => a != null).ToList();
            return JsonSerializer.Serialize(filteredAudiobooks,
                new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching for audiobooks '{Query}'", query);
            return JsonSerializer.Serialize(ErrorResponse.FromException(ex, "SEARCH_AUDIOBOOKS_ERROR"));
        }
    }
}