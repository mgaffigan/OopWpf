using Esatto.Win32.Com;
using Itp.WpfCrossProcess.IPC;
using System;
using System.Windows;

namespace Itp.WpfCrossProcess.DemoServer
{
    public partial class App : Application
    {
        private readonly ClassObjectRegistration res;

        public App()
        {
            var ExampleControlClsid = Guid.Parse("A87689E8-F7C6-34E0-938F-BF17FE842740");
            res = new ClassObjectRegistration(ExampleControlClsid,
                ComInterop.CreateStaClassFactoryFor(() => new WpfCrossChildDispatchServerProxy(new ExportedVisual(new ExampleControl()))),
                CLSCTX.LOCAL_SERVER, REGCLS.MULTIPLEUSE | REGCLS.SUSPENDED);
            ComInterop.CoResumeClassObjects();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            res.Dispose();
            base.OnExit(e);
        }
    }
}
