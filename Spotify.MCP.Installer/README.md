# Spotify MCP Server Installer

This project provides multiple installation options for the Spotify MCP Server:

## Installation Options

### 1. 🚀 **Quick Install (Recommended)**
Run the .NET installer directly:

```bash
# Build and run the installer
dotnet run --project Spotify.MCP.Installer

# Or build first, then run
dotnet build Spotify.MCP.Installer
dotnet run --project Spotify.MCP.Installer
```

**Features:**
- ✅ Automatic .NET 9.0 prerequisite checking
- ✅ Smart file detection and copying
- ✅ Automatic Claude Desktop configuration
- ✅ System PATH integration
- ✅ Desktop shortcuts
- ✅ Clean uninstallation support

### 2. 📜 **PowerShell Scripts**
For advanced users who prefer script-based installation:

```powershell
# Install (requires Administrator privileges)
.\Install-SpotifyMCP.ps1

# Install to custom location
.\Install-SpotifyMCP.ps1 -InstallPath "C:\Tools\SpotifyMCP"

# Install without Claude Desktop configuration
.\Install-SpotifyMCP.ps1 -SkipClaudeConfig

# Uninstall
.\Uninstall-SpotifyMCP.ps1

# Uninstall but keep Claude configuration
.\Uninstall-SpotifyMCP.ps1 -KeepClaudeConfig
```

### 3. 🎯 **WiX MSI Installer**
For enterprise deployment (requires WiX Toolset):

```bash
# Build MSI package (requires WiX Toolset v3.11+)
msbuild Spotify.MCP.Installer.wixproj
```

## Command Line Options

The .NET installer supports several command line options:

```bash
# Basic installation
dotnet run --project Spotify.MCP.Installer

# Silent installation
dotnet run --project Spotify.MCP.Installer -- --silent

# Custom installation path
dotnet run --project Spotify.MCP.Installer -- --path "C:\MyPath"

# Skip Claude Desktop configuration
dotnet run --project Spotify.MCP.Installer -- --skip-claude

# Uninstall
dotnet run --project Spotify.MCP.Installer -- --uninstall

# Silent uninstall
dotnet run --project Spotify.MCP.Installer -- --uninstall --silent
```

## What Gets Installed

The installer will:

1. **📁 Create installation directory** (default: `C:\Program Files\Spotify MCP Server`)
2. **📦 Copy application files:**
   - `Spotify.MCP.Host.exe` - Main application
   - `appsettings.json` - Configuration file
   - All required .NET dependencies
   - Documentation and samples

3. **⚙️ Configure Claude Desktop:**
   - Updates `%APPDATA%\Claude\claude_desktop_config.json`
   - Adds Spotify MCP server configuration
   - Preserves existing MCP server configurations

4. **🔗 Create shortcuts and PATH integration:**
   - Adds installation directory to system PATH
   - Creates desktop shortcut to configuration folder
   - Creates sample configuration files

## Post-Installation Setup

After installation, you need to:

### 1. 🎵 Get Spotify API Credentials
1. Go to [Spotify Developer Dashboard](https://developer.spotify.com/dashboard)
2. Create a new app
3. Copy your Client ID and Client Secret

### 2. ⚙️ Configure the Server
Edit the `appsettings.json` file in the installation directory:

```json
{
  "Spotify": {
    "ClientId": "your_client_id_here",
    "ClientSecret": "your_client_secret_here"
  }
}
```

### 3. 🔄 Restart Claude Desktop
Close and restart Claude Desktop for the MCP server to be recognized.

### 4. 🧪 Test the Installation
Run the server manually to verify it works:
```bash
"C:\Program Files\Spotify MCP Server\Spotify.MCP.Host.exe"
```

## Uninstallation

To remove the Spotify MCP Server:

```bash
# Using the .NET installer
dotnet run --project Spotify.MCP.Installer -- --uninstall

# Using PowerShell script
.\Uninstall-SpotifyMCP.ps1
```

The uninstaller will:
- Stop any running processes
- Remove installation files
- Clean up Claude Desktop configuration
- Remove system PATH entries
- Remove shortcuts

## Troubleshooting

### Prerequisites
- ✅ **Windows 10/11** (x64)
- ✅ **.NET 9.0 Runtime** - [Download here](https://dotnet.microsoft.com/download/dotnet/9.0)
- ✅ **Claude Desktop** - For MCP integration

### Common Issues

**"Could not find Spotify.MCP.Host.exe"**
- Make sure you've built the main project first: `dotnet build Spotify.MCP.Host`

**"Access Denied" during installation**
- Run as Administrator for system-wide installation
- Or use `--path` to install to a user directory

**Claude Desktop doesn't see the server**
- Restart Claude Desktop completely
- Check the configuration file: `%APPDATA%\Claude\claude_desktop_config.json`
- Verify the executable path is correct

**Server starts but no tools available**
- Check your Spotify API credentials in `appsettings.json`
- Verify internet connectivity
- Check server logs for authentication errors

### Manual Configuration

If automatic Claude Desktop configuration fails, manually edit:
`%APPDATA%\Claude\claude_desktop_config.json`

```json
{
  "mcpServers": {
    "spotify": {
      "command": "C:\\Program Files\\Spotify MCP Server\\Spotify.MCP.Host.exe"
    }
  }
}
```

## Development

### Building from Source

```bash
# Clone the repository
git clone <repository-url>
cd Spotify.MCP

# Build all projects
dotnet build

# Build installer specifically  
dotnet build Spotify.MCP.Installer

# Run installer in development
dotnet run --project Spotify.MCP.Installer
```

### Project Structure

```
Spotify.MCP.Installer/
├── Program.cs                           # Main .NET installer
├── Install-SpotifyMCP.ps1              # PowerShell installer
├── Uninstall-SpotifyMCP.ps1            # PowerShell uninstaller
├── Product.wxs                         # WiX installer definition
├── ClaudeDesktopConfig.wxs            # WiX Claude configuration
├── SampleConfig/
│   ├── appsettings.sample.json        # Sample configuration
│   └── claude_desktop_config.sample.json
└── README.md                          # This file
```

## License

This installer is part of the Spotify MCP Server project and follows the same licensing terms.