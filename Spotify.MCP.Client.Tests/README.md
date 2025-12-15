# Spotify.MCP.Client.Tests

End-to-end MCP client tests for the Spotify.MCP server. These tests launch the MCP server as a subprocess and communicate with it via the MCP protocol using `StdioClientTransport`.

## Test Organization

The test project is organized into three test classes:

### ClientConnectionTests
Tests for MCP client connection, initialization, and session management.
- **Client_IsInitializedSuccessfully**: Verifies the client can connect to the server
- **Client_CanListAvailableTools**: Verifies tool discovery works
- **Client_ListsAll*Tools**: Verifies specific tool categories are discovered
- **Client_ToolCount_MatchesExpected**: Verifies expected number of tools (≥35)
- **Client_AllTools_AreValidAndComplete**: Verifies all tools have valid names and descriptions

These tests run without requiring Spotify credentials.

### ToolInvocationTests
Tests actual tool invocation using well-known Spotify IDs.
- **GetClientToken_ReturnsAccessToken**: Tests OAuth token endpoint
- **GetTrack/GetArtist/GetAlbum**: Tests data retrieval tools
- **Search* tools**: Tests search functionality
- **GetAudioFeatures**: Tests audio analysis
- **Tool_InvalidParameters_ReturnsError**: Tests error handling

These tests make real API calls to Spotify using client credentials authentication. They use well-known public IDs (e.g., Bohemian Rhapsody track ID), so they don't require Spotify credentials if the client credentials token can be obtained.

### AuthenticationTests
Tests for authentication tools and OAuth flow.
- **GetClientToken_ReturnsTokenWithProperties**: Verifies token structure
- **GetAuthUrl_GeneratesValidAuthorizationUrl**: Tests OAuth URL generation
- **GetAuthUrl_IncludesRequiredScopesInUrl**: Tests scope handling
- **ExchangeAuthCode_WithInvalidCode_ReturnsError**: Tests error handling
- **RefreshToken_WithInvalidToken_ReturnsError**: Tests error handling
- ***_IsDiscoverable**: Tests that auth tools are discoverable via MCP protocol

These tests can run without Spotify credentials for most tests, except those that require making actual API calls.

## Test Requirements

All tests require Spotify API credentials to be configured in UserSecrets.

### Setting up Spotify Credentials

1. Get your Spotify credentials from the [Spotify Developer Dashboard](https://developer.spotify.com/dashboard)
2. Configure them in UserSecrets for this test project:

```bash
# Navigate to the test project directory
cd Spotify.MCP.Client.Tests

# Set your Client ID
dotnet user-secrets set Spotify:ClientId your_client_id_here

# Set your Client Secret
dotnet user-secrets set Spotify:ClientSecret your_client_secret_here
```

Or set them both at once:
```bash
dotnet user-secrets set "Spotify:ClientId" "your_client_id" && \
dotnet user-secrets set "Spotify:ClientSecret" "your_client_secret"
```

### Verifying Configuration

```bash
# List configured secrets
dotnet user-secrets list

# Remove a secret if needed
dotnet user-secrets remove Spotify:ClientId
```

## Running the Tests

```bash
# List all tests
dotnet test Spotify.MCP.Client.Tests --list-tests

# Run all client tests
dotnet test Spotify.MCP.Client.Tests

# Run only connection/discovery tests
dotnet test Spotify.MCP.Client.Tests --filter "ClientConnectionTests"

# Run specific test
dotnet test Spotify.MCP.Client.Tests --filter "Client_IsInitializedSuccessfully"

# Run with verbose output
dotnet test Spotify.MCP.Client.Tests -v d
```

## Test Characteristics

### Connection Tests (ClientConnectionTests)
- Requires Spotify credentials (for server to start)
- Tests MCP protocol and tool discovery
- Fastest tests (~30-50ms each)
- ~15 tests total

### Authentication Tests (AuthenticationTests)
- Requires Spotify credentials (for server to start)
- Tests OAuth flow and token handling
- Tests both URL generation and token exchange
- ~13 tests total

### Tool Invocation Tests (ToolInvocationTests)
- Requires Spotify credentials (for server to start)
- Makes real API calls to Spotify
- Tests use well-known public Spotify IDs (e.g., "3z8h0TU7RvxVFlaVrFLuQi" for Bohemian Rhapsody)
- Longer execution time (~1-5 seconds per test) due to network calls
- ~19 tests total

## Troubleshooting

### "Spotify.MCP.Host.csproj not found"
The test fixture attempts to locate the Spotify.MCP.Host project file to launch as a subprocess. Ensure the project structure matches expected:
- This test project: `Spotify.MCP.Client.Tests/`
- Host project: `Spotify.MCP.Host/`
- Repository root: `Spotify.MCP/`

### Tests timeout
The server process may be slow to start or network calls may be slow. Increase the timeout in the CLI:
```bash
dotnet test Spotify.MCP.Client.Tests --logger:"console;verbosity=detailed" --configuration Release
```

### API errors in tool invocation tests
If tests that call Spotify API endpoints are failing:
1. Verify network connectivity to api.spotify.com
2. Check if Spotify is experiencing issues
3. Some tools may need Spotify credentials configured for the server
4. Audiobook tests may fail due to market restrictions (US, UK, CA, etc. only)

## Architecture

The tests use `SpotifyMcpClientFixture` which:
1. Locates the Spotify.MCP.Host.csproj file
2. Launches it as a subprocess via `dotnet run` with stdio transport
3. Creates an MCP client connected via stdio pipes
4. Provides helper methods for tool invocation and discovery
5. Cleans up the server process on test completion

Each test class inherits from `IAsyncLifetime` to properly initialize and dispose of the fixture per test execution.
