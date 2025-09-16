using System.Text.Json;
using System.Text.Json.Serialization;
using Spotify.MCP.Host.Models.Output;
using Spotify.MCP.Host.Models.Input;

namespace Spotify.MCP.Host.Services;

public interface ISpotifyApiService
{
    Task<Track?> GetTrackAsync(string trackId, string? accessToken = null);
    Task<List<Track>> GetTracksAsync(string[] trackIds, string? accessToken = null);
    Task<AudioFeatures?> GetAudioFeaturesAsync(string trackId, string? accessToken = null);
    Task<Artist?> GetArtistAsync(string artistId, string? accessToken = null);
    Task<List<Album>> GetArtistAlbumsAsync(string artistId, string? accessToken = null);
    Task<List<Track>> GetArtistTopTracksAsync(string artistId, string market = "US", string? accessToken = null);
    Task<Album?> GetAlbumAsync(string albumId, string? accessToken = null);
    Task<List<Track>> GetAlbumTracksAsync(string albumId, string? accessToken = null);
    Task<SearchResponse> SearchAsync(string query, string[] types, int limit = 20, int offset = 0, string? accessToken = null);
    Task<User?> GetCurrentUserAsync(string accessToken);
    Task<List<Playlist>> GetUserPlaylistsAsync(string accessToken);
    Task<Playlist?> GetPlaylistAsync(string playlistId, string? accessToken = null);
    Task<PlaybackState?> GetCurrentPlaybackAsync(string accessToken);
    Task PausePlaybackAsync(string accessToken);
    Task StartPlaybackAsync(string accessToken, string? contextUri = null, string[]? uris = null);
    Task SkipToNextAsync(string accessToken);
    Task SkipToPreviousAsync(string accessToken);
    Task<Audiobook?> GetAudiobookAsync(string audiobookId, string market = "US", string? accessToken = null);
    Task<List<Audiobook>> GetAudiobooksAsync(string[] audiobookIds, string market = "US", string? accessToken = null);
    Task<List<Chapter>> GetAudiobookChaptersAsync(string audiobookId, string market = "US", int limit = 20, int offset = 0, string? accessToken = null);
    Task<List<Audiobook>> GetUserSavedAudiobooksAsync(string accessToken, int limit = 20, int offset = 0);
    Task SaveAudiobooksForUserAsync(string accessToken, string[] audiobookIds);
    Task RemoveUserSavedAudiobooksAsync(string accessToken, string[] audiobookIds);
    Task<List<bool>> CheckUserSavedAudiobooksAsync(string accessToken, string[] audiobookIds);
}

public class SpotifyApiService : ISpotifyApiService
{
    private readonly HttpClient _httpClient;
    private readonly ISpotifyAuthService _authService;
    private readonly JsonSerializerOptions _jsonOptions;

