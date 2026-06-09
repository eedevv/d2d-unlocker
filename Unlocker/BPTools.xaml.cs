using System.Windows;
using System.Windows.Controls;

namespace d2d
{
    public partial class BPTools : Page
    {
        public BPTools()
        {
            InitializeComponent();
        }

        private void BP_Launch(object sender, RoutedEventArgs e)
        {
            MainWindow.main.MainFrame.Content = MainWindow.bp;
        }

        private void Tome_Launch(object sender, RoutedEventArgs e)
        {
            MainWindow.main.MainFrame.Content = MainWindow.tome;
        }
    }
}
