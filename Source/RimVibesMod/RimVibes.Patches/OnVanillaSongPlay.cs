using System;
using Common;
using HarmonyLib;
using RimWorld;
using Verse;

namespace RimVibes.Patches;

[HarmonyPatch(typeof(MusicManagerPlay), "StartNewSong")]
internal class OnVanillaSongPlay
{
    private static bool hasPaused;

    static OnVanillaSongPlay()
    {
        OnVanillaMusicUpdateTranspiler.UponMusicFadeOut += OnVanillaSongFadeOut;
    }

    private static void Postfix()
    {
        var lastChosenSong = OnChooseVanillaSong.LastChosenSong;
        //Log.Message($"New vanilla song started: {lastChosenSong?.defName ?? "<null>"}");
        if (lastChosenSong != null)
        {
            //Log.Message(
            //lastChosenSong.tense
            //    ? "It is a tense song... Ooh the suspense!"
            //    : "It is not a tense song, just chill.");
        }

        OverrideMusicVolumePatch.Enabled = false;
        var num = Math.Abs(Find.MusicManagerPlay.CurSanitizedVolume);
        OverrideMusicVolumePatch.Enabled = true;
        var anyVolume = num > 0.005f;
        if (anyVolume)
        {
            switch (RimVibesMod.Instance.Settings.SongPauseMode)
            {
                case SongPauseMode.Never_Auto_Pause:
                    anyVolume = false;
                    break;
                case SongPauseMode.Pause_For_Tense_Song:
                    if (lastChosenSong == null || !lastChosenSong.tense)
                    {
                        anyVolume = false;
                    }

                    break;
            }
        }

        if (!anyVolume)
        {
            return;
        }

        //Log.Message("Pausing spotify for vanilla music.");
        RimVibesMod.TrySendExecute(delegate(NetData msg)
        {
            msg.Write((byte)15);
            msg.Write(2f);
        });
        hasPaused = true;
    }

    private static void OnVanillaSongFadeOut(SongDef def)
    {
        if (!hasPaused)
        {
            return;
        }

        hasPaused = false;
        RimVibesMod.TrySendExecute(delegate(NetData msg)
        {
            msg.Write((byte)16);
            msg.Write(2f);
        });
    }
}