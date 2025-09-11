#Requires -Version 5.1
#Requires -RunAsAdministrator

<#
.SYNOPSIS
    Uninstaller script for Spotify MCP Server
    
.DESCRIPTION
    This script removes the Spotify MCP Server and cleans up Claude Desktop configuration.
    
.PARAMETER InstallPath
    The path where the Spotify MCP Server is installed. Default: C:\Program Files\Spotify MCP Server
    
.PARAMETER KeepClaudeConfig
    Keep Claude Desktop configuration (don't remove the Spotify MCP Server entry)
    
.EXAMPLE
    .\Uninstall-SpotifyMCP.ps1
    
.EXAMPLE
    .\Uninstall-SpotifyMCP.ps1 -InstallPath "C:\Tools\SpotifyMCP" -KeepClaudeConfig
#>

param(
    [string]$InstallPath = "C:\Program Files\Spotify MCP Server",
    [switch]$KeepClaudeConfig
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Write-Host "=== Spotify MCP Server Uninstaller ===" -ForegroundColor Red
Write-Host ""

if (-not (Test-Path $InstallPath)) {
    Write-Warning "Installation directory not found: $InstallPath"
    Write-Host "Nothing to uninstall."
    exit 0
}

Write-Host "Uninstalling from: $InstallPath" -ForegroundColor Yellow

# Confirm uninstall
$confirm = Read-Host "Are you sure you want to uninstall Spotify MCP Server? (y/N)"
if ($confirm -notmatch '^[Yy]$') {
    Write-Host "Uninstall cancelled."
    exit 0
}

# Stop any running processes
Write-Host "Checking for running processes..." -ForegroundColor Yellow
$processes = Get-Process -Name "Spotify.MCP.Host" -ErrorAction SilentlyContinue
if ($processes) {
    Write-Host "Stopping Spotify MCP Server processes..." -ForegroundColor Yellow
    $processes | Stop-Process -Force
    Start-Sleep -Seconds 2
    Write-Host "✓ Processes stopped" -ForegroundColor Green
}

# Remove from Claude Desktop configuration (if not keeping)
if (-not $KeepClaudeConfig) {
    Write-Host "Removing Claude Desktop configuration..." -ForegroundColor Yellow
    
    $claudeConfigPath = Join-Path $env:APPDATA "Claude\claude_desktop_config.json"
    
    if (Test-Path $claudeConfigPath) {
        try {
            $claudeConfig = Get-Content $claudeConfigPath -Raw | ConvertFrom-Json
            if ($claudeConfig.mcpServers -and $claudeConfig.mcpServers.spotify) {
                # Remove the spotify server configuration
                $claudeConfig.mcpServers.PSObject.Properties.Remove('spotify')
                
                # Save updated configuration
                $claudeConfig | ConvertTo-Json -Depth 10 | Out-File -FilePath $claudeConfigPath -Encoding UTF8
                Write-Host "✓ Removed from Claude Desktop configuration" -ForegroundColor Green
            } else {
                Write-Host "✓ No Spotify configuration found in Claude Desktop" -ForegroundColor Green
            }
        } catch {
            Write-Warning "Could not update Claude Desktop configuration: $_"
        }
    } else {
        Write-Host "✓ No Claude Desktop configuration found" -ForegroundColor Green
    }
} else {
    Write-Host "⚠ Kept Claude Desktop configuration" -ForegroundColor Yellow
}

# Remove from system PATH
Write-Host "Removing from system PATH..." -ForegroundColor Yellow
try {
    $currentPath = [Environment]::GetEnvironmentVariable("PATH", "Machine")
    if ($currentPath -like "*$InstallPath*") {
        $pathEntries = $currentPath.Split(';') | Where-Object { $_ -ne $InstallPath -and $_ -ne "$InstallPath\" }
        $newPath = $pathEntries -join ';'
        [Environment]::SetEnvironmentVariable("PATH", $newPath, "Machine")
        Write-Host "✓ Removed from system PATH" -ForegroundColor Green
    } else {
        Write-Host "✓ Not found in system PATH" -ForegroundColor Green
    }
} catch {
    Write-Warning "Could not update system PATH: $_"
}

# Remove Start Menu shortcut
Write-Host "Removing Start Menu shortcut..." -ForegroundColor Yellow
$startMenuPath = Join-Path $env:ProgramData "Microsoft\Windows\Start Menu\Programs"
$shortcutPath = Join-Path $startMenuPath "Spotify MCP Server Configuration.lnk"

if (Test-Path $shortcutPath) {
    try {
        Remove-Item $shortcutPath -Force
        Write-Host "✓ Start Menu shortcut removed" -ForegroundColor Green
    } catch {
        Write-Warning "Could not remove Start Menu shortcut: $_"
    }
} else {
    Write-Host "✓ No Start Menu shortcut found" -ForegroundColor Green
}

# Remove installation directory
Write-Host "Removing installation files..." -ForegroundColor Yellow
try {
    Remove-Item $InstallPath -Recurse -Force
    Write-Host "✓ Installation files removed" -ForegroundColor Green
} catch {
    Write-Error "Could not remove installation directory: $_"
    Write-Host "You may need to manually delete: $InstallPath" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "=== Uninstall Complete! ===" -ForegroundColor Green
Write-Host ""

if (-not $KeepClaudeConfig) {
    Write-Host "Please restart Claude Desktop for configuration changes to take effect." -ForegroundColor Cyan
}

Write-Host ""
Write-Host "Press any key to continue..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")