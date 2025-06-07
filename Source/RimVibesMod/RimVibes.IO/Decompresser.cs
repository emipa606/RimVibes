using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using ICSharpCode.SharpZipLib.Tar;
using UnityEngine;
using Verse;

namespace RimVibes.IO;

public class Decompresser
{
    public static bool EnsureExtracted(RuntimePlatform platform)
    {
        if (!IsExtracted(platform))
        {
            return Decompress(GetInputFilePath(platform), GetOutputFolderPath(platform));
        }

        if (!HasModUpdated())
        {
            return true;
        }

        if (!IsExtracted(platform))
        {
            return Decompress(GetInputFilePath(platform), GetOutputFolderPath(platform));
        }

        var outputFolderPath = GetOutputFolderPath(platform);
        CacheCreds(outputFolderPath);
        if (!Decompress(GetInputFilePath(platform), GetOutputFolderPath(platform)))
        {
            return false;
        }

        RestoreCreds(outputFolderPath);
        return true;
    }

    private static bool IsExtracted(RuntimePlatform platform)
    {
        var outputFolderPath = GetOutputFolderPath(platform);
        return outputFolderPath != null && Directory.Exists(outputFolderPath);
    }

    private static bool HasModUpdated()
    {
        var path = Path.Combine(RimVibesMod.Instance.Content.RootDir, "LastLaunchedVersion.txt");
        var text = "0.0.0.0";
        if (File.Exists(path))
        {
            text = File.ReadAllText(path).Trim();
        }

        var text2 = Assembly.GetExecutingAssembly().GetName().Version.ToString().Trim();
        if (text2 == text)
        {
            return false;
        }

        Log.Warning(
            $"Mod has updated or been re-installed - current version: {text2}, last loaded version: {text}");
        //Log.Message("Storing new version number.");
        File.WriteAllText(path, text2);
        return true;
    }

    public static string GetOutputFolderPath(RuntimePlatform platform)
    {
        var path = Path.Combine(RimVibesMod.Instance.Content.RootDir, "Executables", "Decompressed");
        return platform switch
        {
            RuntimePlatform.WindowsPlayer => Path.Combine(path, "Windows_x86"),
            RuntimePlatform.OSXPlayer => Path.Combine(path, "OSX"),
            RuntimePlatform.LinuxPlayer => Path.Combine(path, "Linux_x64"),
            _ => null
        };
    }

    private static string GetInputFilePath(RuntimePlatform platform)
    {
        var path = Path.Combine(RimVibesMod.Instance.Content.RootDir, "Executables", "Compressed");
        return platform switch
        {
            RuntimePlatform.WindowsPlayer => Path.Combine(path, "Windows_x86.tar.gz"),
            RuntimePlatform.OSXPlayer => Path.Combine(path, "OSX.tar.gz"),
            RuntimePlatform.LinuxPlayer => Path.Combine(path, "Linux_x64.tar.gz"),
            _ => null
        };
    }

    private static void CacheCreds(string decompressedFolder)
    {
        var text = Path.Combine(Application.temporaryCachePath, "temp_client_creds");
        var text2 = Path.Combine(decompressedFolder, "client_creds");
        if (!File.Exists(text2))
        {
            Log.Warning($"Failed to cache creds, did not find file: {text2}");
            return;
        }

        try
        {
            if (File.Exists(text))
            {
                File.Delete(text);
            }

            File.Copy(text2, text);
        }
        catch (Exception ex)
        {
            Log.Error("Caching creds failed:");
            Log.Error(ex.ToString());
        }
    }

    private static void RestoreCreds(string decompressedFolder)
    {
        var text = Path.Combine(Application.temporaryCachePath, "temp_client_creds");
        var text2 = Path.Combine(decompressedFolder, "client_creds");
        if (!File.Exists(text))
        {
            Log.Warning($"Failed to restore cached creds, did not find file: {text}");
            return;
        }

        try
        {
            if (File.Exists(text2))
            {
                File.Delete(text2);
            }

            File.Copy(text, text2);
        }
        catch (Exception ex)
        {
            Log.Error("Restoring cached creds failed:");
            Log.Error(ex.ToString());
        }
    }

    private static bool Decompress(string input, string output, bool deleteExisting = true)
    {
        if (input == null)
        {
            return false;
        }

        if (output == null)
        {
            return false;
        }

        if (!File.Exists(input))
        {
            return false;
        }

        try
        {
            if (deleteExisting && Directory.Exists(output))
            {
                Directory.Delete(output, true);
            }
        }
        catch
        {
            // ignored
        }

        if (!Directory.Exists(output))
        {
            Directory.CreateDirectory(output);
        }

        //Log.Message($"Decompressing from {input} to {output}");
        using var baseInputStream = File.OpenRead(input);
        using var inputStream = new GZipStream(baseInputStream, CompressionMode.Decompress);
        using var tarArchive = TarArchive.CreateInputTarArchive(inputStream);
        tarArchive.ExtractContents(output);

        return true;
    }
}