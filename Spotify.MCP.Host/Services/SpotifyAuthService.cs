using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace Spotify.MCP.Host.Services;

public interface ISpotifyAuthService
{
    Task<string> GetClientCredentialsTokenAsync();
    Task<string> GetAuthorizationUrlAsync(string clientId, string redirectUri, string[] scopes);
    Task<TokenResponse> ExchangeAuthorizationCodeAsync(string code, string redirectUri, string clientId, string clientSecret);
    Task<TokenResponse> RefreshTokenAsync(string refreshToken, string clientId, string clientSecret);
}

public class SpotifyAuthService : ISpotifyAuthService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private string? _cachedClientCredentialsToken;
    private DateTime _tokenExpiry;

    public SpotifyAuthService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<string> GetClientCredentialsTokenAsync()
    {
        if (_cachedClientCredentialsToken != null && DateTime.UtcNow < _tokenExpiry)
        {
            return _cachedClientCredentialsToken;
        }

        var clientId = _configuration["Spotify:ClientId"] ?? throw new InvalidOperationException("Spotify:ClientId not configured");
        var clientSecret = _configuration["Spotify:ClientSecret"] ?? throw new InvalidOperationException("Spotify:ClientSecret not configured");

        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));

        var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token")
        {
            Headers = { { "Authorization", $"Basic {credentials}" } },
            Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            })
        };

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        });

        if (tokenResponse?.AccessToken == null)
            throw new InvalidOperationException("Failed to obtain access token");

        _cachedClientCredentialsToken = tokenResponse.AccessToken;
        _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 60); // Refresh 1 minute early

        return tokenResponse.AccessToken;
    }

    public Task<string> GetAuthorizationUrlAsync(string clientId, string redirectUri, string[] scopes)
    {
        var scopeString = string.Join(" ", scopes);
        var state = Guid.NewGuid().ToString();
        
        var parameters = new Dictionary<string, string>
        {
            ["response_type"] = "code",
            ["client_id"] = clientId,
            ["scope"] = scopeString,
            ["redirect_uri"] = redirectUri,
            ["state"] = state
        };

        var queryString = string.Join("&", parameters.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
        return Task.FromResult($"https://accounts.spotify.com/authorize?{queryString}");
    }

    public async Task<TokenResponse> ExchangeAuthorizationCodeAsync(string code, string redirectUri, string clientId, string clientSecret)
    {
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));

        var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token")
        {
            Headers = { { "Authorization", $"Basic {credentials}" } },
            Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("redirect_uri", redirectUri)
            })
        };

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        });

        return tokenResponse ?? throw new InvalidOperationException("Failed to deserialize token response");
    }

    public async Task<TokenResponse> RefreshTokenAsync(string refreshToken, string clientId, string clientSecret)
    {
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));

        var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token")
        {
            Headers = { { "Authorization", $"Basic {credentials}" } },
            Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("refresh_token", refreshToken)
            })
        };

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        });

        return tokenResponse ?? throw new InvalidOperationException("Failed to deserialize token response");
    }
}

public record TokenResponse(
    string AccessToken,
    string TokenType,
    int ExpiresIn,
    string? RefreshToken,
    string? Scope
);