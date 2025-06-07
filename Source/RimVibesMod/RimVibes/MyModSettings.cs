using RimVibes.EventHandling;
using RimVibes.UI;
using UnityEngine;
using Verse;

namespace RimVibes;

public class MyModSettings : ModSettings
{
    private int hudAnchorInt = 6;

    public Vector2 HUDOffset = new Vector2(0f, 200f);
    public float HUDScale = 1f;

    private int hudVisInt = 1;

    public float IconAlpha = 0.5f;

    public bool LaunchDebugWindow;

    public Vector2 MainMenuButtonOffset = new Vector2(0f, 0f);

    private int pauseModeInt = 2;

    public bool ShouldSilenceVanillaMusic = true;

    public HUDAnchor HUDAnchor
    {
        get => (HUDAnchor)hudAnchorInt;
        set => hudAnchorInt = (int)value;
    }

    public HUDVisibility HUDVisibility
    {
        get => (HUDVisibility)hudVisInt;
        set => hudVisInt = (int)value;
    }

    public SongPauseMode SongPauseMode
    {
        get => (SongPauseMode)pauseModeInt;
        set => pauseModeInt = (int)value;
    }

    public Responses Responses { get; } = new Responses();


    public override void ExposeData()
    {
        Scribe_Values.Look(ref LaunchDebugWindow, "RV_LaunchDebugWindow");
        Scribe_Values.Look(ref HUDScale, "RV_HUDScale", 1f);
        Scribe_Values.Look(ref hudAnchorInt, "RV_HUDAnchor", 6);
        Scribe_Values.Look(ref hudVisInt, "RV_HUDVisibility", 1);
        Scribe_Values.Look(ref HUDOffset, "RV_HUDOffset", new Vector2(0f, 200f));
        Scribe_Values.Look(ref IconAlpha, "RV_IconAlpha", 0.5f);
        Scribe_Values.Look(ref pauseModeInt, "RV_SongPauseMode", 2);
        Scribe_Values.Look(ref ShouldSilenceVanillaMusic, "RV_ShouldSilenceVanillaMusic", true);
        var value = Responses.Serialize();
        Scribe_Values.Look(ref value, "RV_ResponsesData", "");
        Responses.Deserialize(value);
        Scribe_Values.Look(ref MainMenuButtonOffset, "RV_MainMenuButtonOffset", Vector2.zero);
    }
}