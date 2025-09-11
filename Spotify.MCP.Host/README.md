# Spotify MCP Server

A Model Context Protocol (MCP) server that provides tools to interact with the Spotify Web API. This server exposes various Spotify endpoints as MCP tools that can be used by MCP-compatible clients.

## Features

This MCP server provides tools for:

### Authentication
- `get_authorization_url` - Generate authorization URLs for OAuth flow
- `exchange_authorization_code` - Exchange authorization codes for access tokens
- `refresh_access_token` - Refresh expired access tokens
- `get_client_credentials_token` - Get access tokens for public data

### Track Management
- `get_track` - Get details about a specific track
- `get_tracks` - Get details about multiple tracks
- `get_audio_features` - Get audio features for a track
- `get_multiple_audio_features` - Get audio features for multiple tracks

### Artist Management
- `get_artist` - Get details about an artist
- `get_artist_albums` - Get all albums by an artist
- `get_artist_top_tracks` - Get top tracks for an artist

### Album Management
- `get_album` - Get details about an album
- `get_album_tracks` - Get all tracks from an album

### Audiobook Management
- `get_audiobook` - Get details about an audiobook
- `get_audiobooks` - Get details about multiple audiobooks
- `get_audiobook_chapters` - Get chapters from an audiobook
- `get_user_saved_audiobooks` - Get user's saved audiobooks (requires user token)
- `save_audiobooks_for_user` - Save audiobooks to user's library (requires user token)
- `remove_user_saved_audiobooks` - Remove audiobooks from user's library (requires user token)
- `check_user_saved_audiobooks` - Check if audiobooks are in user's library (requires user token)

### Playlist Management
- `get_playlist` - Get details about a playlist
- `get_user_playlists` - Get user's playlists (requires user token)

### Search
- `search` - Search for tracks, albums, artists, playlists, or audiobooks
- `search_tracks` - Search specifically for tracks
- `search_artists` - Search specifically for artists
- `search_albums` - Search specifically for albums
- `search_playlists` - Search specifically for playlists
- `search_audiobooks` - Search specifically for audiobooks

### Player Control (requires user authentication)
- `get_current_playback` - Get current playback state
- `pause_playback` - Pause playback
- `start_playback` - Start/resume playback
- `skip_to_next` - Skip to next track
- `skip_to_previous` - Skip to previous track

### User Profile
- `get_current_user` - Get current user's profile

## Setup

1. **Spotify App Registration**
   - Go to [Spotify Developer Dashboard](https://developer.spotify.com/dashboard)
   - Create a new app
   - Note your Client ID and Client Secret

2. **Configuration**
   - Update `appsettings.json` with your Spotify credentials:
   ```json
   {
     "Spotify": {
       "ClientId": "your_client_id_here",
       "ClientSecret": "your_client_secret_here"
     }
   }
   ```

3. **Build and Run**
   ```bash
   dotnet build
   dotnet run
   ```

## Claude Desktop Integration

To use this MCP server with Claude Desktop, add it to your Claude Desktop configuration file.

### Configuration File Location
- **Windows**: `%APPDATA%\Claude\claude_desktop_config.json`
- **macOS**: `~/Library/Application Support/Claude/claude_desktop_config.json`

### Configuration Example

Add the following to your `claude_desktop_config.json`:

```json
{
  "mcpServers": {
    "spotify": {
      "command": "dotnet",
      "args": [
        "run", 
        "--project", "C:\\path\\to\\your\\Spotify.MCP.Host\\Spotify.MCP.Host.csproj",
        "--",
        "--spotify-client-id", "your_spotify_client_id_here",
        "--spotify-client-secret", "your_spotify_client_secret_here"
      ],
      "icon": "file://C:\\path\\to\\your\\Spotify.MCP.Host\\assets\\spotify-icon.png"
    }
  }
}
```

### Alternative: Using Built Executable

If you prefer to use the compiled executable:

```json
{
  "mcpServers": {
    "spotify": {
      "command": "C:\\path\\to\\your\\Spotify.MCP.Host\\bin\\Debug\\net9.0\\Spotify.MCP.Host.exe",
      "args": [
        "--spotify-client-id", "your_spotify_client_id_here",
        "--spotify-client-secret", "your_spotify_client_secret_here"
      ],
      "icon": "file://C:\\path\\to\\your\\Spotify.MCP.Host\\assets\\spotify-icon.png"
    }
  }
}
```

### Setup Steps
1. Replace `C:\\path\\to\\your\\` with the actual path to your project (in both the command and icon paths)
2. Replace `your_spotify_client_id_here` with your actual Spotify Client ID
3. Replace `your_spotify_client_secret_here` with your actual Spotify Client Secret
4. Save the configuration file
5. Restart Claude Desktop

Once configured, you'll see the Spotify MCP server connected in Claude Desktop with the Spotify logo, and you'll be able to use all the Spotify tools directly in your conversations!

## Usage

### Public Data Access
Many endpoints work with just client credentials (no user authentication required):
- Track information
- Artist information 
- Album information
- Audiobook information (market restrictions apply)
- Search functionality

### User-Specific Data
Some endpoints require user authentication and appropriate scopes:
- User playlists
- User's saved audiobooks
- Playback control
- User profile information

### Authentication Flow
1. Use `get_authorization_url` to generate an authorization URL
2. User visits the URL and authorizes your app
3. Use `exchange_authorization_code` to get access and refresh tokens
4. Use the access token for user-specific API calls
5. Use `refresh_access_token` when the access token expires

## Required Scopes

Different operations require different Spotify scopes:

- `user-read-private` - Read user profile data
- `user-read-email` - Read user email
- `playlist-read-private` - Read private playlists
- `playlist-read-collaborative` - Read collaborative playlists
- `user-read-playback-state` - Read playback state
- `user-modify-playback-state` - Control playback
- `user-read-currently-playing` - Read currently playing track
- `user-library-read` - Read user's saved audiobooks
- `user-library-modify` - Save/remove user's audiobooks

## Notes

- The server uses client credentials flow by default for public data
- User authentication is required for personal data and playback control
- All responses are returned as formatted JSON strings
- Error handling is built into each tool with descriptive error messages
- The server supports both standalone track IDs and full Spotify URIs
- **Audiobook availability**: Audiobooks are only available in select markets (US, UK, Canada, Ireland, New Zealand, Australia)
- Market parameter is important for audiobook endpoints to ensure content availability

## Dependencies

- .NET 9.0
- Microsoft.Extensions.Hosting
- Microsoft.Extensions.Http
- ModelContextProtocol
- System.Text.Json