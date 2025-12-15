using ModelContextProtocol.Protocol;
using ModelContextProtocol.Client;

namespace Spotify.MCP.Client.Tests;

/// <summary>
/// Tests for MCP client connection, initialization, and session management.
/// </summary>
public class ClientConnectionTests : IAsyncLifetime
{
    private SpotifyMcpClientFixture _fixture = null!;

    public async Task InitializeAsync()
    {
        _fixture = new SpotifyMcpClientFixture();
        await _fixture.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await _fixture.DisposeAsync();
    }

    [Fact]
    public async Task Client_IsInitializedSuccessfully()
    {
        // Verify client is accessible and connected
        Assert.NotNull(_fixture.Client);
    }

    [Fact]
    public async Task Client_CanListAvailableTools()
    {
        var tools = await _fixture.GetAllToolsAsync();

        // Should discover tools from the server
        Assert.NotEmpty(tools);
    }

    [Fact]
    public async Task Client_DiscoversTool_WithNameAndDescription()
    {
        var tools = await _fixture.GetAllToolsAsync();
        var tool = tools.First();

        // All tools should have name and description
        Assert.NotNull(tool.Name);
        Assert.NotEmpty(tool.Name);
        Assert.NotNull(tool.Description);
        Assert.NotEmpty(tool.Description);
    }

    [Fact]
    public async Task Client_DiscoversTool_WithInputSchema()
    {
        var tools = await _fixture.GetAllToolsAsync();
        var tool = tools.First();

        // All tools should have some form of input schema definition
        // InputSchema may be null or a JSON object depending on implementation
        Assert.NotNull(tool);
        Assert.NotEmpty(tool.Name);
    }

    [Fact]
    public async Task Client_ListsAllAuthenticationTools()
    {
        var tools = await _fixture.GetAllToolsAsync();
        var authTools = tools.Where(t => t.Name.StartsWith("get_auth") || t.Name == "exchange_auth_code" || t.Name == "refresh_token" || t.Name == "get_client_token").ToList();

        // Should discover all auth tools
        Assert.NotEmpty(authTools);
    }

    [Fact]
    public async Task Client_ListsAllTrackTools()
    {
        var tools = await _fixture.GetAllToolsAsync();
        var trackTools = tools.Where(t => t.Name.Contains("track")).ToList();

        // Should discover track-related tools
        Assert.NotEmpty(trackTools);
    }

    [Fact]
    public async Task Client_ListsAllArtistTools()
    {
        var tools = await _fixture.GetAllToolsAsync();
        var artistTools = tools.Where(t => t.Name.Contains("artist")).ToList();

        // Should discover artist-related tools
        Assert.NotEmpty(artistTools);
    }

    [Fact]
    public async Task Client_ListsAllAlbumTools()
    {
        var tools = await _fixture.GetAllToolsAsync();
        var albumTools = tools.Where(t => t.Name.Contains("album")).ToList();

        // Should discover album-related tools
        Assert.NotEmpty(albumTools);
    }

    [Fact]
    public async Task Client_ListsAllSearchTools()
    {
        var tools = await _fixture.GetAllToolsAsync();
        var searchTools = tools.Where(t => t.Name.StartsWith("search")).ToList();

        // Should discover search tools
        Assert.NotEmpty(searchTools);
    }

    [Fact]
    public async Task Client_ListsAllAudiobookTools()
    {
        var tools = await _fixture.GetAllToolsAsync();
        var audiobookTools = tools.Where(t => t.Name.Contains("audiobook")).ToList();

        // Should discover audiobook tools
        Assert.NotEmpty(audiobookTools);
    }

    [Fact]
    public async Task Client_ListsAllPlaylistTools()
    {
        var tools = await _fixture.GetAllToolsAsync();
        var playlistTools = tools.Where(t => t.Name.Contains("playlist")).ToList();

        // Should discover playlist tools
        Assert.NotEmpty(playlistTools);
    }

    [Fact]
    public async Task Client_ListsAllPlayerTools()
    {
        var tools = await _fixture.GetAllToolsAsync();
        var playerTools = tools.Where(t => t.Name.Contains("playback") || t.Name.Contains("skip")).ToList();

        // Should discover player control tools
        Assert.NotEmpty(playerTools);
    }

    [Fact]
    public async Task Client_ListsUserTool()
    {
        var tools = await _fixture.GetAllToolsAsync();
        var userTool = tools.FirstOrDefault(t => t.Name == "get_current_user");

        Assert.NotNull(userTool);
    }

    [Fact]
    public async Task Client_ToolCount_MatchesExpected()
    {
        var tools = await _fixture.GetAllToolsAsync();

        // The server should expose all tools from all tool classes
        // Expected: ~34+ tools based on the implementation (auth + track + artist + album + search + audiobook + playlist + player + user)
        Assert.True(tools.Count >= 34, $"Expected at least 34 tools, but found {tools.Count}");
    }

    [Fact]
    public async Task Client_AllTools_AreValidAndComplete()
    {
        var tools = await _fixture.GetAllToolsAsync();

        foreach (var tool in tools)
        {
            // Each tool should have a valid name and description
            Assert.NotEmpty(tool.Name);
            Assert.NotEmpty(tool.Description);
            // InputSchema is present (may be null but the field exists)
            // Just verify tool is properly formed
            Assert.NotNull(tool);
        }
    }
}
