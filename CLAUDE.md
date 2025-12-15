# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Essential Commands

**Build & Run**
```bash
dotnet build                                              # Build the solution
dotnet run --project Spotify.MCP.Host                     # Run the MCP server (prints available tools)
```

**Testing**
```bash
dotnet test                                              # Run all tests (Host tests + Client tests)
dotnet test Spotify.MCP.Host.Tests                       # Run Host unit and integration tests
dotnet test Spotify.MCP.Host.Tests --filter Category=Unit              # Run unit tests only
dotnet test Spotify.MCP.Host.Tests --filter Category=Integration       # Run integration tests only
dotnet test Spotify.MCP.Client.Tests                     # Run MCP Client tests (tests server as a process)
dotnet test Spotify.MCP.Client.Tests --list-tests        # List all client tests
dotnet test Spotify.MCP.Host.Tests -v d                  # Run with detailed output
```

**Code Quality**
```bash
dotnet build -c Release                                  # Build optimized release binary
```

## Architecture Overview

### High-Level Design

This is a **Model Context Protocol (MCP) server** that exposes Spotify Web API functionality as tools for Claude Desktop. The architecture follows a clean layered pattern:

```
Claude Desktop (MCP Client)
    ↓
Stdin/Stdout Transport (MCP Protocol)
    ↓
Program.cs (DI Container Setup & Tool Discovery)
    ↓
Tool Classes (AuthTools, TrackTools, SearchTools, etc.)
    ↓
Service Layer (SpotifyApiService, SpotifyAuthService)
    ↓
HttpClient
    ↓
Spotify Web API
```

### Projects

**Spotify.MCP.Host** (.NET 9.0 Console App)
- Main MCP server executable
- Auto-discovers tools via reflection using `[McpServerToolType]` attributes
- Handles dependency injection setup via Program.cs
- Loads configuration from appsettings.json and command-line arguments
- Prints all available tools to stderr on startup for debugging

**Spotify.MCP.Host.Tests** (xUnit Test Project)
- Unit tests: Mock-based tests in `/Unit` directory
- Integration tests: Real API calls in `/Integration` directory
- Uses Moq for mocking, coverlet for coverage reporting
- Requires Spotify credentials in User Secrets (ID: `f2e5bb64-afd0-4fb0-a20a-42aee602652c`)

**Spotify.MCP.Client.Tests** (xUnit Test Project - End-to-End MCP Client Tests)
- Tests the MCP server from a client perspective using `StdioClientTransport`
- Launches Spotify.MCP.Host as a subprocess and communicates via stdio
- Tests three main areas:
  - **ClientConnectionTests**: Server connection, tool discovery, session management
  - **ToolInvocationTests**: Actual tool invocation with real Spotify API calls
  - **AuthenticationTests**: OAuth flow and authentication endpoints
- Loads Spotify credentials from UserSecrets and passes them to server
- UserSecretsId: `a1b2c3d4-e5f6-7a8b-9c0d-e1f2a3b4c5d6`
- Configuration keys: `Spotify:ClientId` and `Spotify:ClientSecret`
- Includes README.md with detailed information on test requirements and running tests

### Key Directories

| Directory | Purpose |
|-----------|---------|
| `Models/Output/` | C# record types representing Spotify API responses (Track, Artist, Album, etc.) |
| `Models/Input/` | Wrapper types for API responses (SearchResponse, SearchTracks, etc.) |
| `Services/` | Business logic layer (ISpotifyApiService, ISpotifyAuthService) |
| `Tools/` | MCP tool implementations, one class per tool category (AuthTools, TrackTools, etc.) |
| `Properties/PublishProfiles/` | Release build configuration profiles |
| `Assets/` | Spotify icon for Claude Desktop integration |

### Data Model Design

All models use **C# records** for immutability:

```csharp
public record Track(
    string Id, string Name, string Uri, string Href,
    Dictionary<string, string> ExternalUrls,
    string? PreviewUrl, int TrackNumber, string Type,
    int Popularity, int DurationMs, bool Explicit,
    bool? IsPlayable, List<Artist> Artists, Album Album,
    List<string>? AvailableMarkets);
```

- JSON serialization uses snake_case (Spotify standard)
- Nullable reference types enabled for explicit optional fields
- No mutable state or side effects in models

### Service Layer Patterns

**ISpotifyApiService** (Main API Service, 37 methods)
- All methods are async (`Task<T>` return types)
- Handles token management via ISpotifyAuthService
- Automatic fallback to client credentials if no user token provided
- All HTTP calls use dependency-injected HttpClient for pooling

**ISpotifyAuthService** (Authentication, 4 methods)
- `GetClientCredentialsTokenAsync()` - Cached public access tokens
- `GetAuthorizationUrlAsync()` - OAuth URL generation
- `ExchangeAuthorizationCodeAsync()` - Code-to-token exchange
- `RefreshTokenAsync()` - Token refresh when expired

### Tool Implementation Pattern

All tools follow a consistent pattern:

