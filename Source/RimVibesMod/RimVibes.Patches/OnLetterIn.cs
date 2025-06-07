using HarmonyLib;
using RimVibes.EventHandling;
using RimWorld;
using Verse;

namespace RimVibes.Patches;

[HarmonyPatch(typeof(LetterStack), "ReceiveLetter", typeof(Letter), typeof(string), typeof(int), typeof(bool))]
internal static class OnLetterIn
{
    internal static bool SuppressNext;

    private static void Prefix(Letter let)
    {
        if (SuppressNext)
        {
            SuppressNext = false;
            return;
        }

        var type = EventType.None;
        if (let.def == LetterDefOf.Death)
        {
            type = EventType.Death;
        }
        else if (let.def == LetterDefOf.ThreatBig)
        {
            type = EventType.Big_Threat_Not_Raid;
        }
        else if (let.def == LetterDefOf.ThreatSmall)
        {
            type = EventType.Small_Threat;
        }
        else if (let.def == LetterDefOf.PositiveEvent)
        {
            type = EventType.Positive_Event;
        }
        else if (let.def == LetterDefOf.NeutralEvent)
        {
            type = EventType.Neutral_Event;
        }
        else if (let.def == LetterDefOf.NegativeEvent)
        {
            type = EventType.Negative_Event;
        }
        else if (let.def == DefDatabase<LetterDef>.GetNamedSilentFail("NewQuest"))
        {
            type = EventType.New_Quest;
        }

        EventManager.PostEvent(type);
    }
}