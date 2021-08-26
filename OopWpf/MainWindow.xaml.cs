using Esatto.Win32.Com;
using Itp.WpfCrossProcess;
using Itp.WpfCrossProcess.IPC;
using System.Windows;

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
        }

        private void btTake_Click(object sender, RoutedEventArgs e)
        {
            var child = (IWpfCrossChild)ComInterop.CreateLocalServer("OopWpfServer.WpfAddinHost");
            ccMain.Items.Add(new ImportedVisualHost(child) { Width = 300, Height = 100 });
        }
    }
}
