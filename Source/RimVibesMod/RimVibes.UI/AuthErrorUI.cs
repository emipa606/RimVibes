using Common;
using RimVibes.Components;
using UnityEngine;
using Verse;

namespace RimVibes.UI;

public class AuthErrorUI : Window
{
    private readonly bool fromLogin;
    private readonly string reason;

    private bool hasRejected;

    private AuthErrorUI(bool fromLogin, string reason)
    {
        drawShadow = true;
        this.fromLogin = fromLogin;
        this.reason = reason;
    }

    public static void Open(bool fromLogin, string reason)
    {
        Find.WindowStack.Add(new AuthErrorUI(fromLogin, reason));
    }

    public override void DoWindowContents(Rect inRect)
    {
        DrawNormal(inRect);
    }

    public override void Close(bool doCloseSound = true)
    {
        MainMenuLogoComponent.Instance.ResetAnim();
        base.Close(doCloseSound);
    }

    private void DrawNormal(Rect inRect)
    {
        var logoAndTitle = ContentLoader.LogoAndTitle;
        var width = logoAndTitle.width;
        var x = (inRect.width - width) * 0.5f;
        Widgets.DrawTextureFitted(new Rect(x, inRect.y, width, 75f), logoAndTitle, 1f);
        inRect.y += 75f;
        if (hasRejected)
        {
            doCloseButton = true;
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 60f),
                "RiVi.Login".Translate());
            Text.Font = GameFont.Small;
        }
        else if (TermsUI.HasJustAccepted)
        {
            doCloseButton = false;
            Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 40f),
                "RiVi.Welcome".Translate());
            inRect.y += 40f;
            var text = "RiVi.Login".Translate();
            var num = Text.CalcSize(text).x + 20f;
            if (Widgets.ButtonText(new Rect(inRect.xMax - num - 10f, inRect.y, num, 30f), text))
            {
                RimVibesMod.TrySendExecute(delegate(NetData msg)
                {
                    msg.Write((byte)5);
                    msg.Write(true);
                });
                TermsUI.HasJustAccepted = false;
            }

            var text2 = "RiVi.Authorize".Translate();
            var width2 = Text.CalcSize(text2).x + 20f;
            if (!Widgets.ButtonText(new Rect(inRect.x, inRect.y, width2, 30f), text2))
            {
                return;
            }

            hasRejected = true;
            TermsUI.HasJustAccepted = false;
        }
        else
        {
            doCloseButton = true;
            Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 30f), "RiVi.AuthFailed".Translate());
            inRect.y += 30f;
            Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 40f),
                fromLogin ? "RiVi.NoPerm".Translate() : "RiVi.NoRefresh".Translate());
            inRect.y += 40f;
            Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 40f),
                "RiVi.AuthHow".Translate());
            inRect.y += 40f;
            Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 100f), reason);
        }
    }
}