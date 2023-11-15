using Esatto.Win32.Com;
using Itp.WpfCrossProcess;
using Itp.WpfCrossProcess.IPC;
using System;
using System.Windows;
using System.Windows.Input;

namespace Itp.WpfCrossProcess.DemoApp
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
            var ExampleControlClsid = Guid.Parse("A87689E8-F7C6-34E0-938F-BF17FE842740");
            var child = ComInterop.CreateLocalServerWithStart(ExampleControlClsid, "Itp.WpfCrossProcess.DemoServer.exe");
            ccMain.Items.Add(new ImportedVisualHost(new WpfCrossChildDispatchChildProxy(child)));
        }
    }
}
