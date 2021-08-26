using Esatto.Win32.Com;
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
            var res = new ClassObjectRegistration(typeof(WpfAddinHost).GUID,
                ComInterop.CreateClassFactoryFor(() => new WpfAddinHost(new Remote())),
                CLSCTX.LOCAL_SERVER, REGCLS.MULTIPLEUSE | REGCLS.SUSPENDED);
            ComInterop.CoResumeClassObjects();
            app.Run();
            GC.KeepAlive(res);
        }
    }
}
