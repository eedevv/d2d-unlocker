using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading;
using System.Windows;

namespace d2d.Classes
{
    internal static class Protection
    {
        private static readonly string[] KnownAnalysisTools = {
            "ProcessHacker", "ProcessExplorer", "Wireshark", "Fiddler",
            "HTTPDebugger", "Charles", "dnSpy", "ILSpy", "IDA",
            "x64dbg", "x32dbg", "OllyDbg", "CheatEngine", "ReClass.NET",
            "HTTPAnalyzer", "BurpSuite", "Proxifier", "SocksCap"
        };

        private static readonly string[] KnownBanProcesses = {
            "EasyAntiCheat", "BEService", "BEservice"
        };

        private static readonly Random _jitter = new();

        internal static void Initialize()
        {
            CleanTraces();
            CheckForAnalysisTools();
        }

        internal static void CleanTraces()
        {
            try
            {
                string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string flagDir = localAppData + "/d2d/Flags";

                if (Directory.Exists(flagDir))
                {
                    foreach (string flag in Directory.GetFiles(flagDir))
                    {
                        if (Path.GetFileName(flag) == "renamed.flag") continue;
                        try { File.Delete(flag); } catch { }
                    }
                }

                string tempPath = Path.GetTempPath();
                foreach (string file in Directory.GetFiles(tempPath, "d2d_*.tmp"))
                {
                    try { File.Delete(file); } catch { }
                }
            }
            catch { }
        }

        internal static void SecureCleanup()
        {
            try
            {
                string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string flagDir = localAppData + "/d2d/Flags";

                if (Directory.Exists(flagDir))
                {
                    foreach (string flag in Directory.GetFiles(flagDir))
                    {
                        if (Path.GetFileName(flag) == "renamed.flag") continue;
                        try
                        {
                            File.Delete(flag);
                        }
                        catch { }
                    }
                }
            }
            catch { }
        }

        internal static void CheckForAnalysisTools()
        {
            try
            {
                foreach (Process process in Process.GetProcesses())
                {
                    string name = process.ProcessName;
                    if (KnownAnalysisTools.Any(tool =>
                        name.IndexOf(tool, StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        MainWindow.ErrorLog.CreateLog($"Heuristic alert: {name}");
                    }
                }
            }
            catch { }
        }

        internal static void RandomDelay()
        {
            int delay = _jitter.Next(50, 250);
            Thread.Sleep(delay);
        }

        internal static int GetJitteredDelay(int baseMs)
        {
            return baseMs + _jitter.Next(-100, 200);
        }

        internal static string ObfuscateString(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            char[] chars = input.ToCharArray();
            for (int i = 1; i < chars.Length; i += 2)
            {
                chars[i] = (char)(chars[i] ^ 0x2A);
            }
            return new string(chars);
        }

        internal static string DeobfuscateString(string input)
        {
            return ObfuscateString(input);
        }

        internal static void SafeKillProcess(string name)
        {
            try
            {
                foreach (Process proc in Process.GetProcessesByName(name))
                {
                    try
                    {
                        if (!proc.HasExited)
                            proc.Kill();
                    }
                    catch { }
                }
            }
            catch { }
        }
    }
}
