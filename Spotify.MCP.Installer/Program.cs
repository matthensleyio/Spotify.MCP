using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Spotify.MCP.Installer;

class Program
{
    private static readonly string DefaultInstallPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Spotify MCP Server");

    static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.WriteLine("╔══════════════════════════════════════╗");
        Console.WriteLine("║     Spotify MCP Server Installer    ║");
        Console.WriteLine("╚══════════════════════════════════════╝");
        Console.WriteLine();

        bool uninstall = args.Contains("--uninstall") || args.Contains("-u");
        bool silent = args.Contains("--silent") || args.Contains("-s");
        bool skipClaudeConfig = args.Contains("--skip-claude");
        string installPath = GetArgumentValue(args, "--path") ?? DefaultInstallPath;

        try
        {
            if (uninstall)
            {
                await UninstallAsync(installPath, silent, skipClaudeConfig);
            }
            else
            {
                await InstallAsync(installPath, silent, skipClaudeConfig);
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: {ex.Message}");
            Console.ResetColor();
            Environment.Exit(1);
        }

        if (!silent)
        {
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }
    }

    private static async Task InstallAsync(string installPath, bool silent, bool skipClaudeConfig)
    {
        Console.WriteLine("🚀 Starting installation...");
        Console.WriteLine();

        // Check prerequisites
        await CheckPrerequisitesAsync();

        // Create installation directory
        Console.WriteLine($"📁 Creating installation directory: {installPath}");
        Directory.CreateDirectory(installPath);

        // Find source files
        string sourceDir = FindSourceDirectory();
        Console.WriteLine($"📦 Source files found at: {sourceDir}");

        // Copy application files
        Console.WriteLine("📋 Copying application files...");
        await CopyApplicationFilesAsync(sourceDir, installPath);

        // Create configuration
        Console.WriteLine("⚙️  Creating configuration files...");
        CreateConfigurationFiles(installPath);

        // Configure Claude Desktop
        if (!skipClaudeConfig)
        {
            Console.WriteLine("🤖 Configuring Claude Desktop...");
            await ConfigureClaudeDesktopAsync(installPath);
        }
        else
        {
            Console.WriteLine("⏭️  Skipped Claude Desktop configuration");
        }

        // Create shortcuts
        Console.WriteLine("🔗 Creating shortcuts...");
        CreateShortcuts(installPath);

        // Update PATH
        Console.WriteLine("🛣️  Updating system PATH...");
        UpdateSystemPath(installPath, true);

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("✅ Installation completed successfully!");
        Console.ResetColor();
        Console.WriteLine();
        
        PrintNextSteps(installPath);
    }

