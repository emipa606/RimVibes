using HarmonyLib;
using RimWorld;
using Verse;

namespace RimVibes.Patches;

[HarmonyPatch(typeof(MusicManagerPlay), "ChooseNextSong")]
internal class OnChooseVanillaSong
{
    public static SongDef LastChosenSong;

    private static void Postfix(SongDef __result)
    {
        LastChosenSong = __result;
    }
}