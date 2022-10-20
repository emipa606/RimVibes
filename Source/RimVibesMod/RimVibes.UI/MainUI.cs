using System.Collections.Generic;
using System.Threading;
using Common;
using RimVibes.Components;
using RimVibes.IO;
using RimVibes.Utils;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimVibes.UI;

public class MainUI : Window
{
    private static MainUI open;

    private MainUI()
    {
        doCloseX = true;
    }

    public static void Open()
    {
        if (open != null)
        {
            if (open.IsOpen)
            {
                Log.Warning("Already open, won't open again.");
                return;
            }

            open = null;
        }

        var window = new MainUI();
        Find.WindowStack.Add(window);
        open = window;
    }

    public override void Close(bool doCloseSound = true)
    {
        MainMenuLogoComponent.Instance.ResetAnim();
        base.Close(doCloseSound);
        open = null;
    }

    public override void DoWindowContents(Rect inRect)
    {
        var logoAndTitle = ContentLoader.LogoAndTitle;
        var width = logoAndTitle.width;
        var x = (inRect.width - width) * 0.5f;
        Widgets.DrawTextureFitted(new Rect(x, inRect.y, width, 75f), logoAndTitle, 1f);
        inRect.y += 65f;
        var x2 = Text.CalcSize("By Epicguru").x;
        Widgets.Label(new Rect((inRect.width - x2) * 0.5f, inRect.y, inRect.width, 30f), "By Epicguru");
        inRect.y += 30f;
        Widgets.DrawLineHorizontal(inRect.x + 20f, inRect.y, inRect.width - 40f);
        inRect.y += 30f;
        if (!Terms.HasUserAgreed)
        {
            DrawNotAccepted(inRect);
            return;
        }

        if (Widgets.ButtonText(new Rect(inRect.x, inRect.height - 34f, 110f, 30f), "RiVi.OpenSettings".Translate()))
        {
            var dialog_ModSettings = new Dialog_ModSettings(RimVibesMod.Instance);
            //var field = dialog_ModSettings.GetType().GetField("selMod", BindingFlags.Instance | BindingFlags.NonPublic);
            //field?.SetValue(dialog_ModSettings, RimVibesMod.Instance);
            Find.WindowStack.Add(dialog_ModSettings);
        }

        var status = RimVibesMod.Instance.Status;
        var label = status.ToReadable();
        var icon = status.GetIcon();
        Widgets.DrawTextureFitted(new Rect(inRect.x, inRect.y, 32f, 32f), icon, 1f);
        Widgets.Label(new Rect(inRect.x + 42f, inRect.y + 6f, inRect.width, 40f), label);
        switch (status)
        {
            case Vibe.ConnectedReady:
            {
                inRect.y += 50f;
                var playbackState = RimVibesMod.Instance.PlaybackState;
                var isPlaying = playbackState.IsPlaying;
                var label2 = "RiVi.NotPlaying".Translate();
                if (isPlaying)
                {
                    label2 = "RiVi.PlayingInfo".Translate(playbackState.Item.IsActual ? "" : "(?) ",
                        playbackState.Item.Name, playbackState.Item.ArtistName);
                }

                Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 34f), label2);
                var text3 = "RiVi.Relaunch".Translate();
                var vector3 = Text.CalcSize(text3);
                vector3.x += 20f;
                if (Widgets.ButtonText(new Rect(inRect.width - vector3.x - 10f, inRect.y, vector3.x, 30f), text3))
                {
                    //Log.Message("Launching external process...");
                    if (!RimVibesMod.Instance.AppManager.TryLaunch(true))
                    {
                        Log.Error(
                            "CRITICAL: Failed to launch process, RimVibes will not work. Process re-launch can be attempted from the menu.");
                    }

                    Thread.Sleep(500);
                }

                inRect.y += 35f;

                if (Widgets.ButtonText(new Rect(inRect.x, inRect.y, 300f, 32f), "RiVi.PlayPlaylist".Translate()))
                {
                    var list = new List<FloatMenuOption>
                    {
                        new FloatMenuOption("Rimworld Inspired by Hackerman", delegate
                            {
                                RimVibesMod.TrySendExecute(delegate(NetData msg)
                                {
                                    msg.Write((byte)14);
                                    msg.Write("spotify:playlist:6kObb7fqYrNthL8c6ZB27K");
                                    msg.Write(0);
                                    msg.Write(0);
                                });
                            },
                            MenuOptionPriority.Default, null, null, 29f),
                        new FloatMenuOption("Music for Space Travelers by BREAKFAST BOI", delegate
                            {
                                RimVibesMod.TrySendExecute(delegate(NetData msg)
                                {
                                    msg.Write((byte)14);
                                    msg.Write("spotify:playlist:46qRepmbmV0hgesx7jXfjg");
                                    msg.Write(0);
                                    msg.Write(0);
                                });
                            },
                            MenuOptionPriority.Default, null, null, 29f),
                        new FloatMenuOption("RimWorld Music to Jam to by HeftyYeti8764", delegate
                            {
                                RimVibesMod.TrySendExecute(delegate(NetData msg)
                                {
                                    msg.Write((byte)14);
                                    msg.Write("spotify:playlist:1WRl2hh2YaSxTYuFwd8g3n");
                                    msg.Write(0);
                                    msg.Write(0);
                                });
                            },
                            MenuOptionPriority.Default, null, null, 29f)
                    };

                    Find.WindowStack.Add(new FloatMenu(list));
                }

                inRect.y += 36f;
                if (Widgets.ButtonText(new Rect(inRect.x, inRect.y, 300f, 32f), "RiVi.ViewPlaylist".Translate()))
                {
                    var list = new List<FloatMenuOption>
                    {
                        new FloatMenuOption("Rimworld Inspired by Hackerman",
                            delegate
                            {
                                Application.OpenURL(
                                    "https://open.spotify.com/playlist/6kObb7fqYrNthL8c6ZB27K?si=lX8HZ__xT-SHqbxtumXuLg");
                            },
                            MenuOptionPriority.Default, null, null, 29f),
                        new FloatMenuOption("Music for Space Travelers by BREAKFAST BOI",
                            delegate
                            {
                                Application.OpenURL(
                                    "https://open.spotify.com/playlist/46qRepmbmV0hgesx7jXfjg?si=6328b7112689418d");
                            },
                            MenuOptionPriority.Default, null, null, 29f),
                        new FloatMenuOption("RimWorld Music to Jam to by HeftyYeti8764",
                            delegate
                            {
                                Application.OpenURL(
                                    "https://open.spotify.com/playlist/1WRl2hh2YaSxTYuFwd8g3n?si=55446a5556614254");
                            },
                            MenuOptionPriority.Default, null, null, 29f)
                    };

                    Find.WindowStack.Add(new FloatMenu(list));
                }

                break;
            }
            case Vibe.ConnectedNoAuth:
            {
                var text = "RiVi.Help".Translate();
                var vector = Text.CalcSize(text);
                vector.x += 20f;
                if (Widgets.ButtonText(new Rect(inRect.width - vector.x, inRect.y, vector.x, 30f), text))
                {
                    Application.OpenURL("https://github.com/Epicguru/RimVibesMod/tree/master#how-to-fix");
                }

                var text4 = "RiVi.Authorize".Translate();
                var vector4 = Text.CalcSize(text4);
                vector4.x += 20f;
                if (Widgets.ButtonText(new Rect(inRect.width - vector.x - vector4.x - 10f, inRect.y, vector4.x, 30f),
                        text4))
                {
                    RimVibesMod.TrySendExecute(delegate(NetData msg)
                    {
                        msg.Write((byte)5);
                        msg.Write(true);
                    });
                }

                break;
            }
            case Vibe.Disconnected:
            {
                var text = "RiVi.Help".Translate();
                var vector = Text.CalcSize(text);
                vector.x += 20f;
                if (Widgets.ButtonText(new Rect(inRect.width - vector.x, inRect.y, vector.x, 30f), text))
                {
                    Application.OpenURL("https://github.com/Epicguru/RimVibesMod/blob/master/README.md");
                }

                var text3 = "RiVi.Relaunch".Translate();
                var vector3 = Text.CalcSize(text3);
                vector3.x += 20f;
                if (Widgets.ButtonText(new Rect(inRect.width - vector.x - vector3.x - 10f, inRect.y, vector3.x, 30f),
                        text3))
                {
                    //Log.Message("Launching external process...");
                    if (!RimVibesMod.Instance.AppManager.TryLaunch(true))
                    {
                        Log.Error(
                            "CRITICAL: Failed to launch process, RimVibes will not work. Process re-launch can be attempted from the menu.");
                    }

                    Thread.Sleep(500);
                }

                break;
            }
            case Vibe.NotResponding:
            {
                var text = "RiVi.Help".Translate();
                var vector = Text.CalcSize(text);
                vector.x += 20f;
                if (Widgets.ButtonText(new Rect(inRect.width - vector.x, inRect.y, vector.x, 40f), text))
                {
                    Application.OpenURL("https://github.com/Epicguru/RimVibesMod/blob/master/README.md");
                }

                var text2 = "RiVi.Kill".Translate();
                var vector2 = Text.CalcSize(text2);
                vector2.x += 20f;
                if (Widgets.ButtonText(new Rect(inRect.width - vector.x - vector2.x - 10f, inRect.y, vector2.x, 40f),
                        text2))
                {
                    RimVibesMod.Instance.AppManager.KillProcess();
                }

                break;
            }
        }
    }

    private void DrawNotAccepted(Rect inRect)
    {
        Text.Font = GameFont.Medium;
        Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 30f), "RiVi.TermNotAccepted".Translate());
        inRect.y += 40f;
        Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 30f), "RiVi.AcceptTerms".Translate());
        inRect.y += 40f;
        var text = "RiVi.ReviewTerms".Translate();
        var vector = Text.CalcSize(text);
        vector.x += 20f;
        vector.y += 20f;
        if (Widgets.ButtonText(new Rect(inRect.x, inRect.y, vector.x, vector.y), text))
        {
            Close();
            TermsUI.Open();
        }

        Text.Font = GameFont.Small;
    }
}