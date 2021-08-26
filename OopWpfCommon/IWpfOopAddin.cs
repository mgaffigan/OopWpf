using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OopWpfCommon
{
    [ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("6A3968B5-FEBC-417F-8294-32C3D94B184A")]
    public interface IWpfOopAddin
    {
        IntPtr Hwnd { get; }
        void Shutdown();
        bool HasFocusWithin();
        bool TabInto(/* FocusNavigationDirection */ int direction, ref bool wrapped);

        void ConnectToHost(IWpfOopAddinHost host);
    }

    [ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("6A3968B5-FEBC-417F-8294-32C3D94B184B")]
    public interface IWpfOopAddinHost
    {
        bool OnNoMoreTabStops(/* FocusNavigationDirection */ int direction, ref bool wrapped);
    }

    public static class WpfOopAddinConstants
    {
        public const string ProgID = "OopWpfServer.WpfAddinHost";
    }
}
