using Itp.WpfCrossProcess.IPC;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using static Itp.WpfCrossProcess.NativeMethods;

namespace Itp.WpfCrossProcess
{
    public class ImportedVisualHost : HwndHost, IKeyboardInputSink
    {
        private readonly IntPtr childHwnd;
        private readonly IWpfCrossChild RemoteInstance;

        static ImportedVisualHost()
        {
            Control.IsTabStopProperty.OverrideMetadata(typeof(ImportedVisualHost), new FrameworkPropertyMetadata(true));
            FocusableProperty.OverrideMetadata(typeof(ImportedVisualHost), new FrameworkPropertyMetadata(true));
        }

        public ImportedVisualHost(IWpfCrossChild inst)
        {
            if (inst == null)
            {
                throw new ArgumentNullException(nameof(inst));
            }

            this.childHwnd = inst.Hwnd;
            this.RemoteInstance = inst;
            inst.ConnectToHost(new WpfOopAddinHostThunk(this));
        }

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            SetWindowLong(childHwnd, GWLParameter.GWL_HWNDPARENT, hwndParent.Handle.ToInt32());
            return new HandleRef(this, childHwnd);
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            RemoteInstance.Shutdown();
        }

        bool IKeyboardInputSink.TabInto(TraversalRequest request)
        {
            var wrapped = request.Wrapped;
            var result = RemoteInstance.TabInto((int)request.FocusNavigationDirection, ref wrapped);
            request.Wrapped = wrapped;
            return result;
        }

        // Need to inherit from StandardOleMarshalObject for STA thread / can't do this on CustomAddInHost
        private class WpfOopAddinHostThunk : StandardOleMarshalObject, IWpfCrossHost
        {
            private readonly ImportedVisualHost Parent;

            public WpfOopAddinHostThunk(ImportedVisualHost parent)
            {
                this.Parent = parent;
            }

            public bool OnNoMoreTabStops(int direction, ref bool wrapped)
            {
                var req = new TraversalRequest((FocusNavigationDirection)direction) { Wrapped = wrapped };
                var result = Parent.MoveFocus(req);
                wrapped = req.Wrapped;
                return result;
            }
        }
    }
}
