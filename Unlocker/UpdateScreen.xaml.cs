using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
namespace d2d
{
    public partial class UpdateScreen : Page
    {
        public UpdateScreen()
        {
            InitializeComponent();
        }

        internal async void CheckForUpdate()
        {
            UpdateText.Text = "Checking for updates...";
            Spinner.Visibility = Visibility.Visible;
            await Task.Delay(1000);
            MainWindow.AutoUpdater.CheckForUpdates();
        }

        internal async void NoUpdate()
        {
            UpdateText.Text = "No updates found";
            Spinner.Visibility = Visibility.Hidden;
            Check.Visibility = Visibility.Visible;
            await Task.Delay(1000);
            MainWindow.main.UpdateCheckDone();
        }

        internal async void FailUpdate()
        {
            UpdateText.Text = "Could not check for updates";
            Spinner.Visibility = Visibility.Hidden;
            Error.Visibility = Visibility.Visible;
            await Task.Delay(1000);
            MainWindow.main.UpdateCheckDone();
        }

        internal void FoundUpdate(string Version)
        {
            UpdateText.Text = "Found Update: v" + Version;
        }
    }
}
