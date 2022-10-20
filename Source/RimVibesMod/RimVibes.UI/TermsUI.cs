using RimVibes.Components;
using RimVibes.IO;
using UnityEngine;
using Verse;

namespace RimVibes.UI;

public class TermsUI : Window
{
    private bool hasRejected;
    private Vector2 scroll;

    private string text;

    private TermsUI()
    {
        drawShadow = true;
        draggable = false;
        doCloseX = false;
        doCloseButton = false;
    }

    public static bool HasJustAccepted { get; internal set; }

    public static void Open()
    {
        Find.WindowStack.Add(new TermsUI());
    }

    public override void DoWindowContents(Rect inRect)
    {
        if (!hasRejected)
        {
            DrawNormal(inRect);
        }
        else
        {
            DrawRejected(inRect);
        }
    }

    public override void Close(bool doCloseSound = true)
    {
        MainMenuLogoComponent.Instance.ResetAnim();
        base.Close(doCloseSound);
    }

    private void DrawNormal(Rect inRect)
    {
        if (text == null)
        {
            text = Terms.LoadTerms();
        }

        var logoAndTitle = ContentLoader.LogoAndTitle;
        var width = logoAndTitle.width;
        var x = (inRect.width - width) * 0.5f;
        Widgets.DrawTextureFitted(new Rect(x, inRect.y, width, 75f), logoAndTitle, 1f);
        var rect = new Rect(inRect.x, inRect.y + 80f, inRect.width, inRect.height - 50f - 80f);
        Widgets.TextAreaScrollable(rect, text, ref scroll, true);
        var acceptTerms = "RiVi.AcceptTerms".Translate();
        var vector = Text.CalcSize(acceptTerms);
        vector.x += 20f;
        var buttonRect = new Rect(inRect.xMax - vector.x - 10f, inRect.yMax - 40f, vector.x, 30f);
        TooltipHandler.TipRegion(buttonRect, "RiVi.AcceptTermsHaveTo".Translate());
        if (Widgets.ButtonText(buttonRect, "RiVi.AcceptTerms".Translate()))
        {
            //Log.Message("User has accepted terms, saving...");
            Terms.SaveAgreement();
            HasJustAccepted = true;
            //Log.Message("Launching external process...");
            if (!RimVibesMod.Instance.AppManager.TryLaunch(false))
            {
                Log.Error(
                    "CRITICAL: Failed to launch process, RimVibes will not work. Process re-launch can be attempted from the menu.");
            }

            Close();
        }

        if (!Widgets.ButtonText(new Rect(inRect.x, inRect.yMax - 40f, 110f, 30f), "RiVi.RejectTerms".Translate()))
        {
            return;
        }

        //Log.Message("User has rejected terms");
        hasRejected = true;
        scroll = Vector2.zero;
    }

    private void DrawRejected(Rect inRect)
    {
        Text.Font = GameFont.Medium;
        Widgets.TextAreaScrollable(new Rect(inRect.x, inRect.y, inRect.width, inRect.height - 50f),
            "RiVi.RejectMessage".Translate(),
            ref scroll, true);
        Text.Font = GameFont.Small;
        if (Widgets.ButtonText(new Rect(inRect.xMax - 120f, inRect.yMax - 40f, 110f, 30f), "RiVi.Close".Translate()))
        {
            base.Close();
        }
    }
}