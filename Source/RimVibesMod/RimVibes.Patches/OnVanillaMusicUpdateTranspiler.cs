using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace RimVibes.Patches;

[HarmonyPatch(typeof(MusicManagerPlay), "MusicUpdate")]
internal class OnVanillaMusicUpdateTranspiler
{
    private static readonly MethodInfo customMethodInfo = SymbolExtensions.GetMethodInfo(() => OnMusicFadeOut());

    public static event Action<SongDef> UponMusicFadeOut;

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var found = false;
        foreach (var instruction in instructions)
        {
            if (!found && instruction.opcode == OpCodes.Callvirt && instruction.operand.ToString() == "Void Stop()")
            {
                found = true;
                yield return new CodeInstruction(OpCodes.Call, customMethodInfo);
            }

            yield return instruction;
        }

        if (!found)
        {
            Log.Error("Failed to find insertion point in Music Update transpiler. Things may be broken now...");
        }
    }

    private static void OnMusicFadeOut()
    {
        //Log.Message($"Music just faded out: {OnChooseVanillaSong.LastChosenSong?.defName ?? "<null>"}");
        UponMusicFadeOut?.Invoke(OnChooseVanillaSong.LastChosenSong);
    }
}