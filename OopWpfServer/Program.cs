using Esatto.Win32.Com;
using System;
using System.Windows;

namespace Itp.WpfCrossProcess.DemoServer;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // Register the `Remote` control as for the session
        using var res = new ClassObjectRegistration(Guid.Parse("A87689E8-F7C6-34E0-938F-BF17FE842740"),
            ComInterop.CreateClassFactoryFor(() => new ExportedVisual(new Remote())),
            CLSCTX.LOCAL_SERVER, REGCLS.MULTIPLEUSE | REGCLS.SUSPENDED);
        ComInterop.CoResumeClassObjects();

        // Run the WPF Dispatcher loop
        new Application().Run();
    }
}
