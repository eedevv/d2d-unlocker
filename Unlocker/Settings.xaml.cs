using System;
using System.Windows;
using System.Windows.Controls;

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
        }

        private async void PakBypass_Start(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Outdated");
            //if (MainWindow.CurrentType == "MS") return;

            //await MainWindow.PakBypass.LoadPakBypass();

            //if (MainWindow.CurrentType == "Steam")
            //{
            //    await MainWindow.PakBypass.LoadSSLBypass();
            //}
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

            Classes.Settings.SaveSettings(); // Update to WorkerService to notice changes
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

                Classes.Mods.ModManager.UpdateEngine();
            }
            else if(MainWindow.CurrentType == "EGS")
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

                Classes.Mods.ModManager.UpdateEngine();
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
            OverlayEnabled = !OverlayEnabled; // Change State

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
    }
}
