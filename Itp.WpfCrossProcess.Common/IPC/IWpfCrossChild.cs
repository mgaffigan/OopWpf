using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Itp.WpfCrossProcess.IPC
{
    [ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("6A3968B5-FEBC-417F-8294-32C3D94B184C")]
    public interface IWpfCrossChild
    {
        IntPtr Hwnd { get; }

        void Shutdown();
        bool HasFocusWithin();
        bool TabInto(/* FocusNavigationDirection */ int direction, ref bool wrapped);

        void ConnectToHost(IWpfCrossHost host);

        IpcSize Measure(IpcSize size);
        void Arrange(IpcSize size);
    }

    [ComVisible(true)]
    [Guid("C4019C20-B877-4490-B126-95FA1B7D1143")]
    public struct IpcSize
    { 
        public readonly double Width, Height;
        public IpcSize(double w, double h)
        {
            this.Width = w;
            this.Height = h;
        }
        public static implicit operator Size(IpcSize s) => new Size(s.Width, s.Height);
        public static implicit operator IpcSize(Size s) => new IpcSize(s.Width, s.Height);
    }
}
