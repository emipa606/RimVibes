using HarmonyLib;
using RimVibes.EventHandling;
using RimWorld;

namespace RimVibes.Patches;

[HarmonyPatch(typeof(IncidentWorker_RaidEnemy), "TryExecuteWorker")]
internal static class OnRaidStart
{
    private static void Prefix()
    {
        //Log.Message("Enemy raid start.");
        OnLetterIn.SuppressNext = true;
        EventManager.PostEvent(EventType.Raid);
    }
}