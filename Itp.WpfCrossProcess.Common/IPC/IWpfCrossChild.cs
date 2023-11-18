using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;

namespace Itp.WpfCrossProcess.IPC
{
    [ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("6A3968B5-FEBC-417F-8294-32C3D94B184C")]
    public interface IWpfCrossChild
    {
        IntPtr Hwnd { get; }

        void Shutdown();
        bool HasFocusWithin { get; }
        bool TabInto(/* FocusNavigationDirection */ int direction, ref bool wrapped);

        void ConnectToHost(IWpfCrossHost host);

        void Measure(ref double w, ref double h);
        void Arrange(double w, double h);
    }

    // Used in the main app
    public class WpfCrossChildDispatchChildProxy : IWpfCrossChild
    {
        private readonly dynamic Target;

        public WpfCrossChildDispatchChildProxy(object disp)
        {
            this.Target = disp;
        }

        IntPtr IWpfCrossChild.Hwnd => (IntPtr)(int)Target.Hwnd;
        void IWpfCrossChild.Arrange(double w, double h) => Target.Arrange(w, h);
        void IWpfCrossChild.ConnectToHost(IWpfCrossHost host) => Target.ConnectToHost(new WpfCrossHostDispatchServerProxy(host));
        bool IWpfCrossChild.HasFocusWithin => Target.HasFocusWithin();
        void IWpfCrossChild.Measure(ref double w, ref double h) => Target.Measure(ref w, ref h);
        void IWpfCrossChild.Shutdown() => Target.Shutdown();
        bool IWpfCrossChild.TabInto(int direction, ref bool wrapped) => Target.TabInto(direction, ref wrapped);
    }

    // Child app
    // must be public lest .Net not allow Dispatch
    [ComVisible(true)]
    public class WpfCrossChildDispatchServerProxy : DotnetRuntimeIssue94749.StandardOleMarshalObject
    {
        private readonly IWpfCrossChild Target;

#nullable disable
        public WpfCrossChildDispatchServerProxy()
        {
        }
#nullable restore

        public WpfCrossChildDispatchServerProxy(IWpfCrossChild disp)
        {
            this.Target = disp;
        }

        // IntPtr is not supported in IDispatch, but HWND's may not exceed 32 bit
        // https://learn.microsoft.com/en-us/windows/win32/winprog64/interprocess-communication?redirectedfrom=MSDN
        public int Hwnd => checked((int)Target.Hwnd);
        public void Arrange(double w, double h) => Target.Arrange(w, h);
        public void ConnectToHost(object host) => Target.ConnectToHost(new WpfCrossHostDispatchChildProxy(host));
        public bool HasFocusWithin => Target.HasFocusWithin;
        public void Measure(ref double w, ref double h) => Target.Measure(ref w, ref h);
        public void Shutdown() => Target.Shutdown();
        public bool TabInto(int direction, ref bool wrapped) => Target.TabInto(direction, ref wrapped);
    }
}
