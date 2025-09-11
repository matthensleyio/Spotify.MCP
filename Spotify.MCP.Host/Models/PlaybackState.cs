using System.Text.Json.Serialization;

namespace Spotify.MCP.Host.Models;

public record PlaybackState(
    Device Device,
    [property: JsonPropertyName("repeat_state")] string RepeatState,
    [property: JsonPropertyName("shuffle_state")] bool ShuffleState,
    PlaybackContext? Context,
    long Timestamp,
    [property: JsonPropertyName("progress_ms")] int? ProgressMs,
    [property: JsonPropertyName("is_playing")] bool IsPlaying,
    Track? Item,
    [property: JsonPropertyName("currently_playing_type")] string CurrentlyPlayingType,
    PlaybackActions Actions
);
