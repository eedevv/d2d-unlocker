using System;
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
        internal string EpicUsername = "";

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
            MainWindow.cookie.CookieBox.Text = "";

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

        private void ApplyFOV(object sender, RoutedEventArgs e)
        {
            try
            {
                string configDir = Classes.Utils.GetGameINIDir();
                if (string.IsNullOrEmpty(configDir))
                {
                    MessageBox.Show("Could not determine game config directory.");
                    return;
                }
                string engineIni = System.IO.Path.Combine(configDir, "Engine.ini");
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(engineIni));

                var lines = new System.Collections.Generic.List<string>();
                if (System.IO.File.Exists(engineIni))
                    lines.AddRange(System.IO.File.ReadAllLines(engineIni));

                int killerFov = (int)FOV_KillerSlider.Value;
                int survivorFov = (int)FOV_SurvivorSlider.Value;

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
                    lines.Add($"AspectRatioAxisConstraint=AspectRatio_MaintainYFOV");
                }
                else
                {
                    lines.Insert(sectionIndex + 1, $"AspectRatioAxisConstraint=AspectRatio_MaintainYFOV");
                }

                System.IO.File.WriteAllLines(engineIni, lines);
                MessageBox.Show($"FOV applied! Killer: {killerFov}, Survivor: {survivorFov}");
                MainWindow.ErrorLog.CreateLog($"FOV written to {engineIni}");
            }
            catch (Exception ex)
            {
                MainWindow.ErrorLog.CreateLog($"FOV apply error: {ex.Message}");
                MessageBox.Show($"Failed to save FOV: {ex.Message}");
            }
        }
    }
}
