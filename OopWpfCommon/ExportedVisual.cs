using Itp.WpfCrossProcess.IPC;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace Itp.WpfCrossProcess
{
    // StandardOleMarshalObject keeps us single-threaded on the UI thread
    public class ExportedVisual : StandardOleMarshalObject, IWpfCrossChild
    {
        public readonly HwndSource source;
        public IKeyboardInputSink keyboardInputSink => source;

        public IntPtr Hwnd => source.Handle;

        public ExportedVisual(Visual rootVisual)
        {
            var parameters = new HwndSourceParameters("AddIn")
            {
                ParentWindow = new IntPtr(-3) /* HWND_MESSAGE */,
                WindowStyle = 0x40000000 /* WS_CHILD */,
            };
            source = new HwndSource(parameters)
            {
                RootVisual = rootVisual,
                CompositionTarget = { BackgroundColor = Colors.White },
                SizeToContent = SizeToContent.Manual
            };
        }

        void IWpfCrossChild.Shutdown()
        {
            ShutdownInternal();
            source.Dispose();
        }

        protected virtual void ShutdownInternal()
        {
            // no-op
        }

        bool IWpfCrossChild.HasFocusWithin() => this.keyboardInputSink.HasFocusWithin();

        bool IWpfCrossChild.TabInto(/* FocusNavigationDirection */ int direction, ref bool wrapped)
        {
            var req = new TraversalRequest((FocusNavigationDirection)direction) { Wrapped = wrapped };
            var result = this.keyboardInputSink.TabInto(req);
            wrapped = req.Wrapped;
            return result;
        }

        void IWpfCrossChild.ConnectToHost(IWpfCrossHost host)
        {
            keyboardInputSink.KeyboardInputSite = new SiteProxy(host, keyboardInputSink);
        }

        private class SiteProxy : IKeyboardInputSite
        {
            private IWpfCrossHost host;

            public IKeyboardInputSink Sink { get; private set; }

            public SiteProxy(IWpfCrossHost host, IKeyboardInputSink keyboardInputSink)
            {
                this.host = host;
                this.Sink = keyboardInputSink;
            }

            public bool OnNoMoreTabStops(TraversalRequest request)
            {
                var wrapped = request.Wrapped;
                var result = host?.OnNoMoreTabStops((int)request.FocusNavigationDirection, ref wrapped) ?? false;
                request.Wrapped = wrapped;
                return result;
            }

            public void Unregister()
            {
                this.Sink.KeyboardInputSite = null;
                this.Sink = null;
                this.host = null;
            }
        }
    }
}
