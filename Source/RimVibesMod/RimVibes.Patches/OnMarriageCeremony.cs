using HarmonyLib;
using RimVibes.EventHandling;
using RimWorld;

namespace RimVibes.Patches;

[HarmonyPatch(typeof(VoluntarilyJoinableLordsStarter), "TryStartMarriageCeremony")]
internal static class OnMarriageCeremony
{
    private static void Postfix(bool __result)
    {
        if (__result)
        {
            EventManager.PostEvent(EventType.Marriage_Ceremony);
        }
    }
}