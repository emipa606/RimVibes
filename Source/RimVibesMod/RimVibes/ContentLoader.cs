using RimVibes.UI;
using UnityEngine;
using Verse;

namespace RimVibes;

[StaticConstructorOnStartup]
internal static class ContentLoader
{
    public static readonly Camera cam;

    public static readonly Texture2D LogoAndTitle;

    public static readonly Texture2D StatusNormal;

    public static readonly Texture2D StatusWarn;

    public static readonly Texture2D StatusError;

    public static readonly Texture2D StatusUnknown;

    public static readonly Texture2D BinIcon;

    static ContentLoader()
    {
        //Log.Message("Loading RimVibes content.");
        StatusError = ContentFinder<Texture2D>.Get("StatusIconError");
        StatusWarn = ContentFinder<Texture2D>.Get("StatusIconWarn");
        StatusUnknown = ContentFinder<Texture2D>.Get("StatusIconUnknown");
        StatusNormal = ContentFinder<Texture2D>.Get("StatusIconNormal");
        HUD.NextButton = ContentFinder<Texture2D>.Get("NextButton");
        HUD.PreviousButton = ContentFinder<Texture2D>.Get("PreviousButton");
        HUD.PlayButton = ContentFinder<Texture2D>.Get("PlayButton");
        HUD.PauseButton = ContentFinder<Texture2D>.Get("PauseButton");
        HUD.VolumeIcon = ContentFinder<Texture2D>.Get("VolumeIcon");
        HUD.SettingsIcon = ContentFinder<Texture2D>.Get("SettingsIcon");
        HUD.RepeatButtonNormal = ContentFinder<Texture2D>.Get("RepeatButtonNormal");
        HUD.RepeatButtonActive = ContentFinder<Texture2D>.Get("RepeatButtonActive");
        HUD.RepeatButtonOne = ContentFinder<Texture2D>.Get("RepeatButtonOne");
        HUD.ShuffleButtonNormal = ContentFinder<Texture2D>.Get("ShuffleButtonNormal");
        HUD.ShuffleButtonActive = ContentFinder<Texture2D>.Get("ShuffleButtonActive");
        HUD.MissingAlbumArt = ContentFinder<Texture2D>.Get("MissingAlbumArt");
        HUD.HideIconNormal = ContentFinder<Texture2D>.Get("HUDIconNormal");
        HUD.HideIconRed = ContentFinder<Texture2D>.Get("HUDIconRed");
        HUD.HideIconGreen = ContentFinder<Texture2D>.Get("HUDIconGreen");
        HUD.CustomBox = ContentFinder<Texture2D>.Get("CustomBox");
        BinIcon = ContentFinder<Texture2D>.Get("BinIcon");
        cam = new GameObject().AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.Depth;
        cam.orthographic = true;
        cam.enabled = false;
        cam.cullingMask = LayerMask.GetMask("UI");
        Object.DontDestroyOnLoad(cam.gameObject);
        LogoAndTitle = ContentFinder<Texture2D>.Get("LogoAndTitle");
    }
}