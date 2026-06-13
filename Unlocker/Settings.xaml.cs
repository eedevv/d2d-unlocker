using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace d2d
{
    public partial class Settings : Page
    {
        internal bool OverlayEnabled = true;
        internal bool HideToTray = true;
        internal bool RPC = true;
        internal bool FOV_AutoApply = true;
        internal string EpicUsername = "";
        private List<FileSystemWatcher> fovWatchers = new List<FileSystemWatcher>();

        public Settings()
        {
            InitializeComponent();
            VersionInfo.Text = $"d2d v{MainWindow.CurrVersion} | DBD v{MainWindow.DBDVersion}";
        }

        private void Sys_Clicked(object sender, RoutedEventArgs e)
        {
            HideToTray = !HideToTray;
            Sys_Check.IsChecked = HideToTray;
        }

        private void RPC_Clicked(object sender, RoutedEventArgs e)
        {
            RPC = !RPC;
            RPC_Check.IsChecked = RPC;
            Classes.Settings.SaveSettings();
        }

        private void Switch_Platform(object sender, RoutedEventArgs e)
        {
            Classes.CookieHandler.ResetCookie();

            if (MainWindow.CurrentType == "Steam")
            {
                MainWindow.CurrentType = "EGS";
                TypeBox.Text = "Epic Games";
            }
            else if (MainWindow.CurrentType == "EGS")
            {
                EpicUsername = EpicUsernameBox.Text;
                MainWindow.CurrentType = "MS";
                TypeBox.Text = "MS";
            }
            else if (MainWindow.CurrentType == "MS")
            {
                MainWindow.CurrentType = "Steam";
                TypeBox.Text = "Steam";
            }
            EpicUsernameGrid.Visibility = Visibility.Visible;
        }

        internal void UpdateTypeBox(string type)
        {
            switch (type)
            {
                case "Steam":
                    TypeBox.Text = "Steam";
                    break;
                case "EGS":
                    TypeBox.Text = "Epic Games";
                    break;
                case "MS":
                    TypeBox.Text = "MS";
                    break;
            }
            EpicUsernameGrid.Visibility = Visibility.Visible;
        }

        private void Overlay_Clicked(object sender, RoutedEventArgs e)
        {
            OverlayEnabled = !OverlayEnabled;
            Overlay_Check.IsChecked = OverlayEnabled;
            if (!OverlayEnabled)
            {
                if (MainWindow.currentOverlay != null)
                {
                    Overlay.StopTimer();
                    MainWindow.currentOverlay.Close();
                }
            }
            else
            {
                if (Classes.Utils.IsGameCurrentlyRunning(MainWindow.CurrentType))
                {
                    MainWindow.currentOverlay = new Overlay();
                }
            }
        }

        internal void FOV_Slider_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = sender as Slider;
            if (slider == null) return;
            if (slider.Name == "FOV_KillerSlider" && FOV_KillerValue != null)
                FOV_KillerValue.Content = ((int)slider.Value).ToString();
            else if (slider.Name == "FOV_SurvivorSlider" && FOV_SurvivorValue != null)
                FOV_SurvivorValue.Content = ((int)slider.Value).ToString();
        }

        private void FOV_AutoApply_Clicked(object sender, RoutedEventArgs e)
        {
            FOV_AutoApply = !FOV_AutoApply;
            FOV_AutoApplyCheck.IsChecked = FOV_AutoApply;
            Classes.Settings.SaveSettings();
        }

        internal void ApplyFOVNow()
        {
            ApplyFOV(null, null);
        }

        private void ApplyFOV(object sender, RoutedEventArgs e)
        {
            try
            {
                int killerFov = (int)FOV_KillerSlider.Value;
                int survivorFov = (int)FOV_SurvivorSlider.Value;

                string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string configBase = Path.Combine(localAppData, "DeadByDaylight", "Saved", "Config");

                string[] dirs = {
                    Path.Combine(configBase, "WindowsNoEditor"),
                    Path.Combine(configBase, "WindowsClient"),
                    Path.Combine(configBase, "WinGDK")
                };

                int written = 0;
                foreach (string dir in dirs)
                {
                    if (Directory.Exists(dir))
                    {
                        WriteFOVToEngineIni(dir, killerFov, survivorFov);
                        WriteFOVToGameUserSettings(dir, killerFov, survivorFov);
                        written++;
                    }
                }

                if (written == 0)
                {
                    string defaultDir = dirs[0];
                    WriteFOVToEngineIni(defaultDir, killerFov, survivorFov);
                    WriteFOVToGameUserSettings(defaultDir, killerFov, survivorFov);
                }

                if (sender != null)
                    MessageBox.Show($"FOV applied! Killer: {killerFov}, Survivor: {survivorFov}");

                MainWindow.ErrorLog.CreateLog($"FOV written to {written} config directories");
            }
            catch (Exception ex)
            {
                MainWindow.ErrorLog.CreateLog($"FOV apply error: {ex.Message}");
                if (sender != null)
                    MessageBox.Show($"Failed to save FOV: {ex.Message}");
            }
        }

        internal static void WriteFOVToEngineIni(string configDir, int killerFov, int survivorFov)
        {
            Directory.CreateDirectory(configDir);
            string engineIni = Path.Combine(configDir, "Engine.ini");
            var lines = new System.Collections.Generic.List<string>();
            if (File.Exists(engineIni))
                lines.AddRange(File.ReadAllLines(engineIni));

            lines.RemoveAll(l => l.Trim().StartsWith("AspectRatioAxisConstraint="));

            bool hasSection = false;
            int sectionIndex = -1;
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Trim().Equals("[/script/engine.localplayer]", StringComparison.OrdinalIgnoreCase))
                {
                    hasSection = true;
                    sectionIndex = i;
                    break;
                }
            }

            if (!hasSection)
            {
                lines.Add("[/script/engine.localplayer]");
                lines.Add("AspectRatioAxisConstraint=AspectRatio_MaintainYFOV");
            }
            else
            {
                lines.Insert(sectionIndex + 1, "AspectRatioAxisConstraint=AspectRatio_MaintainYFOV");
            }

            File.WriteAllLines(engineIni, lines);
        }

        internal static void WriteFOVToGameUserSettings(string configDir, int killerFov, int survivorFov)
        {
            Directory.CreateDirectory(configDir);
            string gameUserSettings = Path.Combine(configDir, "GameUserSettings.ini");
            var lines = new System.Collections.Generic.List<string>();
            if (File.Exists(gameUserSettings))
                lines.AddRange(File.ReadAllLines(gameUserSettings));

            lines.RemoveAll(l => l.Trim().StartsWith("FOV="));

            bool hasSection = false;
            int sectionIndex = -1;
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Trim().Equals("[/script/deadbydaylight.dbdgameuserettings]", StringComparison.OrdinalIgnoreCase))
                {
                    hasSection = true;
                    sectionIndex = i;
                    break;
                }
            }

            if (!hasSection)
            {
                lines.Add("[/script/deadbydaylight.dbdgameuserettings]");
                lines.Add($"FOV={killerFov}");
            }
            else
            {
                lines.Insert(sectionIndex + 1, $"FOV={killerFov}");
            }

            File.WriteAllLines(gameUserSettings, lines);
        }

        internal void StartFOVWatcher()
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string configBase = Path.Combine(localAppData, "DeadByDaylight", "Saved", "Config");

            foreach (string dir in new[] { "WindowsNoEditor", "WindowsClient", "WinGDK" })
            {
                string fullPath = Path.Combine(configBase, dir);
                if (!Directory.Exists(fullPath)) continue;
                try
                {
                    var watcher = new FileSystemWatcher(fullPath, "*.ini");
                    watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
                    string capturedDir = dir;
                    watcher.Changed += (s, e) =>
                    {
                        System.Threading.Thread.Sleep(1000);
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            int killerFov = (int)MainWindow.settingspage.FOV_KillerSlider.Value;
                            int survivorFov = (int)MainWindow.settingspage.FOV_SurvivorSlider.Value;
                            WriteFOVToEngineIni(fullPath, killerFov, survivorFov);
                            WriteFOVToGameUserSettings(fullPath, killerFov, survivorFov);
                            MainWindow.ErrorLog.CreateLog($"FOV re-applied after config change in {capturedDir}");
                        });
                    };
                    watcher.EnableRaisingEvents = true;
                    fovWatchers.Add(watcher);
                }
                catch { }
            }
        }

        internal void StopFOVWatcher()
        {
            foreach (var w in fovWatchers)
            {
                w.EnableRaisingEvents = false;
                w.Dispose();
            }
            fovWatchers.Clear();
        }
    }
}
