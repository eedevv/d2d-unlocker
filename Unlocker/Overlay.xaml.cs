using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;
using System.Timers;
using System.Threading.Tasks;

namespace d2d
{
    public partial class Overlay : Window
    {
        public static string matchId = "";
        public static string matchRegion = "";
        public static string accountUsername = "";

        private IntPtr OverlayWindowHandle = default;
        private IntPtr Window = default;
        private Classes.User32.Rect rect = default;
        internal static Timer timer;

        public Overlay()
        {
            InitializeComponent();
            this.IsHitTestVisible = false;

            timer = new Timer(100);
            timer.AutoReset = true;
            timer.Elapsed += new ElapsedEventHandler(UpdateWindow);
            timer.Enabled = true;
            OverlayWindowHandle = new WindowInteropHelper(this).Handle;
            LoadOverlay();
        }

        private async void LoadOverlay()
        {
            //Classes.User32.SetWindowLong(OverlayWindowHandle, -20, Classes.User32.GetWindowLong(OverlayWindowHandle, -20) | 32768 | 32);
            Classes.User32.MakeTransParent(OverlayWindowHandle);

            string ProcessName = Classes.Utils.ProccessNames[MainWindow.CurrentType];

            Process[] processesByName = Process.GetProcessesByName(ProcessName);
            if (processesByName.Length != 0)
            {
                Window = await GetWindowHandle(processesByName[0]);
                Classes.User32.GetWindowSize(Window, ref rect);
                this.Width = (rect.Right - rect.Left);
                this.Height = (rect.Bottom - rect.Top);
                this.Left = rect.Left;
                this.Top = rect.Top;
                this.VER_Text.Text = "v" + MainWindow.CurrVersion;
                this.Visibility = Visibility.Visible;
                this.Show();
                timer.Start();
            }
            else
            {
                this.Visibility = Visibility.Hidden;
            }
        }

        private async Task<IntPtr> GetWindowHandle(Process DBDProcess)
        {
            while (DBDProcess.MainWindowHandle == IntPtr.Zero)
            {
                await Task.Delay(1000);
            }

            return DBDProcess.MainWindowHandle;
        }

        internal void UpdateWindow(object sender, ElapsedEventArgs e)
        {
            IntPtr ForeGroundWindow = Classes.User32.GetForegroundWindow();
            if (Window == IntPtr.Zero || ForeGroundWindow != Window)
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    this.Visibility = Visibility.Hidden;
                }));
            }
            else
            {
                Classes.User32.GetWindowSize(Window, ref rect);

                this.Dispatcher.Invoke((Action)(() =>
                {
                    if (MainWindow.main.InQueue)
                    {
                        this.Standard.Visibility = Visibility.Hidden;
                        this.ETA_Text.Text = new DateTime(MainWindow.main.ETA).ToString("mm:ss");
                        this.POS_Text.Text = MainWindow.main.Pos;
                        this.InQ.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        this.InQ.Visibility = Visibility.Hidden;
                        this.Standard.Visibility = Visibility.Visible;
                    }


                    this.Time_Text.Text = DateTime.Now.ToString("HH:mm:ss");
                    this.Width = (rect.Right - rect.Left);
                    this.Height = (rect.Bottom - rect.Top);
                    this.Left = rect.Left;
                    this.Top = rect.Top;
                    this.Visibility = Visibility.Visible;
                }));
            }
        }

        internal static void StopTimer()
        {
            timer.Stop();
            timer.Dispose();
        }

        public static void UpdateMatchInfo(string newMatchId, string newRegion, string newUsername)
        {
            matchId = newMatchId;
            matchRegion = newRegion;
            accountUsername = newUsername;

            if (MainWindow.currentOverlay != null)
            {
                MainWindow.currentOverlay.Dispatcher.Invoke((Action)(() =>
                {
                    MainWindow.currentOverlay.MatchId_Text.Text = "Match: " + matchId;
                    MainWindow.currentOverlay.Region_Text.Text = "Region: " + newRegion;
                    MainWindow.currentOverlay.Username_Text.Text = "User: " + newUsername;
                    MainWindow.currentOverlay.MatchInfo.Visibility = Visibility.Visible;
                }));
            }
        }

    }
}
