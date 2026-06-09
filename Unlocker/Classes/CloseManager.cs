using System;
using System.IO;

namespace d2d.Classes
{
    internal class CloseManager
    {
        internal static void Close(bool error = false, string errormsg = "")
        {
            if (error)
            {
                MainWindow.ErrorLog.CreateLog(errormsg);
            }

            Protection.SecureCleanup();
            FiddlerCore.StopFiddlerCore();
            Settings.SaveConfig();
            Settings.SaveSettings();
            Settings.SaveMods();

            if (Overlay.timer != null)
            {
                Overlay.StopTimer();
            }

            string flagDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/d2d/Flags";
            string renameFlag = Path.Combine(flagDir, "renamed.flag");

            if (File.Exists(renameFlag))
                File.Delete(renameFlag);
        }
    }
}
