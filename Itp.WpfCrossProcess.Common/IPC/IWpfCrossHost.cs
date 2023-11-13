using System;
using System.Runtime.InteropServices;

namespace Itp.WpfCrossProcess.IPC
{
    [ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("6A3968B5-FEBC-417F-8294-32C3D94B184D")]
    public interface IWpfCrossHost
    {
        bool OnNoMoreTabStops(/* FocusNavigationDirection */ int direction, ref bool wrapped);

        void InvalidateMeasure();
        void OnActivated();
    }
}
