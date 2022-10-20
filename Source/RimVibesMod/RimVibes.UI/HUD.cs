using System;
using System.Diagnostics;
using Common;
using RimVibes.Components;
using RimVibes.IO;
using UnityEngine;
using Verse;

namespace RimVibes.UI;

[StaticConstructorOnStartup]
public static class HUD
{
    public static Texture2D MissingAlbumArt;

    public static Texture2D VolumeIcon;

    public static Texture2D SettingsIcon;

    public static Texture2D PauseButton;

    public static Texture2D PlayButton;

    public static Texture2D NextButton;

    public static Texture2D PreviousButton;

    public static Texture2D ShuffleButtonNormal;

    public static Texture2D ShuffleButtonActive;

    public static Texture2D RepeatButtonNormal;

    public static Texture2D RepeatButtonActive;

    public static Texture2D RepeatButtonOne;

    public static Texture2D HideIconNormal;

    public static Texture2D HideIconGreen;

    public static Texture2D HideIconRed;

    public static Texture2D CustomBox;

    public static bool IsPlaying;

    public static bool IsShuffling;

    public static byte RepeatMode;

    private static float volume;

    private static float normalizedTime;

    private static bool isHidden;

    private static float hideLerp;

    private static readonly Stopwatch hideTimer = new Stopwatch();

    private static readonly Stopwatch deltaTimer = new Stopwatch();

    private static bool isChangingTime;

    private static bool isChangingVolume;

    public static bool IsHidden
    {
        get => isHidden;
        set
        {
            if (isHidden == value)
            {
                return;
            }

            isHidden = value;
            RimVibesMod.TrySendExecute(delegate(NetData msg) { msg.Write((byte)(isHidden ? 7 : 6)); });
        }
    }

    public static float HideTime { get; set; } = 0.25f;


    public static int TimeToAutoHideMS { get; set; } = 2500;


    public static float HideScale { get; private set; }

    public static float HiddenIconAlpha => RimVibesMod.Instance.Settings.IconAlpha;

    public static void Draw(Rect rect)
    {
        if (!RimVibesMod.HasShownMainMenu)
        {
            return;
        }

        if (!hideTimer.IsRunning)
        {
            hideTimer.Start();
        }

        var instance = RimVibesMod.Instance;
        var hUDVisibility = instance.Settings.HUDVisibility;
        switch (hUDVisibility)
        {
            case HUDVisibility.AlwaysVisible:
                IsHidden = false;
                break;
            case HUDVisibility.AutoHide:
                IsHidden = hideTimer.ElapsedMilliseconds >= TimeToAutoHideMS ||
                           RimVibesMod.Instance.Status == Vibe.Disconnected;
                break;
        }

        if (IsMouseInBounds(rect, 25f))
        {
            hideTimer.Restart();
        }

        if (!Terms.HasUserAgreed)
        {
            IsHidden = true;
        }

        UpdateHiding();
        if (HideScale == 0f)
        {
            return;
        }

        var playbackState = instance.PlaybackState;
        var text = playbackState.Item.Name ?? "RiVi.Nothing".Translate();
        var text2 = playbackState.Item.ArtistName ?? "";
        var url = playbackState.Item.Album.HasImage ? playbackState.Item.Album.ImageURL : null;
        var imageWidth = playbackState.Item.Album.ImageWidth;
        var imageHeight = playbackState.Item.Album.ImageHeight;
        var timeSpan = TimeSpan.FromMilliseconds(playbackState.ProgressMS);
        var timeSpan2 = TimeSpan.FromMilliseconds(playbackState.Item.LengthMS);
        var box = GUI.skin.box;
        var background = box.normal.background;
        box.normal.background = CustomBox;
        GUI.Box(rect, "");
        GUI.color = new Color(0f, 0f, 0f, 0f);
        GUI.color = Color.white;
        rect.x += 5f;
        rect.y += 5f;
        rect.width -= 10f;
        rect.height -= 10f;
        GUI.DrawTexture(new Rect(rect.x, rect.y, 128f, 128f), ImageCache.GetImage(url, imageWidth, imageHeight),
            ScaleMode.ScaleToFit, true, 0f, Color.white, 0f, 3f);
        MoveRight(138f);
        SetFontSize(22);
        GUI.Label(new Rect(rect.x, rect.y, rect.xMax - rect.x, 30f), text);
        MoveDown(28f);
        SetFontSize(18);
        GUI.Label(new Rect(rect.x, rect.y, rect.xMax - rect.x, 30f), $"<b>{text2}</b>");
        MoveDown(28f);
        GUI.color = new Color(0f, 0f, 0f, 0f);
        if (GUI.Button(new Rect(rect.x + ((rect.width - 32f) * 0.5f), rect.y, 32f, 32f), ""))
        {
            TogglePlay();
        }

        GUI.color = Color.white;
        GUI.DrawTexture(new Rect(rect.x + ((rect.width - 32f) * 0.5f), rect.y, 32f, 32f),
            IsPlaying ? PauseButton : PlayButton);
        GUI.color = new Color(0f, 0f, 0f, 0f);
        if (GUI.Button(new Rect(rect.x + ((rect.width - 32f) * 0.5f) - 35f, rect.y + 8f, 16f, 16f).ExpandedBy(4f), ""))
        {
            PreviousTrack();
        }

        GUI.color = Color.white;
        GUI.DrawTexture(new Rect(rect.x + ((rect.width - 32f) * 0.5f) - 35f, rect.y + 8f, 16f, 16f), PreviousButton);
        GUI.color = new Color(0f, 0f, 0f, 0f);
        if (GUI.Button(new Rect(rect.x + ((rect.width - 32f) * 0.5f) + 35f + 16f, rect.y + 8f, 16f, 16f).ExpandedBy(4f),
                ""))
        {
            NextTrack();
        }

        GUI.color = Color.white;
        GUI.DrawTexture(new Rect(rect.x + ((rect.width - 32f) * 0.5f) + 35f + 16f, rect.y + 8f, 16f, 16f), NextButton);
        GUI.color = new Color(0f, 0f, 0f, 0f);
        if (GUI.Button(new Rect(rect.x + ((rect.width - 32f) * 0.5f) - 70f, rect.y + 8f, 16f, IsShuffling ? 24 : 16),
                ""))
        {
            ToggleShuffle();
        }

        GUI.color = Color.white;
        GUI.DrawTexture(new Rect(rect.x + ((rect.width - 32f) * 0.5f) - 70f, rect.y + 8f, 16f, IsShuffling ? 24 : 16),
            IsShuffling ? ShuffleButtonActive : ShuffleButtonNormal);
        GUI.color = new Color(0f, 0f, 0f, 0f);
        if (GUI.Button(
                new Rect(rect.x + ((rect.width - 32f) * 0.5f) + 84f, rect.y + 8f, 16f, RepeatMode != 0 ? 24 : 16), ""))
        {
            ToggleRepeatMode();
        }

        GUI.color = Color.white;
        GUI.DrawTexture(
            new Rect(rect.x + ((rect.width - 32f) * 0.5f) + 84f, rect.y + 8f, 16f, RepeatMode != 0 ? 24 : 16),
            RepeatMode == 0 ? RepeatButtonNormal : RepeatMode == 1 ? RepeatButtonActive : RepeatButtonOne);
        MoveDown(38f);
        GUI.changed = false;
        normalizedTime =
            GUI.HorizontalSlider(new Rect(rect.x + 30f, rect.y, rect.width - 65f, 20f), normalizedTime, 0f, 1f);
        if (GUI.changed)
        {
            isChangingTime = true;
        }
        else if (isChangingTime && !Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2))
        {
            isChangingTime = false;
            SetTime(normalizedTime);
        }