    public SpotifyApiService(HttpClient httpClient, ISpotifyAuthService authService)
    {
        _httpClient = httpClient;
        _authService = authService;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<Track?> GetTrackAsync(string trackId, string? accessToken = null)
    {
        var token = accessToken ?? await _authService.GetClientCredentialsTokenAsync();
        var request = CreateRequest(HttpMethod.Get, $"https://api.spotify.com/v1/tracks/{trackId}", token);
        
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Track>(content, _jsonOptions);
    }

    public async Task<List<Track>> GetTracksAsync(string[] trackIds, string? accessToken = null)
    {
        var token = accessToken ?? await _authService.GetClientCredentialsTokenAsync();
        var ids = string.Join(",", trackIds);
        var request = CreateRequest(HttpMethod.Get, $"https://api.spotify.com/v1/tracks?ids={ids}", token);
        
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<TracksResponse>(content, _jsonOptions);
        return result?.Tracks ?? new List<Track>();
    }

    public async Task<AudioFeatures?> GetAudioFeaturesAsync(string trackId, string? accessToken = null)
    {
        var token = accessToken ?? await _authService.GetClientCredentialsTokenAsync();
        var request = CreateRequest(HttpMethod.Get, $"https://api.spotify.com/v1/audio-features/{trackId}", token);
        
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<AudioFeatures>(content, _jsonOptions);
    }

    public async Task<Artist?> GetArtistAsync(string artistId, string? accessToken = null)
    {
        var token = accessToken ?? await _authService.GetClientCredentialsTokenAsync();
        var request = CreateRequest(HttpMethod.Get, $"https://api.spotify.com/v1/artists/{artistId}", token);
        
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Artist>(content, _jsonOptions);
    }

    public async Task<List<Album>> GetArtistAlbumsAsync(string artistId, string? accessToken = null)
    {
        var token = accessToken ?? await _authService.GetClientCredentialsTokenAsync();
        var request = CreateRequest(HttpMethod.Get, $"https://api.spotify.com/v1/artists/{artistId}/albums", token);
        
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<AlbumsResponse>(content, _jsonOptions);
        return result?.Items ?? new List<Album>();
    }

    public async Task<List<Track>> GetArtistTopTracksAsync(string artistId, string market = "US", string? accessToken = null)
    {
        var token = accessToken ?? await _authService.GetClientCredentialsTokenAsync();
        var request = CreateRequest(HttpMethod.Get, $"https://api.spotify.com/v1/artists/{artistId}/top-tracks?market={market}", token);
        
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<TopTracksResponse>(content, _jsonOptions);
        return result?.Tracks ?? new List<Track>();
    }

    public async Task<Album?> GetAlbumAsync(string albumId, string? accessToken = null)
    {
        var token = accessToken ?? await _authService.GetClientCredentialsTokenAsync();
        var request = CreateRequest(HttpMethod.Get, $"https://api.spotify.com/v1/albums/{albumId}", token);
        
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Album>(content, _jsonOptions);
    }

    public async Task<List<Track>> GetAlbumTracksAsync(string albumId, string? accessToken = null)
    {
        var token = accessToken ?? await _authService.GetClientCredentialsTokenAsync();
        var request = CreateRequest(HttpMethod.Get, $"https://api.spotify.com/v1/albums/{albumId}/tracks", token);
        
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<TracksResponse>(content, _jsonOptions);
        return result?.Items ?? new List<Track>();
    }

    public async Task<SearchResponse> SearchAsync(string query, string[] types, int limit = 20, int offset = 0, string? accessToken = null)
    {
        var token = accessToken ?? await _authService.GetClientCredentialsTokenAsync();
        var typeString = string.Join(",", types);
        var encodedQuery = Uri.EscapeDataString(query);
        var request = CreateRequest(HttpMethod.Get, $"https://api.spotify.com/v1/search?q={encodedQuery}&type={typeString}&limit={limit}&offset={offset}", token);
        
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<SearchResponse>(content, _jsonOptions) ?? new SearchResponse(null, null, null, null, null);
    }

    public async Task<User?> GetCurrentUserAsync(string accessToken)
    {
        var request = CreateRequest(HttpMethod.Get, "https://api.spotify.com/v1/me", accessToken);
        
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<User>(content, _jsonOptions);
    }

    public async Task<List<Playlist>> GetUserPlaylistsAsync(string accessToken)
    {
        var request = CreateRequest(HttpMethod.Get, "https://api.spotify.com/v1/me/playlists", accessToken);
        
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PlaylistsResponse>(content, _jsonOptions);
        return result?.Items ?? new List<Playlist>();
    }

    public async Task<Playlist?> GetPlaylistAsync(string playlistId, string? accessToken = null)
    {
        var token = accessToken ?? await _authService.GetClientCredentialsTokenAsync();
        var request = CreateRequest(HttpMethod.Get, $"https://api.spotify.com/v1/playlists/{playlistId}", token);
        
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Playlist>(content, _jsonOptions);
    }

    public async Task<PlaybackState?> GetCurrentPlaybackAsync(string accessToken)
    {
        var request = CreateRequest(HttpMethod.Get, "https://api.spotify.com/v1/me/player", accessToken);
        
        var response = await _httpClient.SendAsync(request);
        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            return null;
        
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<PlaybackState>(content, _jsonOptions);
    }

    public async Task PausePlaybackAsync(string accessToken)
    {
        var request = CreateRequest(HttpMethod.Put, "https://api.spotify.com/v1/me/player/pause", accessToken);
        
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public async Task StartPlaybackAsync(string accessToken, string? contextUri = null, string[]? uris = null)
    {
        var request = CreateRequest(HttpMethod.Put, "https://api.spotify.com/v1/me/player/play", accessToken);
        
        if (contextUri != null || uris != null)
        {
            var body = new Dictionary<string, object?>();
            if (contextUri != null) body["context_uri"] = contextUri;
            if (uris != null) body["uris"] = uris;
            
            var json = JsonSerializer.Serialize(body, _jsonOptions);
            request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        }
        
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public async Task SkipToNextAsync(string accessToken)
    {
        var request = CreateRequest(HttpMethod.Post, "https://api.spotify.com/v1/me/player/next", accessToken);
        
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public async Task SkipToPreviousAsync(string accessToken)
    {
        var request = CreateRequest(HttpMethod.Post, "https://api.spotify.com/v1/me/player/previous", accessToken);
        
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public async Task<Audiobook?> GetAudiobookAsync(string audiobookId, string market = "US", string? accessToken = null)
    {
        var token = accessToken ?? await _authService.GetClientCredentialsTokenAsync();
        var request = CreateRequest(HttpMethod.Get, $"https://api.spotify.com/v1/audiobooks/{audiobookId}?market={market}", token);
        
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Audiobook>(content, _jsonOptions);
    }

    public async Task<List<Audiobook>> GetAudiobooksAsync(string[] audiobookIds, string market = "US", string? accessToken = null)
    {
        var token = accessToken ?? await _authService.GetClientCredentialsTokenAsync();
        var ids = string.Join(",", audiobookIds);
        var request = CreateRequest(HttpMethod.Get, $"https://api.spotify.com/v1/audiobooks?ids={ids}&market={market}", token);
        
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<AudiobooksResponse>(content, _jsonOptions);
        return result?.Audiobooks ?? new List<Audiobook>();
    }

    public async Task<List<Chapter>> GetAudiobookChaptersAsync(string audiobookId, string market = "US", int limit = 20, int offset = 0, string? accessToken = null)
    {
        var token = accessToken ?? await _authService.GetClientCredentialsTokenAsync();
        var request = CreateRequest(HttpMethod.Get, $"https://api.spotify.com/v1/audiobooks/{audiobookId}/chapters?market={market}&limit={limit}&offset={offset}", token);
        
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ChaptersResponse>(content, _jsonOptions);
        return result?.Items ?? new List<Chapter>();
    }

    public async Task<List<Audiobook>> GetUserSavedAudiobooksAsync(string accessToken, int limit = 20, int offset = 0)
    {
        var request = CreateRequest(HttpMethod.Get, $"https://api.spotify.com/v1/me/audiobooks?limit={limit}&offset={offset}", accessToken);
        
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<SavedAudiobooksResponse>(content, _jsonOptions);
        return result?.Items?.Select(item => item.Audiobook).ToList() ?? new List<Audiobook>();
    }

    public async Task SaveAudiobooksForUserAsync(string accessToken, string[] audiobookIds)
    {
        var ids = string.Join(",", audiobookIds);
        var request = CreateRequest(HttpMethod.Put, $"https://api.spotify.com/v1/me/audiobooks?ids={ids}", accessToken);
        
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public async Task RemoveUserSavedAudiobooksAsync(string accessToken, string[] audiobookIds)
    {
        var ids = string.Join(",", audiobookIds);
        var request = CreateRequest(HttpMethod.Delete, $"https://api.spotify.com/v1/me/audiobooks?ids={ids}", accessToken);
        
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<bool>> CheckUserSavedAudiobooksAsync(string accessToken, string[] audiobookIds)
    {
        var ids = string.Join(",", audiobookIds);
        var request = CreateRequest(HttpMethod.Get, $"https://api.spotify.com/v1/me/audiobooks/contains?ids={ids}", accessToken);
        
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<bool>>(content, _jsonOptions) ?? new List<bool>();
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string url, string accessToken)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Add("Authorization", $"Bearer {accessToken}");
        return request;
    }
}

internal record TracksResponse(List<Track> Tracks, List<Track> Items);
internal record AlbumsResponse(List<Album> Items);
internal record TopTracksResponse(List<Track> Tracks);
internal record PlaylistsResponse(List<Playlist> Items);
internal record AudiobooksResponse(List<Audiobook> Audiobooks);
internal record ChaptersResponse(List<Chapter> Items);
internal record SavedAudiobooksResponse(List<SavedAudiobookItem> Items);
internal record SavedAudiobookItem(Audiobook Audiobook, [property: JsonPropertyName("added_at")] DateTime AddedAt);