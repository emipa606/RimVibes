using Common;
using RimVibes.UI;

namespace RimVibes;

public static class Handlers
{
    internal static void HandlePing(NetData data)
    {
        RimVibesMod.Instance.PingTimer.Restart();
        RimVibesMod.Instance.Status = data.ReadBoolean() ? Vibe.ConnectedReady : Vibe.ConnectedNoAuth;
    }

    internal static void HandlePlaybackState(NetData data)
    {
        RimVibesMod.Instance.PlaybackState.Deserialize(data);
        var playbackState = RimVibesMod.Instance.PlaybackState;
        HUD.IsPlaying = playbackState.IsPlaying;
        HUD.IsShuffling = playbackState.ShuffleState;
        HUD.RepeatMode =
            (byte)(playbackState.RepeatState != "Off" ? playbackState.RepeatState != "Track" ? 1 : 2 : 0);
    }

    internal static void HandleAuthError(NetData data)
    {
        var fromLogin = data.ReadBoolean();
        var reason = data.ReadString();
        AuthErrorUI.Open(fromLogin, reason);
    }
}