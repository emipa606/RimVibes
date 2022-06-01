using RimVibes.UI;
using UnityEngine;

namespace RimVibes.Components;

public class MainMenuLogoComponent : MonoBehaviour
{
    public Animator Anim;

    public RectTransform Offset;

    public Vector2 Size = new Vector2(390f, 100f);

    public static MainMenuLogoComponent Instance { get; private set; }

    public Rect ScreenBounds
    {
        get
        {
            var myModSettings = RimVibesMod.Instance?.Settings;
            if (myModSettings == null)
            {
                return default;
            }

            return new Rect(Screen.width - Size.x - myModSettings.MainMenuButtonOffset.x,
                Screen.height - Size.y - myModSettings.MainMenuButtonOffset.y, Size.x, Size.y);
        }
    }

    private void Start()
    {
        Instance = this;
    }

    private void Update()
    {
        var myModSettings = RimVibesMod.Instance?.Settings;
        if (myModSettings == null)
        {
            return;
        }

        Offset.anchoredPosition = new Vector2(0f - myModSettings.MainMenuButtonOffset.x,
            0f - myModSettings.MainMenuButtonOffset.y);
        Vector2 point = Input.mousePosition;
        point.y = Screen.height - point.y;
        Anim.SetBool("MouseOver", ScreenBounds.Contains(point));
        if (RimVibesMod.Instance.Hook.DrawLogo && ScreenBounds.Contains(point) &&
            (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)))
        {
            OnClick();
        }
    }

    private void OnClick()
    {
        MainUI.Open();
    }

    public void ResetAnim()
    {
        Anim.SetTrigger("Reset");
    }
}