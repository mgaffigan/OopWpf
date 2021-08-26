using OopWpfCommon;
using System.AddIn.Contract;
using System.AddIn.Pipeline;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            ccMain.Items.Add(new CustomAddInHost(WpfOopAddinConstants.ProgID) { Width = 300, Height = 100 });
        }
    }
}
