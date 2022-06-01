using System;
using System.IO;
using Verse;

namespace RimVibes.IO;

public static class Terms
{
    private const string FILE_NAME = "Agreed_To_Terms.txt";

    private static bool hasAgreedLastKnown;

    private static bool hasCheckedEver;

    public static bool HasUserAgreed
    {
        get
        {
            if (!hasCheckedEver)
            {
                CheckFileForAgreement();
            }

            return hasAgreedLastKnown;
        }
    }

    public static bool CheckFileForAgreement()
    {
        hasAgreedLastKnown = File.Exists(Path.Combine(RimVibesMod.Instance.Content.RootDir, "Agreed_To_Terms.txt"));
        return hasAgreedLastKnown;
    }

    public static string LoadTerms()
    {
        try
        {
            return File.ReadAllText(Path.Combine(RimVibesMod.Instance.Content.RootDir, "Resources", "TermsOfUse.txt"));
        }
        catch (Exception ex)
        {
            Log.Error(
                "Failed to find Terms of use file! Please verify that the mod is installed correctly and isn't missing any files.");
            Log.Error(ex.Message);
            return "Terms of use file not found - please re-install the mod.";
        }
    }

    internal static void SaveAgreement()
    {
        var path = Path.Combine(RimVibesMod.Instance.Content.RootDir, "Agreed_To_Terms.txt");
        File.WriteAllText(path, "You have agreed to the terms of this mod, enjoy!");
        hasCheckedEver = true;
        hasAgreedLastKnown = true;
    }
}