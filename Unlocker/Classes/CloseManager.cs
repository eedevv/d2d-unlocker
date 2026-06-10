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
            Settings.SaveFOVSettings();

            if (Overlay.timer != null)
            {
                Overlay.StopTimer();
            }
        }
    }
}
