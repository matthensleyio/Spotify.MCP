using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using ModelContextProtocol.Server;
using Spotify.MCP.Host.Services;
using System.ComponentModel;
using System.Reflection;
using System.IO;

namespace Spotify.MCP.Host
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            // Print available tools on startup
            PrintAvailableTools();

            var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder(args);

            builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            
            // Parse command line arguments for Spotify credentials
            builder.Configuration.AddCommandLine(args, new Dictionary<string, string>
            {
                ["--spotify-client-id"] = "Spotify:ClientId",
                ["--spotify-client-secret"] = "Spotify:ClientSecret"
            });

            builder.Logging.AddConsole(consoleLogOptions =>
            {
                consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
            });

            builder.Services
                .AddHttpClient()
                .AddScoped<ISpotifyAuthService, SpotifyAuthService>()
                .AddScoped<ISpotifyApiService, SpotifyApiService>()
                .AddMcpServer()
                .WithStdioServerTransport()
                .WithToolsFromAssembly();

            await builder.Build().RunAsync();
        }

        private static void PrintAvailableTools()
        {
            Console.Error.WriteLine("=== Available Spotify MCP Tools ===");
            Console.Error.WriteLine();

            var assembly = Assembly.GetExecutingAssembly();
            var toolTypes = assembly.GetTypes()
                .Where(type => type.GetCustomAttribute<McpServerToolTypeAttribute>() != null)
                .OrderBy(type => type.Name);

            foreach (var toolType in toolTypes)
            {
                Console.Error.WriteLine($"[{toolType.Name}]");
                
                var methods = toolType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(method => method.GetCustomAttribute<McpServerToolAttribute>() != null)
                    .OrderBy(method => method.Name);

                foreach (var method in methods)
                {
                    var descriptionAttr = method.GetCustomAttribute<DescriptionAttribute>();
                    var description = descriptionAttr?.Description ?? "No description available";
                    
                    Console.Error.WriteLine($"  - {method.Name}: {description}");
                }
                
                Console.Error.WriteLine();
            }

            Console.Error.WriteLine("=== End of Available Tools ===");
            Console.Error.WriteLine();
        }
    }
}
