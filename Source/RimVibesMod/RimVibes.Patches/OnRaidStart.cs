using HarmonyLib;
using RimVibes.EventHandling;
using RimWorld;

namespace RimVibes.Patches;

[HarmonyPatch(typeof(IncidentWorker_RaidEnemy), "TryExecuteWorker")]
internal static class OnRaidStart
{
    private static void Prefix(IncidentWorker_Raid __instance)
    {
        //Log.Message("Enemy raid start.");
        OnLetterIn.SuppressNext = true;
        EventManager.PostEvent(EventType.Raid);
    }
}