using Esatto.Win32.Com;
using Itp.WpfCrossProcess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
            var res = new ClassObjectRegistration(typeof(Example1ExportedVisual).GUID,
                ComInterop.CreateClassFactoryFor(() => new Example1ExportedVisual()),
                CLSCTX.LOCAL_SERVER, REGCLS.MULTIPLEUSE | REGCLS.SUSPENDED);
            ComInterop.CoResumeClassObjects();
            app.Run();
            GC.KeepAlive(res);
        }
    }

    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("OopWpfServer.WpfAddinHost"), Guid("A87689E8-F7C6-34E0-938F-BF17FE842740")]
    public class Example1ExportedVisual : ExportedVisual
    {
        public Example1ExportedVisual()
            : base(new Remote())
        {
        }
    }
}