```csharp
[McpServerToolType]
public class CategoryTools {
    public CategoryTools(ISpotifyApiService api, ILogger<CategoryTools> logger) { }

    [McpServerTool(Name = "tool_name", Title = "Display Title")]
    [Description("Detailed description of what this tool does")]
    public async Task<string> ToolNameAsync(
        [Description("Parameter description")] string parameter1,
        [Description("...")] int parameter2 = defaultValue)
    {
        // Input validation at beginning
        // Service call (await async method)
        // JSON serialization of complex results
        // Exception handling with logging
        // Return JSON string or error message
    }
}
```

**Key conventions:**
- Tool names use snake_case in `[McpServerTool(Name = "...")]` attribute
- Return type is always `Task<string>`
- Complex objects are JSON-serialized before returning
- Errors are plain text strings (not JSON)
- Constructor injection for dependencies

## Configuration

### Spotify.MCP.Host Server

**appsettings.json** (not in repo, must be created)
```json
{
  "Spotify": {
    "ClientId": "your_client_id",
    "ClientSecret": "your_client_secret"
  }
}
```

**Command-line Override**
```bash
dotnet run --project Spotify.MCP.Host -- --spotify-client-id YOUR_ID --spotify-client-secret YOUR_SECRET
```

### Spotify.MCP.Client.Tests

The client test project loads credentials from UserSecrets and passes them to the server:

```bash
# Set credentials in UserSecrets
dotnet user-secrets set Spotify:ClientId your_client_id_here --project Spotify.MCP.Client.Tests
dotnet user-secrets set Spotify:ClientSecret your_client_secret_here --project Spotify.MCP.Client.Tests

# Verify configuration
dotnet user-secrets list --project Spotify.MCP.Client.Tests
```

The fixture will fail with a clear error message if credentials are not configured.

**Claude Desktop Integration** (from README)
Two options:
1. Run from source: `dotnet run --project C:\path\to\Spotify.MCP.Host`
2. Run compiled executable: `C:\path\to\Spotify.MCP.Host.exe`

## Testing Strategy

### Test Structure

**Unit Tests** (`/Unit` directory)
- Mock ISpotifyApiService using Moq
- Test tool classes in isolation
- Example: `TrackToolsTests.cs`

**Integration Tests** (`/Integration` directory)
- Use MockSpotifyApiService for controlled real-world scenarios
- Test full tool chains with realistic data
- Require Spotify credentials in User Secrets

### Running Tests

```bash
# All tests
dotnet test

# Just unit tests
dotnet test --filter Category=Unit

# Specific test class
dotnet test --filter FullyQualifiedName~TrackToolsTests

# With coverage
dotnet test /p:CollectCoverage=true
```

### MockSpotifyApiService

Located at `Spotify.MCP.Host.Tests/MockSpotifyApiService.cs` (8.6 KB)
- Full implementation of ISpotifyApiService
- Returns realistic mock data for all methods
- Used for integration tests without API calls

## Common Development Tasks

### Adding a New Tool

1. **Add service method** (if needed) to `ISpotifyApiService` and `SpotifyApiService`
2. **Create or update tool class** in `Tools/` directory with `[McpServerToolType]` attribute
3. **Implement tool method** with `[McpServerTool]` and `[Description]` attributes
4. **Return JSON-serialized string** for complex objects
5. **Write unit test** with Moq mocks
6. **Write integration test** with MockSpotifyApiService
7. **Run tests** to verify: `dotnet test`

### Adding a New Data Model

1. **Create record type** in appropriate `Models/` subdirectory
2. **Use JsonPropertyName attribute** if property name differs from JSON (snake_case)
3. **Make fields nullable** (`string?`) for optional Spotify fields
4. **No constructors or methods** - records are immutable data containers
5. **Reference in SpotifyApiService** deserialization

### Debugging Tool Registration

Run the server with `dotnet run --project Spotify.MCP.Host` - it prints all discovered tools to stderr on startup.

If tools don't appear:
1. Check class has `[McpServerToolType]` attribute
2. Check method has `[McpServerTool]` attribute
3. Check method signature is `Task<string> MethodAsync(...)`
4. Rebuild and run again

## Technology Stack

| Technology | Version | Purpose |
|-----------|---------|---------|
| **.NET** | 9.0 | Runtime |
| **C#** | 12+ | Language (nullable reference types enabled) |
| **ModelContextProtocol** | 0.5.0-preview.1 | MCP server SDK |
| **Microsoft.Extensions.*** | 9.0.x | DI, configuration, hosting, HTTP |
| **System.Text.Json** | 10.0.0 | JSON serialization |
| **xUnit** | 2.9.2 | Unit testing |
| **Moq** | 4.20.70 | Mocking library |

## Important Notes

- **Icon copying**: Build config always copies `Assets/spotify-icon.png` to output directory
- **Audiobook availability**: Only available in specific markets (US, UK, Canada, Ireland, New Zealand, Australia)
- **Token management**: Service automatically falls back to client credentials if user token unavailable
- **Error handling**: Each tool catches exceptions and returns plain text error strings for debugging
- **No appsettings.json in repo**: Create locally with your Spotify credentials or pass via command-line args
- **Reflection-based tool discovery**: All changes to tool attributes require rebuild before changes are visible to MCP server

## Project Maturity

- Active development (recent refactors and bug fixes)
- Comprehensive test coverage (unit + integration)
- Consistent naming conventions (recently standardized)
- Feature complete for Spotify Web API as of Sept 2025
