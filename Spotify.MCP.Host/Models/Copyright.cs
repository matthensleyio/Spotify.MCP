using System.Text.Json.Serialization;

namespace Spotify.MCP.Host.Models.Output;

public record Copyright(
    string Text,
    string Type
);
