using System;
using UnityEngine;

namespace RimVibes.Utils;

public static class VibeExtensions
{
    public static string ToReadable(this Vibe vibe)
    {
        return vibe switch
        {
            Vibe.Disconnected => "Disconnected (Process not started)",
            Vibe.ConnectedNoAuth => "Spotify not authorized",
            Vibe.ConnectedReady => "Ready!",
            Vibe.NotResponding => "Not responding (Process not responding)",
            _ => throw new ArgumentOutOfRangeException(nameof(vibe), vibe, null)
        };
    }

    public static Texture2D GetIcon(this Vibe vibe)
    {
        return vibe switch
        {
            Vibe.Disconnected => ContentLoader.StatusError,
            Vibe.ConnectedNoAuth => ContentLoader.StatusWarn,
            Vibe.ConnectedReady => ContentLoader.StatusNormal,
            Vibe.NotResponding => ContentLoader.StatusUnknown,
            _ => throw new ArgumentOutOfRangeException(nameof(vibe), vibe, null)
        };
    }
}