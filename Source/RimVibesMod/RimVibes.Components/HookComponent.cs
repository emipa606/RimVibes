using System;
using RimVibes.UI;
using UnityEngine;
using Verse;

namespace RimVibes.Components;

public class HookComponent : MonoBehaviour
{
    public static Rect HUDBounds;

    public static Matrix4x4 OldMatrix;

    public static Matrix4x4 Matrix;

    public bool DrawLogo
    {
        get
        {
            try
            {
                if (Current.ProgramState == ProgramState.Entry)
                {
                    return Find.WindowStack.Count <= (Prefs.DevMode ? 1 : 0);
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }
    }

    public event Action OnExit;

    public void OnApplicationQuit()
    {
        OnExit?.Invoke();
    }

    public void OnGUI()
    {
        var hUDScale = RimVibesMod.Instance.Settings.HUDScale;
        var hUDOffset = RimVibesMod.Instance.Settings.HUDOffset;
        var hUDAnchor = RimVibesMod.Instance.Settings.HUDAnchor;
        var hidescale = HUD.HideScale;
        var num = 225f * hUDScale;
        var num2 = 69f * hUDScale;
        Vector2 vector = Vector3.zero;
        switch (hUDAnchor)
        {
            case HUDAnchor.Right:
                vector = hUDOffset + new Vector2(Screen.width - num - 10f, Screen.height * 0.5f);
                break;
            case HUDAnchor.Left:
                vector = hUDOffset + new Vector2(10f + num, Screen.height * 0.5f);
                break;
            case HUDAnchor.Top:
                vector = hUDOffset + new Vector2(Screen.width * 0.5f, 10f + num2);
                break;
            case HUDAnchor.Bottom:
                vector = hUDOffset + new Vector2(Screen.width * 0.5f, Screen.height - 10f - num2 - 32f);
                break;
            case HUDAnchor.BottomLeft:
                vector = hUDOffset + new Vector2(10f + num, Screen.height - 10f - num2);
                break;
            case HUDAnchor.BottomRight:
                vector = hUDOffset + new Vector2(Screen.width - num - 10f, Screen.height - 10f - num2);
                break;
            case HUDAnchor.TopLeft:
                vector = hUDOffset + new Vector2(10f + num, 10f + num2);
                break;
            case HUDAnchor.TopRight:
                vector = hUDOffset + new Vector2(Screen.width - num - 10f, 10f + num2);
                break;
            case HUDAnchor.Free:
                vector = hUDOffset + (new Vector2(Screen.width, Screen.height) * 0.5f);
                break;
        }

        if (hidescale <= 0)
        {
            hidescale = 0.001f;
        }

        HUDBounds = new Rect(-225f, -69f, 450f, 138f);
        OldMatrix = GUI.matrix;
        var matrix = Matrix4x4.TRS(vector, Quaternion.identity,
            Vector3.one * RimVibesMod.Instance.Settings.HUDScale * hidescale);
        GUI.matrix = Matrix = matrix;
        HUD.Draw(HUDBounds);
        GUI.matrix = Matrix = Matrix4x4.TRS(vector, Quaternion.identity,
            Vector3.one * RimVibesMod.Instance.Settings.HUDScale);
        HUD.DrawHiddenSafe(HUDBounds);
        Matrix = matrix;
        GUI.matrix = OldMatrix;
        if (DrawLogo && Event.current.type == EventType.Repaint)
        {
            ContentLoader.cam.Render();
        }
    }
}