using System;
using System.Diagnostics;
using System.IO;
using RimVibes.IO;
using UnityEngine;
using Verse;

namespace RimVibes.RemoteApp;

public class AppManager
{
    public bool IsRemoteProcessRunning
    {
        get
        {
            if (RemoteProcess != null)
            {
                return !RemoteProcess.HasExited;
            }

            return false;
        }
    }

    public Process RemoteProcess { get; private set; }

    public void KillProcess()
    {
        RemoteProcess?.Kill();
        RemoteProcess = null;
    }

    public bool TryLaunch(bool forceNew)
    {
        if (RemoteProcess is { HasExited: false })
        {
            if (!forceNew)
            {
                return true;
            }

            Log.Warning("Trying to launch new process while old one has not finished running. Killing old one...");
            RemoteProcess.Kill();
            RemoteProcess = null;
        }

        var outputFolderPath = Decompresser.GetOutputFolderPath(Application.platform);
        var relativeRunnableName = GetRelativeRunnableName(Application.platform);
        var text = Path.Combine(outputFolderPath, relativeRunnableName);
        if (!File.Exists(text))
        {
            Log.Error($"Failed to find {relativeRunnableName}");
            return false;
        }

        try
        {
            RunPlatformPreprocessing(Application.platform, text);
            RemoteProcess = RunProcess(Application.platform, text, !RimVibesMod.Instance.Settings.LaunchDebugWindow);
            if (RemoteProcess == null)
            {
                throw new Exception(
                    $"Failed to run '{text}' on platform {Application.platform}, hidden: {!RimVibesMod.Instance.Settings.LaunchDebugWindow}");
            }
        }
        catch (Exception ex)
        {
            Log.Error("Failed launching executable:");
            Log.Error($"[{ex.GetType().Name}] {ex.Message}");
            Log.Error(ex.ToString());
            return false;
        }

        return true;
    }

    private void RunPlatformPreprocessing(RuntimePlatform platform, string executablePath)
    {
        if (platform != RuntimePlatform.LinuxPlayer)
        {
            return;
        }

        try
        {
            var cmd = $"chmod +x {executablePath}";
            _ = cmd.Bash(true);
            //Log.Message($"Run permission output: {text}");
        }
        catch (Exception ex)
        {
            Log.Error("Failed to give run permissions for Linux: ");
            Log.Error(ex.ToString());
        }
    }

    private string GetRelativeRunnableName(RuntimePlatform platform)
    {
        return platform switch
        {
            RuntimePlatform.WindowsPlayer => "Core.exe",
            RuntimePlatform.LinuxPlayer => "Core",
            _ => "Platform_Not_Supported"
        };
    }

    private Process RunProcess(RuntimePlatform platform, string executablePath, bool hidden)
    {
        switch (platform)
        {
            case RuntimePlatform.WindowsPlayer:
            {
                var processStartInfo = new ProcessStartInfo(executablePath);
                if (!hidden)
                {
                    return Process.Start(processStartInfo);
                }

                processStartInfo.RedirectStandardOutput = true;
                processStartInfo.RedirectStandardError = true;
                processStartInfo.UseShellExecute = false;
                processStartInfo.CreateNoWindow = true;

                return Process.Start(processStartInfo);
            }
            case RuntimePlatform.LinuxPlayer:
                return executablePath.BashProcess(hidden);
            default:
                return null;
        }
    }
}