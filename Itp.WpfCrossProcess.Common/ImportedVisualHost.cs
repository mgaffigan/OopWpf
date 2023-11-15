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
            SetParent(childHwnd, hwndParent.Handle);
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

        protected override Size MeasureOverride(Size constraint)
        {
            double w = constraint.Width, h = constraint.Height;
            RemoteInstance.Measure(ref w, ref h);
            return new Size(w, h);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            RemoteInstance.Arrange(finalSize.Width, finalSize.Height);
            return base.ArrangeOverride(finalSize);
        }

        // Need to inherit from StandardOleMarshalObject for STA thread / can't do this on CustomAddInHost
        private class WpfOopAddinHostThunk : StandardOleMarshalObject, IWpfCrossHost
        {
            private readonly ImportedVisualHost Parent;

            public WpfOopAddinHostThunk(ImportedVisualHost parent)
            {
                this.Parent = parent;
            }

            public void InvalidateMeasure()
            {
                Parent.InvalidateMeasure();
            }

            public void OnActivated()
            {
                // Shouldn't set keyboard focus, since keyboard focus is outside of our scope.
                var kf = Keyboard.FocusedElement;
                var focusManager = FocusManager.GetFocusScope(Parent);
                var prev = FocusManager.GetFocusedElement(focusManager);
                FocusManager.SetFocusedElement(focusManager, null);
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
