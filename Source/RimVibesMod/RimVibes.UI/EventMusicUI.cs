using System;
using RimVibes.EventHandling;
using UnityEngine;
using Verse;
using EventType = RimVibes.EventHandling.EventType;

namespace RimVibes.UI;

public class EventMusicUI : Window
{
    private float height;
    private Vector2 scroll;

    private EventMusicUI()
    {
        doCloseButton = true;
        closeOnClickedOutside = true;
    }

    public override Vector2 InitialSize => new Vector2(750f, 600f);

    public static void Open()
    {
        Find.WindowStack.Add(new EventMusicUI());
    }

    public override void DoWindowContents(Rect inRect)
    {
        Text.Font = GameFont.Medium;
        Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 32f), "RiVi.EventBased".Translate());
        Text.Font = GameFont.Small;
        var all = RimVibesMod.Instance.Settings.Responses.All;
        inRect.y += 36f;
        if (Widgets.ButtonText(new Rect(inRect.x, inRect.y, 90f, 25f), "RiVi.AddNew".Translate()))
        {
            all.Add(new EventResponse
            {
                SpotifyID = "RiVi.SpotifyId".Translate()
            });
        }

        if (Widgets.ButtonText(new Rect(inRect.x + 100f, inRect.y, 50f, 25f), "RiVi.Help".Translate()))
        {
            Application.OpenURL("https://github.com/Epicguru/RimVibesMod/blob/master/MusicEvents.md");
        }

        if (all.Count != 0 && RimVibesMod.Instance.Settings.SongPauseMode != 0)
        {
            GUI.color = Color.yellow;
            var label = "RiVi.PauseWarning".Translate();
            Widgets.Label(new Rect(inRect.x + 155f, inRect.y, inRect.width - (inRect.x + 155f) - 50f, 42f), label);
            GUI.color = Color.white;
            if (Widgets.ButtonText(new Rect(inRect.xMax - 50f, inRect.y + 4f, 45f, 26f), "Fix"))
            {
                RimVibesMod.Instance.Settings.SongPauseMode = SongPauseMode.Never_Auto_Pause;
            }
        }

        inRect.y += 65f;
        inRect.height -= 101f;
        Widgets.BeginScrollView(new Rect(inRect.x, inRect.y, inRect.width, inRect.height - 50f), ref scroll,
            new Rect(inRect.x, inRect.y, inRect.width, height));
        height = 0f;
        for (var i = 0; i < all.Count; i++)
        {
            var eventResponse = all[i];
            var color = i % 2 == 0 ? Color.white : Color.grey;
            color.a = 0.4f;
            Widgets.DrawBoxSolid(new Rect(inRect.x, inRect.y, inRect.width - 10f, eventResponse.DrawHeight), color);
            var widgetRow = new WidgetRow(inRect.x + 4f, inRect.y + 7f, UIDirection.RightThenDown);
            if (widgetRow.ButtonIcon(ContentLoader.BinIcon, "RiVi.Delete".Translate(), Color.red,
                    doMouseoverSound: true))
            {
                all.RemoveAt(i);
                i--;
                continue;
            }

            widgetRow.Label("RiVi.Event".Translate());
            if (widgetRow.ButtonText(eventResponse.ActivatedUpon.ToString().Replace('_', ' ')))
            {
                var num = Enum.GetNames(typeof(EventType)).Length;
                var num2 = (int)(eventResponse.ActivatedUpon + 1);
                num2 = (int)(eventResponse.ActivatedUpon = (EventType)(num2 % num));
            }

            widgetRow.Label("RiVi.Action".Translate());
            if (widgetRow.ButtonText(eventResponse.ResponseType.ToString().Replace('_', ' ')))
            {
                var num3 = Enum.GetNames(typeof(ResponseType)).Length;
                var num4 = (int)(eventResponse.ResponseType + 1);
                num4 = (int)(eventResponse.ResponseType = (ResponseType)(num4 % num3));
            }

            if (eventResponse.ResponseType != 0)
            {
                Widgets.Label(new Rect(inRect.x + 5f, inRect.y + 42f, 100f, 32f), "RiVi.Enabled".Translate());
                Widgets.Checkbox(new Vector2(inRect.x + 105f, inRect.y + 42f), ref eventResponse.IsEnabled, 32f);
            }

            if (eventResponse.ResponseType == ResponseType.Play_Music)
            {
                widgetRow.Label("RiVi.Playlist".Translate());
                Widgets.Checkbox(new Vector2(widgetRow.FinalX, widgetRow.FinalY), ref eventResponse.IsPlaylist, 32f);
                widgetRow.Gap(32f);
                eventResponse.SpotifyID = Widgets.TextEntryLabeled(
                    new Rect(inRect.x, inRect.y + 42f, inRect.width - 20f, 28f), "RiVi.SpotifyIdIn".Translate(),
                    eventResponse.SpotifyID);
                if (eventResponse.IsPlaylist)
                {
                    widgetRow.Label("RiVi.Random".Translate());
                    Widgets.Checkbox(new Vector2(widgetRow.FinalX, widgetRow.FinalY),
                        ref eventResponse.RandomFromPlaylist, 32f);
                    widgetRow.Gap(32f);
                }

                if (Widgets.ButtonText(new Rect(inRect.x + 105f + 32f + 5f, inRect.y + 42f, 100f, 32f),
                        "RiVi.Test".Translate()))
                {
                    eventResponse.Run();
                }
            }

            var num5 = eventResponse.DrawHeight = eventResponse.ResponseType != 0 ? 76 : 42;
            MoveDown(num5 + 10f);
            height += num5 + 10f;
        }

        Widgets.EndScrollView();
        return;

        void MoveDown(float amount)
        {
            inRect.y += amount;
            inRect.height -= amount;
        }
    }
}