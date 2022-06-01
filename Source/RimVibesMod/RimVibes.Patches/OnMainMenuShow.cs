using HarmonyLib;
using Verse;

namespace RimVibes.Patches;

[HarmonyPatch(typeof(UIRoot_Entry), "Init")]
internal class OnMainMenuShow
{
    private static void Prefix()
    {
        RimVibesMod.Instance.OnRimworldShowMainMenu();
    }
}