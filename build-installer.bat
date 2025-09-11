@echo off
echo Building Spotify MCP Server Installer
echo ====================================
echo.

REM Check if .NET is available
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo Error: .NET SDK not found. Please install .NET 9.0 SDK.
    echo Download from: https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

echo Building main application...
dotnet build Spotify.MCP.Host --configuration Release
if errorlevel 1 (
    echo Error: Failed to build Spotify.MCP.Host
    pause
    exit /b 1
)

echo.
echo Building installer...
dotnet build Spotify.MCP.Installer --configuration Release
if errorlevel 1 (
    echo Error: Failed to build installer
    pause
    exit /b 1
)

echo.
echo Publishing self-contained installer...
dotnet publish Spotify.MCP.Installer --configuration Release --runtime win-x64 --self-contained true -p:PublishSingleFile=true
if errorlevel 1 (
    echo Warning: Self-contained publish failed, using framework-dependent build
)

echo.
echo ====================================
echo Build completed successfully!
echo.
echo Installer executable location:
echo   Debug: Spotify.MCP.Installer\bin\Debug\net9.0\Spotify.MCP.Installer.exe
echo   Release: Spotify.MCP.Installer\bin\Release\net9.0\Spotify.MCP.Installer.exe
echo.
echo PowerShell scripts location:
echo   Spotify.MCP.Installer\Install-SpotifyMCP.ps1
echo   Spotify.MCP.Installer\Uninstall-SpotifyMCP.ps1
echo.
echo To run the installer:
echo   dotnet run --project Spotify.MCP.Installer
echo   OR
echo   Spotify.MCP.Installer\bin\Release\net9.0\Spotify.MCP.Installer.exe
echo.
pause