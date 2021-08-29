using Esatto.Win32.Com;
using Itp.WpfCrossProcess;
using Itp.WpfCrossProcess.IPC;
using System;
using System.Windows;
using System.Windows.Input;

namespace OopWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            LostFocus += this.MainWindow_LostFocus;
            GotFocus += this.MainWindow_GotFocus;
        }

        private void MainWindow_LostFocus(object sender, RoutedEventArgs e)
        {
            this.Title = $"Lost: {e.OriginalSource}";
        }

        private void MainWindow_GotFocus(object sender, RoutedEventArgs e)
        {
            this.Title = e.OriginalSource.ToString();
        }

        private void btTake_Click(object sender, RoutedEventArgs e)
        {
            var child = (IWpfCrossChild)ComInterop.CreateLocalServer("OopWpfServer.WpfAddinHost");
            ccMain.Items.Add(new ImportedVisualHost(child));
        }
    }
}
