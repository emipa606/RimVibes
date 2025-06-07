using HarmonyLib;
using RimWorld;

namespace RimVibes.Patches;

[HarmonyPatch(typeof(MusicManagerPlay), "get_CurSanitizedVolume")]
internal static class OverrideMusicVolumePatch
{
    public static float VolumeScale { get; set; } = 1f;


    public static bool Enabled { get; set; } = true;


    private static void Postfix(ref float __result)
    {
        if (Enabled)
        {
            __result *= VolumeScale;
        }
    }
}