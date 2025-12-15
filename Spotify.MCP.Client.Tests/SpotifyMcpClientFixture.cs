using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using Microsoft.Extensions.Configuration;

namespace Spotify.MCP.Client.Tests;

/// <summary>
/// Fixture for managing MCP client connection to Spotify.MCP.Host server.
/// Handles setup, teardown, and provides convenient access to the client.
/// Loads Spotify credentials from UserSecrets and passes them to the server.
/// </summary>
public class SpotifyMcpClientFixture : IAsyncLifetime
{
    private McpClient? _client;
    private bool _disposed;

    public McpClient Client => _client ?? throw new InvalidOperationException("Client not initialized. Call InitializeAsync first.");

    public async Task InitializeAsync()
    {
        // Load configuration from UserSecrets
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<SpotifyMcpClientFixture>()
            .Build();

        // Get the path to the built Spotify.MCP.Host assembly
        var hostProjectPath = GetSpotifyHostPath();

        // Get Spotify credentials - required for server to function
        var clientId = configuration["Spotify:ClientId"];
        var clientSecret = configuration["Spotify:ClientSecret"];

        if (string.IsNullOrEmpty(clientId))
        {
            throw new InvalidOperationException(
                "Spotify ClientId not found in UserSecrets. " +
                "Configure it with: dotnet user-secrets set Spotify:ClientId <your_client_id> --project Spotify.MCP.Client.Tests");
        }

        if (string.IsNullOrEmpty(clientSecret))
        {
            throw new InvalidOperationException(
                "Spotify ClientSecret not found in UserSecrets. " +
                "Configure it with: dotnet user-secrets set Spotify:ClientSecret <your_client_secret> --project Spotify.MCP.Client.Tests");
        }

        // Build arguments for the server with credentials
        var arguments = new List<string>
        {
            "run", "--project", hostProjectPath, "--",
            "--spotify-client-id", clientId,
            "--spotify-client-secret", clientSecret
        };

        var clientTransport = new StdioClientTransport(new StdioClientTransportOptions
        {
            Name = "Spotify MCP Client Tests",
            Command = "dotnet",
            Arguments = arguments,
        });

        _client = await McpClient.CreateAsync(clientTransport);
    }

    public async Task DisposeAsync()
    {
        if (_disposed) return;

        if (_client != null)
        {
            await _client.DisposeAsync();
        }

        _disposed = true;
    }

    /// <summary>
    /// Gets the path to the Spotify.MCP.Host project for STDIO execution.
    /// Constructs the path relative to the test assembly location.
    /// </summary>
    private static string GetSpotifyHostPath()
    {
        // AppContext.BaseDirectory points to bin\Debug\net9.0 during test execution
        // Test assembly location: Spotify.MCP.Client.Tests\bin\Debug\net9.0
        // We need to navigate up to repo root (Spotify.MCP)
        var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
        var testAssemblyDir = Path.GetDirectoryName(assemblyLocation) ?? AppContext.BaseDirectory;

        // Navigate from Spotify.MCP.Client.Tests\bin\Debug\net9.0 up to repo root: ../../../../..
        var repoRoot = Path.Combine(testAssemblyDir, "..", "..", "..", "..");
        var hostProjectPath = Path.Combine(repoRoot, "Spotify.MCP.Host", "Spotify.MCP.Host.csproj");

        var fullPath = Path.GetFullPath(hostProjectPath);

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"Spotify.MCP.Host.csproj not found at: {fullPath}. Test assembly is at: {testAssemblyDir}");
        }

        return fullPath;
    }

    /// <summary>
    /// Helper method to call a tool and extract the text result.
    /// Handles both successful responses and error cases.
    /// </summary>
    public async Task<string?> CallToolAsync(string toolName, Dictionary<string, object?> arguments)
    {
        try
        {
            var result = await Client.CallToolAsync(toolName, arguments, progress: null);

            // Extract text from any text content blocks
            var textContent = result.Content.OfType<TextContentBlock>().FirstOrDefault()?.Text;

            if (textContent != null)
            {
                return textContent;
            }

            // If no text content found, try other content types
            if (result.Content.Count > 0)
            {
                // Log what we got instead
                var contentType = result.Content.First().GetType().Name;
                throw new InvalidOperationException(
                    $"Tool '{toolName}' returned unexpected content type: {contentType}. " +
                    $"Expected TextContentBlock but got {result.Content.Select(c => c.GetType().Name)}");
            }

            return null;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Error calling tool '{toolName}' with arguments {string.Join(", ", arguments.Select(kvp => $"{kvp.Key}={kvp.Value}"))}. " +
                $"Error: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Helper method to get all available tools.
    /// </summary>
    public async Task<IList<McpClientTool>> GetAllToolsAsync()
    {
        return await Client.ListToolsAsync();
    }
}
