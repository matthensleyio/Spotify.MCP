namespace Spotify.MCP.Host.Models.Output;

/// <summary>
/// Standardized error response returned by all tools
/// </summary>
public record ErrorResponse(
    bool error = true,
    string message = "",
    string? code = null,
    string? details = null)
{
    /// <summary>
    /// Creates an error response with a message
    /// </summary>
    public static ErrorResponse FromMessage(string message, string? code = null, string? details = null) =>
        new(error: true, message, code, details);

    /// <summary>
    /// Creates an error response from an exception
    /// </summary>
    public static ErrorResponse FromException(Exception ex, string? code = null) =>
        new(error: true, message: ex.Message, code, details: ex.StackTrace);
}
