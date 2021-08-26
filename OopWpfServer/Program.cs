using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OopWpfServer
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var app = new Application();
            app.MainWindow = new MainWindow();
            app.MainWindow.Show();
            app.Run();
        }
    }
}
