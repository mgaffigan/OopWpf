using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Itp.WpfCrossProcess.IPC
{
    [ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("6A3968B5-FEBC-417F-8294-32C3D94B184A")]
    public interface IWpfCrossChild
    {
        IntPtr Hwnd { get; }

        void Shutdown();
        bool HasFocusWithin();
        bool TabInto(/* FocusNavigationDirection */ int direction, ref bool wrapped);

        void ConnectToHost(IWpfCrossHost host);
    }
}
