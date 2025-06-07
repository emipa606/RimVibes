using System;
using UnityEngine;
using Verse;

namespace RimVibes.Utils;

public static class VibeExtensions
{
    public static string ToReadable(this Vibe vibe)
    {
        return vibe switch
        {
            Vibe.Disconnected => "RiVi.Disconnected".Translate(),
            Vibe.ConnectedNoAuth => "RiVi.NotAuthorized".Translate(),
            Vibe.ConnectedReady => "RiVi.Ready".Translate(),
            Vibe.NotResponding => "RiVi.NotResponding".Translate(),
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