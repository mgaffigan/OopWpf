using OopWpfCommon;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace OopWpfServer
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId(WpfOopAddinConstants.ProgID)]
    // StandardOleMarshalObject keeps us single-threaded on the UI thread
    public class WpfAddinHost : StandardOleMarshalObject, IWpfOopAddin
    {
        public readonly HwndSource source;
        public IKeyboardInputSink keyboardInputSink => source;

        public IntPtr Hwnd => source.Handle;

        public WpfAddinHost(Visual rootVisual)
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

        void IWpfOopAddin.Shutdown()
        {
            source.Dispose();
        }

        bool IWpfOopAddin.HasFocusWithin() => this.keyboardInputSink.HasFocusWithin();

        bool IWpfOopAddin.TabInto(/* FocusNavigationDirection */ int direction, ref bool wrapped)
        {
            var req = new TraversalRequest((FocusNavigationDirection)direction) { Wrapped = wrapped };
            var result = this.keyboardInputSink.TabInto(req);
            wrapped = req.Wrapped;
            return result;
        }

        void IWpfOopAddin.ConnectToHost(IWpfOopAddinHost host)
        {
            keyboardInputSink.KeyboardInputSite = new SiteProxy(host, keyboardInputSink);
        }

        private class SiteProxy : IKeyboardInputSite
        {
            private IWpfOopAddinHost host;

            public IKeyboardInputSink Sink { get; private set; }

            public SiteProxy(IWpfOopAddinHost host, IKeyboardInputSink keyboardInputSink)
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
