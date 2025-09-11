using System.Text.Json.Serialization;

namespace Spotify.MCP.Host.Models;

public record PlaybackActions(
    bool Interrupting_playback,
    bool Pausing,
    bool Resuming,
    bool Seeking,
    [property: JsonPropertyName("skipping_next")] bool SkippingNext,
    [property: JsonPropertyName("skipping_prev")] bool SkippingPrev,
    [property: JsonPropertyName("toggling_repeat_context")] bool TogglingRepeatContext,
    [property: JsonPropertyName("toggling_shuffle")] bool TogglingShuffle,
    [property: JsonPropertyName("toggling_repeat_track")] bool TogglingRepeatTrack,
    [property: JsonPropertyName("transferring_playback")] bool TransferringPlayback
);
