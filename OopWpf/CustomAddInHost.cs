using Esatto.Win32.Com;
using Esatto.Win32.Windows;
using OopWpfCommon;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

namespace OopWpf
{
    internal class CustomAddInHost : HwndHost, IKeyboardInputSink
    {
        private readonly IntPtr childHwnd;
        private readonly IWpfOopAddin RemoteInstance;

        static CustomAddInHost()
        {
            Control.IsTabStopProperty.OverrideMetadata(typeof(CustomAddInHost), new FrameworkPropertyMetadata(true));
            FocusableProperty.OverrideMetadata(typeof(CustomAddInHost), new FrameworkPropertyMetadata(true));
        }

        public CustomAddInHost(string progId)
        {
            var inst = (IWpfOopAddin)ComInterop.CreateLocalServer(progId);
            this.childHwnd = inst.Hwnd;
            this.RemoteInstance = inst;

            inst.ConnectToHost(new WpfOopAddinHostThunk(this));
        }

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            new Win32Window(childHwnd).SetParent(new Win32Window(hwndParent.Handle));
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
        private class WpfOopAddinHostThunk : StandardOleMarshalObject, IWpfOopAddinHost
        {
            private readonly CustomAddInHost Parent;

            public WpfOopAddinHostThunk(CustomAddInHost parent)
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
