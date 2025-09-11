#Requires -Version 5.1
#Requires -RunAsAdministrator

<#
.SYNOPSIS
    Installer script for Spotify MCP Server
    
.DESCRIPTION
    This script installs the Spotify MCP Server and configures Claude Desktop to use it.
    
.PARAMETER InstallPath
    The path where the Spotify MCP Server will be installed. Default: C:\Program Files\Spotify MCP Server
    
.PARAMETER SkipClaudeConfig
    Skip automatic Claude Desktop configuration
    
.EXAMPLE
    .\Install-SpotifyMCP.ps1
    
.EXAMPLE
    .\Install-SpotifyMCP.ps1 -InstallPath "C:\Tools\SpotifyMCP" -SkipClaudeConfig
#>

param(
    [string]$InstallPath = "C:\Program Files\Spotify MCP Server",
    [switch]$SkipClaudeConfig
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Write-Host "=== Spotify MCP Server Installer ===" -ForegroundColor Green
Write-Host ""

# Check if .NET 9.0 is installed
Write-Host "Checking .NET 9.0 Runtime..." -ForegroundColor Yellow
try {
    $dotnetInfo = dotnet --info 2>$null | Select-String "Microsoft.NETCore.App 9\."
    if (-not $dotnetInfo) {
        Write-Warning ".NET 9.0 Runtime not found. Please install it from https://dotnet.microsoft.com/download"
        Write-Host "Press any key to continue or Ctrl+C to abort..."
        $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    } else {
        Write-Host "✓ .NET 9.0 Runtime found" -ForegroundColor Green
    }
} catch {
    Write-Warning "Could not check .NET version. Please ensure .NET 9.0 Runtime is installed."
}

# Create installation directory
Write-Host "Creating installation directory: $InstallPath" -ForegroundColor Yellow
if (-not (Test-Path $InstallPath)) {
    New-Item -ItemType Directory -Path $InstallPath -Force | Out-Null
}

# Find the built application
$sourceDir = Join-Path $PSScriptRoot "..\Spotify.MCP.Host\bin\Release\net9.0"
if (-not (Test-Path $sourceDir)) {
    $sourceDir = Join-Path $PSScriptRoot "..\Spotify.MCP.Host\bin\Debug\net9.0"
}

if (-not (Test-Path $sourceDir)) {
    Write-Error "Could not find built application. Please build the Spotify.MCP.Host project first."
    exit 1
}

Write-Host "✓ Found built application at: $sourceDir" -ForegroundColor Green

# Copy application files
Write-Host "Copying application files..." -ForegroundColor Yellow
$filesToCopy = @(
    "Spotify.MCP.Host.exe",
    "Spotify.MCP.Host.dll",
    "appsettings.json",
    "Spotify.MCP.Host.runtimeconfig.json",
    "Spotify.MCP.Host.deps.json"
)

foreach ($file in $filesToCopy) {
    $sourcePath = Join-Path $sourceDir $file
    $destPath = Join-Path $InstallPath $file
    if (Test-Path $sourcePath) {
        Copy-Item $sourcePath $destPath -Force
        Write-Host "  ✓ Copied $file" -ForegroundColor Green
    } else {
        Write-Warning "  ⚠ Could not find $file"
    }
}

# Copy all DLL dependencies
Write-Host "Copying dependencies..." -ForegroundColor Yellow
Get-ChildItem -Path $sourceDir -Filter "*.dll" | ForEach-Object {
    $destPath = Join-Path $InstallPath $_.Name
    Copy-Item $_.FullName $destPath -Force
}
Write-Host "✓ Dependencies copied" -ForegroundColor Green

# Copy README
$readmePath = Join-Path $PSScriptRoot "..\Spotify.MCP.Host\README.md"
if (Test-Path $readmePath) {
    Copy-Item $readmePath (Join-Path $InstallPath "README.md") -Force
    Write-Host "✓ README.md copied" -ForegroundColor Green
}

# Create sample configuration
Write-Host "Creating sample configuration..." -ForegroundColor Yellow
$configDir = Join-Path $InstallPath "Config"
if (-not (Test-Path $configDir)) {
    New-Item -ItemType Directory -Path $configDir -Force | Out-Null
}

# Create sample appsettings.json
$sampleAppSettings = @"
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "Spotify": {
    "ClientId": "YOUR_SPOTIFY_CLIENT_ID_HERE",
    "ClientSecret": "YOUR_SPOTIFY_CLIENT_SECRET_HERE"
  }
}
"@

$sampleAppSettings | Out-File -FilePath (Join-Path $configDir "appsettings.sample.json") -Encoding UTF8