        if (!isChangingTime)
        {
            if (playbackState.Item.LengthMS != 0)
            {
                normalizedTime = playbackState.ProgressMS / (float)playbackState.Item.LengthMS;
            }
            else
            {
                normalizedTime = 0f;
            }
        }

        SetFontSize(12);
        if (!isChangingTime)
        {
            GUI.Label(new Rect(rect.x, rect.y - 4f, 40f, 20f), $"{timeSpan.Minutes}:{timeSpan.Seconds:D2}");
        }
        else
        {
            var timeSpan3 = TimeSpan.FromMilliseconds(timeSpan2.TotalMilliseconds * normalizedTime);
            GUI.Label(new Rect(rect.x, rect.y - 4f, 40f, 20f), $"{timeSpan3.Minutes}:{timeSpan3.Seconds:D2}");
        }

        GUI.Label(new Rect(rect.xMax - 30f, rect.y - 4f, 40f, 20f), $"{timeSpan2.Minutes}:{timeSpan2.Seconds:D2}");
        MoveDown(30f);
        GUI.changed = false;
        volume = GUI.HorizontalSlider(new Rect(rect.xMax - 80f, rect.yMax - 15f, 70f, 20f), volume, 0f, 100f);
        if (GUI.changed)
        {
            isChangingVolume = true;
        }
        else if (isChangingVolume && !Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2))
        {
            isChangingVolume = false;
            SetVolume((int)volume);
        }

        if (!isChangingVolume)
        {
            volume = playbackState.Device.VolumePercentage;
        }

        GUI.DrawTexture(new Rect(rect.xMax - 100f, rect.yMax - 17f, 16f, 16f), VolumeIcon);
        if (GUI.Button(new Rect(rect.xMax - 24f + 5f, rect.yMax + 10f, 24f, 24f), SettingsIcon))
        {
            MainUI.Open();
        }

        if (hUDVisibility == HUDVisibility.ManualHide)
        {
            SetFontSize(20);
            if (GUI.Button(new Rect(rect.xMax - 24f - 5f - 60f, rect.yMax + 10f, 60f, 24f), "RiVi.Hide".Translate()))
            {
                IsHidden = true;
            }
        }

        box.normal.background = background;

        void MoveDown(float amount)
        {
            rect.y += amount;
            rect.height -= amount;
        }

        void MoveRight(float amount)
        {
            rect.x += amount;
            rect.width -= amount;
        }
    }

    public static void DrawHiddenSafe(Rect rect)
    {
        if (!RimVibesMod.HasShownMainMenu)
        {
            return;
        }

        var rect2 = Rect.zero;
        switch (RimVibesMod.Instance.Settings.HUDAnchor)
        {
            case HUDAnchor.Left:
            case HUDAnchor.TopLeft:
            case HUDAnchor.BottomLeft:
                rect2 = new Rect(rect.x + 10f, rect.y + (rect.height * 0.5f) - 32f, 64f, 64f);
                break;
            case HUDAnchor.Right:
            case HUDAnchor.TopRight:
            case HUDAnchor.BottomRight:
                rect2 = new Rect(rect.xMax - 10f - 64f, rect.y + (rect.height * 0.5f) - 32f, 64f, 64f);
                break;
            case HUDAnchor.Top:
                rect2 = new Rect(rect.x + (rect.width * 0.5f) - 32f, rect.y + 10f, 64f, 64f);
                break;
            case HUDAnchor.Bottom:
                rect2 = new Rect(rect.x + (rect.width * 0.5f) - 32f, rect.yMax - 64f - 10f, 64f, 64f);
                break;
            case HUDAnchor.Free:
                rect2 = Rect.zero;
                rect2.center = rect.center;
                rect2.size = new Vector2(64f, 64f);
                break;
        }

        var position = rect2.ExpandedBy(20f);
        GUI.color = new Color(0f, 0f, 0f, 0f);
        GUI.Button(position, "");
        GUI.color = Color.white;
        if (!IsHidden)
        {
            return;
        }

        var matrix = HookComponent.Matrix;
        Vector2 min = matrix.MultiplyPoint(position.min);
        Vector2 max = matrix.MultiplyPoint(position.max);
        position.min = min;
        position.max = max;
        Vector2 point = Input.mousePosition;
        point.y = Screen.height - point.y;
        GUI.color = new Color(1f, 1f, 1f, HiddenIconAlpha);
        GUI.DrawTexture(rect2, position.Contains(point) ? HideIconGreen : HideIconNormal);
        GUI.color = Color.white;
        if (!position.Contains(point) || !Input.GetMouseButtonDown(0))
        {
            return;
        }

        IsHidden = false;
        hideTimer.Restart();
    }

    private static void TogglePlay()
    {
        IsPlaying = !IsPlaying;
        RimVibesMod.TrySendExecute(delegate(NetData msg)
        {
            msg.Write((byte)1);
            msg.Write(false);
            msg.Write(false);
        });
    }

    private static void ToggleShuffle()
    {
        IsShuffling = !IsShuffling;
        RimVibesMod.TrySendExecute(delegate(NetData msg) { msg.Write((byte)8); });
    }

    private static void ToggleRepeatMode()
    {
        RepeatMode++;
        if (RepeatMode == 3)
        {
            RepeatMode = 0;
        }

        RimVibesMod.TrySendExecute(delegate(NetData msg) { msg.Write((byte)9); });
    }

    private static void PreviousTrack()
    {
        RimVibesMod.TrySendExecute(delegate(NetData msg)
        {
            msg.Write((byte)11);
            msg.Write(false);
        });
    }

    private static void NextTrack()
    {
        RimVibesMod.TrySendExecute(delegate(NetData msg) { msg.Write((byte)10); });
    }

    private static void SetVolume(int percentage)
    {
        percentage = Mathf.Clamp(percentage, 0, 100);
        RimVibesMod.TrySendExecute(delegate(NetData msg)
        {
            msg.Write((byte)13);
            msg.Write(percentage);
        });
        RimVibesMod.Instance.PlaybackState.Device.VolumePercentage = percentage;
    }

    private static void SetTime(float normalized)
    {
        var lengthMS = RimVibesMod.Instance.PlaybackState.Item.LengthMS;
        var seekMS = (int)(lengthMS * normalized);
        RimVibesMod.TrySendExecute(delegate(NetData msg)
        {
            msg.Write((byte)12);
            msg.Write(seekMS);
        });
        RimVibesMod.Instance.PlaybackState.ProgressMS = seekMS;
    }

    private static void SetFontSize(int size)
    {
        GUI.skin.label.fontSize = size;
    }

    private static void UpdateHiding()
    {
        if (IsHidden)
        {
            hideLerp += (float)deltaTimer.Elapsed.TotalSeconds / HideTime;
        }
        else
        {
            hideLerp -= (float)deltaTimer.Elapsed.TotalSeconds / HideTime;
        }

        hideLerp = Mathf.Clamp01(hideLerp);
        deltaTimer.Restart();
        HideScale = 1f - hideLerp;
        if (HideScale <= 0)
        {
            HideScale = 0.001f;
        }
    }

    private static bool IsMouseInBounds(Rect rect, float exteriorPadding)
    {
        rect = rect.ExpandedBy(exteriorPadding);
        Vector2 point = Input.mousePosition;
        point.y = Screen.height - point.y;
        var matrix = HookComponent.Matrix;
        Vector2 min = matrix.MultiplyPoint(rect.min);
        Vector2 max = matrix.MultiplyPoint(rect.max);
        rect.min = min;
        rect.max = max;
        return rect.Contains(point);
    }
}