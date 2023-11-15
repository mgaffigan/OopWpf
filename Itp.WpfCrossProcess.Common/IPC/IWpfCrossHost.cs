using System;
using System.Diagnostics;
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

    // Child app
    public class WpfCrossHostDispatchChildProxy : IWpfCrossHost
    {
        private readonly dynamic Target;

        public WpfCrossHostDispatchChildProxy(object disp)
        {
            this.Target = disp;
        }

        bool IWpfCrossHost.OnNoMoreTabStops(int direction, ref bool wrapped) => Target.OnNoMoreTabStops(direction, ref wrapped);
        void IWpfCrossHost.InvalidateMeasure() => Target.InvalidateMeasure();
        void IWpfCrossHost.OnActivated() => Target.OnActivated();
    }

    // Used in the main app
    // Must be public lest .Net not allow dispatch
    [ComVisible(true), ClassInterface(ClassInterfaceType.AutoDispatch)]
    public class WpfCrossHostDispatchServerProxy : DotnetRuntimeIssue94749.StandardOleMarshalObject
    {
        private readonly IWpfCrossHost Target;

#nullable disable
        public WpfCrossHostDispatchServerProxy()
        {
        }
#nullable restore

        public WpfCrossHostDispatchServerProxy(IWpfCrossHost disp)
        {
            this.Target = disp;
        }

        public bool OnNoMoreTabStops(int direction, ref bool wrapped) => Target.OnNoMoreTabStops(direction, ref wrapped);
        public void InvalidateMeasure() => Target.InvalidateMeasure();
        public void OnActivated() => Target.OnActivated();
    }
}