    private static async Task UninstallAsync(string installPath, bool silent, bool keepClaudeConfig)
    {
        if (!Directory.Exists(installPath))
        {
            Console.WriteLine("❌ Installation directory not found. Nothing to uninstall.");
            return;
        }

        if (!silent)
        {
            Console.Write("⚠️  Are you sure you want to uninstall Spotify MCP Server? (y/N): ");
            var response = Console.ReadLine();
            if (!string.Equals(response?.Trim(), "y", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Uninstall cancelled.");
                return;
            }
        }

        Console.WriteLine("🗑️  Starting uninstallation...");

        // Stop running processes
        StopRunningProcesses();

        // Remove Claude Desktop configuration
        if (!keepClaudeConfig)
        {
            Console.WriteLine("🤖 Removing Claude Desktop configuration...");
            await RemoveClaudeDesktopConfigAsync();
        }

        // Remove from PATH
        Console.WriteLine("🛣️  Removing from system PATH...");
        UpdateSystemPath(installPath, false);

        // Remove shortcuts
        Console.WriteLine("🔗 Removing shortcuts...");
        RemoveShortcuts();

        // Remove installation directory
        Console.WriteLine("📁 Removing installation files...");
        Directory.Delete(installPath, true);

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("✅ Uninstall completed successfully!");
        Console.ResetColor();
    }

    private static async Task CheckPrerequisitesAsync()
    {
        Console.WriteLine("🔍 Checking prerequisites...");
        
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "--info",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (!output.Contains("Microsoft.NETCore.App 9.") && !output.Contains("Microsoft.AspNetCore.App 9."))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("⚠️  .NET 9.0 Runtime not detected. Please install it from https://dotnet.microsoft.com/download");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine("✅ .NET 9.0 Runtime found");
            }
        }
        catch
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("⚠️  Could not verify .NET installation. Please ensure .NET 9.0 Runtime is installed.");
            Console.ResetColor();
        }
    }

    private static string FindSourceDirectory()
    {
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var baseDir = Path.GetDirectoryName(assemblyLocation);
        
        // Look for the built Spotify.MCP.Host files
        string[] searchPaths = {
            Path.Combine(baseDir!, "..", "Spotify.MCP.Host"),
            Path.Combine(baseDir!, "..", "..", "Spotify.MCP.Host", "bin", "Release", "net9.0"),
            Path.Combine(baseDir!, "..", "..", "Spotify.MCP.Host", "bin", "Debug", "net9.0"),
            Path.Combine(baseDir!, "Spotify.MCP.Host")
        };

        foreach (var searchPath in searchPaths)
        {
            var fullPath = Path.GetFullPath(searchPath);
            if (Directory.Exists(fullPath) && File.Exists(Path.Combine(fullPath, "Spotify.MCP.Host.exe")))
            {
                return fullPath;
            }
        }

        throw new FileNotFoundException("Could not find Spotify.MCP.Host.exe. Please build the project first.");
    }

    private static async Task CopyApplicationFilesAsync(string sourceDir, string installPath)
    {
        var filesToCopy = new[]
        {
            "Spotify.MCP.Host.exe",
            "Spotify.MCP.Host.dll",
            "appsettings.json",
            "Spotify.MCP.Host.runtimeconfig.json",
            "Spotify.MCP.Host.deps.json"
        };

        foreach (var file in filesToCopy)
        {
            var sourcePath = Path.Combine(sourceDir, file);
            var destPath = Path.Combine(installPath, file);
            
            if (File.Exists(sourcePath))
            {
                File.Copy(sourcePath, destPath, true);
                Console.WriteLine($"  ✅ {file}");
            }
            else
            {
                Console.WriteLine($"  ⚠️  {file} (not found)");
            }
        }

        // Copy all DLL dependencies
        foreach (var dllFile in Directory.GetFiles(sourceDir, "*.dll"))
        {
            var fileName = Path.GetFileName(dllFile);
            var destPath = Path.Combine(installPath, fileName);
            File.Copy(dllFile, destPath, true);
        }

        // Copy README if available
        var readmePath = Path.Combine(sourceDir, "README.md");
        if (File.Exists(readmePath))
        {
            File.Copy(readmePath, Path.Combine(installPath, "README.md"), true);
        }
    }

    private static void CreateConfigurationFiles(string installPath)
    {
        var configDir = Path.Combine(installPath, "Config");
        Directory.CreateDirectory(configDir);

        // Sample appsettings.json
        var sampleConfig = new
        {
            Logging = new
            {
                LogLevel = new
                {
                    Default = "Information",
                    Microsoft = new { Hosting = new { Lifetime = "Information" } }
                }
            },
            Spotify = new
            {
                ClientId = "YOUR_SPOTIFY_CLIENT_ID_HERE",
                ClientSecret = "YOUR_SPOTIFY_CLIENT_SECRET_HERE"
            }
        };

        var sampleConfigJson = JsonSerializer.Serialize(sampleConfig, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Path.Combine(configDir, "appsettings.sample.json"), sampleConfigJson);

        // Sample Claude Desktop configuration
        var exePath = Path.Combine(installPath, "Spotify.MCP.Host.exe").Replace("\\", "\\\\");
        var claudeConfig = new
        {
            mcpServers = new
            {
                spotify = new
                {
                    command = exePath
                }
            }
        };

        var claudeConfigJson = JsonSerializer.Serialize(claudeConfig, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Path.Combine(configDir, "claude_desktop_config.sample.json"), claudeConfigJson);

        Console.WriteLine("  ✅ Configuration samples created");
    }

    private static async Task ConfigureClaudeDesktopAsync(string installPath)
    {
        var claudeConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Claude", "claude_desktop_config.json");
        var claudeDir = Path.GetDirectoryName(claudeConfigPath)!;

        Directory.CreateDirectory(claudeDir);

        var config = new Dictionary<string, object>();
        
        // Load existing configuration
        if (File.Exists(claudeConfigPath))
        {
            try
            {
                var existingJson = await File.ReadAllTextAsync(claudeConfigPath);
                var existingConfig = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(existingJson);
                if (existingConfig != null)
                {
                    foreach (var kvp in existingConfig)
                    {
                        config[kvp.Key] = kvp.Value;
                    }
                }
            }
            catch
            {
                Console.WriteLine("  ⚠️  Could not parse existing Claude configuration. Creating new configuration.");
            }
        }

        // Ensure mcpServers exists
        if (!config.ContainsKey("mcpServers"))
        {
            config["mcpServers"] = new Dictionary<string, object>();
        }

        // Add Spotify MCP Server configuration
        var mcpServers = config["mcpServers"] as Dictionary<string, object> ?? new Dictionary<string, object>();
        mcpServers["spotify"] = new Dictionary<string, object>
        {
            ["command"] = Path.Combine(installPath, "Spotify.MCP.Host.exe")
        };
        config["mcpServers"] = mcpServers;

        // Save configuration
        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(claudeConfigPath, json);

        Console.WriteLine($"  ✅ Claude Desktop configured at: {claudeConfigPath}");
        Console.WriteLine("  🔄 Please restart Claude Desktop for changes to take effect.");
    }

    private static async Task RemoveClaudeDesktopConfigAsync()
    {
        var claudeConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Claude", "claude_desktop_config.json");

        if (!File.Exists(claudeConfigPath))
        {
            Console.WriteLine("  ✅ No Claude Desktop configuration found");
            return;
        }

        try
        {
            var json = await File.ReadAllTextAsync(claudeConfigPath);
            var config = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

            if (config != null && config.ContainsKey("mcpServers"))
            {
                var mcpServers = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(config["mcpServers"].GetRawText());
                if (mcpServers != null && mcpServers.ContainsKey("spotify"))
                {
                    mcpServers.Remove("spotify");
                    config["mcpServers"] = JsonSerializer.SerializeToElement(mcpServers);

                    var updatedJson = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                    await File.WriteAllTextAsync(claudeConfigPath, updatedJson);
                    Console.WriteLine("  ✅ Removed from Claude Desktop configuration");
                }
            }
        }
        catch
        {
            Console.WriteLine("  ⚠️  Could not update Claude Desktop configuration");
        }
    }

    private static void CreateShortcuts(string installPath)
    {
        // This would require additional COM interop to create .lnk files
        // For now, we'll just create a batch file that opens the config folder
        var batchContent = $@"@echo off
echo Opening Spotify MCP Server configuration...
start """" ""{Path.Combine(installPath, "Config")}""
";
        var batchPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Spotify MCP Server Config.bat");
        File.WriteAllText(batchPath, batchContent);
        Console.WriteLine("  ✅ Desktop shortcut created");
    }

    private static void RemoveShortcuts()
    {
        var batchPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Spotify MCP Server Config.bat");
        if (File.Exists(batchPath))
        {
            File.Delete(batchPath);
            Console.WriteLine("  ✅ Desktop shortcut removed");
        }
    }

    private static void UpdateSystemPath(string installPath, bool add)
    {
        try
        {
            var currentPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine) ?? "";
            var pathEntries = currentPath.Split(';').ToList();

            if (add)
            {
                if (!pathEntries.Contains(installPath, StringComparer.OrdinalIgnoreCase))
                {
                    pathEntries.Add(installPath);
                    Environment.SetEnvironmentVariable("PATH", string.Join(";", pathEntries), EnvironmentVariableTarget.Machine);
                    Console.WriteLine("  ✅ Added to system PATH");
                }
                else
                {
                    Console.WriteLine("  ✅ Already in system PATH");
                }
            }
            else
            {
                if (pathEntries.RemoveAll(p => string.Equals(p, installPath, StringComparison.OrdinalIgnoreCase)) > 0)
                {
                    Environment.SetEnvironmentVariable("PATH", string.Join(";", pathEntries), EnvironmentVariableTarget.Machine);
                    Console.WriteLine("  ✅ Removed from system PATH");
                }
                else
                {
                    Console.WriteLine("  ✅ Not found in system PATH");
                }
            }
        }
        catch
        {
            Console.WriteLine("  ⚠️  Could not update system PATH (requires administrator privileges)");
        }
    }

    private static void StopRunningProcesses()
    {
        try
        {
            var processes = Process.GetProcessesByName("Spotify.MCP.Host");
            if (processes.Length > 0)
            {
                Console.WriteLine("🛑 Stopping running processes...");
                foreach (var process in processes)
                {
                    process.Kill();
                    process.WaitForExit(5000);
                }
                Console.WriteLine("  ✅ Processes stopped");
            }
        }
        catch
        {
            Console.WriteLine("  ⚠️  Could not stop running processes");
        }
    }

    private static void PrintNextSteps(string installPath)
    {
        Console.WriteLine("📋 Next Steps:");
        Console.WriteLine("1. 🎵 Get Spotify API credentials from https://developer.spotify.com/dashboard");
        Console.WriteLine($"2. ⚙️  Update appsettings.json with your credentials:");
        Console.WriteLine($"   {Path.Combine(installPath, "appsettings.json")}");
        Console.WriteLine("3. 🔄 Restart Claude Desktop if it's running");
        Console.WriteLine("4. 🧪 Test the installation by running:");
        Console.WriteLine($"   \"{Path.Combine(installPath, "Spotify.MCP.Host.exe")}\"");
        Console.WriteLine();
        Console.WriteLine($"📁 Configuration files and documentation are in:");
        Console.WriteLine($"   {Path.Combine(installPath, "Config")}");
    }

    private static string? GetArgumentValue(string[] args, string argumentName)
    {
        var index = Array.IndexOf(args, argumentName);
        return index >= 0 && index < args.Length - 1 ? args[index + 1] : null;
    }
}