# Create sample Claude configuration
$exePath = Join-Path $InstallPath "Spotify.MCP.Host.exe"
$sampleClaudeConfig = @"
{
  "mcpServers": {
    "spotify": {
      "command": "$($exePath -replace '\\', '\\')"
    }
  }
}
"@

$sampleClaudeConfig | Out-File -FilePath (Join-Path $configDir "claude_desktop_config.sample.json") -Encoding UTF8

Write-Host "✓ Sample configurations created in Config folder" -ForegroundColor Green

# Configure Claude Desktop (if not skipped)
if (-not $SkipClaudeConfig) {
    Write-Host "Configuring Claude Desktop..." -ForegroundColor Yellow
    
    $claudeConfigPath = Join-Path $env:APPDATA "Claude\claude_desktop_config.json"
    $claudeDir = Split-Path $claudeConfigPath -Parent
    
    if (-not (Test-Path $claudeDir)) {
        New-Item -ItemType Directory -Path $claudeDir -Force | Out-Null
    }
    
    $claudeConfig = @{
        mcpServers = @{}
    }
    
    # Load existing configuration if it exists
    if (Test-Path $claudeConfigPath) {
        try {
            $existingConfig = Get-Content $claudeConfigPath -Raw | ConvertFrom-Json
            if ($existingConfig.mcpServers) {
                $claudeConfig.mcpServers = $existingConfig.mcpServers
            }
        } catch {
            Write-Warning "Could not parse existing Claude configuration. Creating new configuration."
        }
    }
    
    # Add Spotify MCP Server configuration
    $claudeConfig.mcpServers.spotify = @{
        command = $exePath
    }
    
    # Save configuration
    $claudeConfig | ConvertTo-Json -Depth 10 | Out-File -FilePath $claudeConfigPath -Encoding UTF8
    Write-Host "✓ Claude Desktop configured at: $claudeConfigPath" -ForegroundColor Green
    Write-Host "  Please restart Claude Desktop for changes to take effect." -ForegroundColor Cyan
} else {
    Write-Host "⚠ Skipped Claude Desktop configuration" -ForegroundColor Yellow
}

# Create Start Menu shortcut
Write-Host "Creating Start Menu shortcut..." -ForegroundColor Yellow
$startMenuPath = Join-Path $env:ProgramData "Microsoft\Windows\Start Menu\Programs"
$shortcutPath = Join-Path $startMenuPath "Spotify MCP Server Configuration.lnk"

try {
    $WScriptShell = New-Object -ComObject WScript.Shell
    $Shortcut = $WScriptShell.CreateShortcut($shortcutPath)
    $Shortcut.TargetPath = "notepad.exe"
    $Shortcut.Arguments = """$(Join-Path $configDir "README.md")"""
    $Shortcut.WorkingDirectory = $InstallPath
    $Shortcut.Description = "Open Spotify MCP Server documentation and configuration"
    $Shortcut.Save()
    Write-Host "✓ Start Menu shortcut created" -ForegroundColor Green
} catch {
    Write-Warning "Could not create Start Menu shortcut: $_"
}

# Add to Windows PATH (optional)
Write-Host "Adding to system PATH..." -ForegroundColor Yellow
try {
    $currentPath = [Environment]::GetEnvironmentVariable("PATH", "Machine")
    if ($currentPath -notlike "*$InstallPath*") {
        $newPath = "$currentPath;$InstallPath"
        [Environment]::SetEnvironmentVariable("PATH", $newPath, "Machine")
        Write-Host "✓ Added to system PATH" -ForegroundColor Green
        Write-Host "  You may need to restart your command prompt/terminal" -ForegroundColor Cyan
    } else {
        Write-Host "✓ Already in system PATH" -ForegroundColor Green
    }
} catch {
    Write-Warning "Could not update system PATH: $_"
}

Write-Host ""
Write-Host "=== Installation Complete! ===" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Get your Spotify API credentials from https://developer.spotify.com/dashboard" -ForegroundColor White
Write-Host "2. Update the appsettings.json file with your credentials:" -ForegroundColor White
Write-Host "   $InstallPath\appsettings.json" -ForegroundColor Gray
Write-Host "3. Restart Claude Desktop if it's running" -ForegroundColor White
Write-Host "4. Test the installation by running:" -ForegroundColor White
Write-Host "   `"$exePath`"" -ForegroundColor Gray
Write-Host ""
Write-Host "Documentation and samples are available in:" -ForegroundColor White
Write-Host "   $configDir" -ForegroundColor Gray
Write-Host ""
Write-Host "Press any key to continue..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")