using System.Diagnostics;

namespace RimVibes.RemoteApp;

public static class BashHelper
{
    public static string Bash(this string cmd, bool wait)
    {
        var text = cmd.Replace("\"", "\\\"");
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{text}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        process.Start();
        if (!wait)
        {
            return null;
        }

        var result = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        return result;
    }

    public static Process BashProcess(this string cmd, bool hidden)
    {
        var text = cmd.Replace("\"", "\\\"");
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{text}\"",
                RedirectStandardOutput = hidden,
                UseShellExecute = !hidden,
                CreateNoWindow = hidden
            }
        };
        process.Start();
        return process;
    }
}