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
                EpicUsernameGrid.Visibility = Visibility.Visible;
            }
            else if (MainWindow.CurrentType == "EGS")
            {
                EpicUsername = EpicUsernameBox.Text;
                MainWindow.CurrentType = "MS";
                TypeBox.Text = "MS";
                EpicUsernameGrid.Visibility = Visibility.Collapsed;
            }
            else if (MainWindow.CurrentType == "MS")
            {
                MainWindow.CurrentType = "Steam";
                TypeBox.Text = "Steam";
                EpicUsernameGrid.Visibility = Visibility.Collapsed;
            }
        }

        internal void UpdateTypeBox(string type)
        {
            switch (type)
            {
                case "Steam":
                    TypeBox.Text = "Steam";
                    EpicUsernameGrid.Visibility = Visibility.Collapsed;
                    break;
                case "EGS":
                    TypeBox.Text = "Epic Games";
                    EpicUsernameGrid.Visibility = Visibility.Visible;
                    break;
                case "MS":
                    TypeBox.Text = "MS";
                    EpicUsernameGrid.Visibility = Visibility.Collapsed;
                    break;
            }
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
                var mods = new System.Collections.Generic.Dictionary<string, string>
                {
                    ["KillerFOV"] = ((int)FOV_KillerSlider.Value).ToString(),
                    ["SurvivorFOV"] = ((int)FOV_SurvivorSlider.Value).ToString()
                };
                string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string engineIni = System.IO.Path.Combine(localAppData, "d2d", "Mods", "FOV", "Engine.ini");
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(engineIni));
                using (var writer = new System.IO.StreamWriter(engineIni))
                {
                    writer.WriteLine("[SystemSettings]");
                    writer.WriteLine($"KillerFOV={mods["KillerFOV"]}");
                    writer.WriteLine($"SurvivorFOV={mods["SurvivorFOV"]}");
                }
                MessageBox.Show("FOV settings saved! They will apply on next game launch.");
            }
            catch (Exception ex)
            {
                MainWindow.ErrorLog.CreateLog($"FOV apply error: {ex.Message}");
                MessageBox.Show($"Failed to save FOV: {ex.Message}");
            }
        }
    }
}
