using Itp.WpfCrossProcess.IPC;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace Itp.WpfCrossProcess
{
    // StandardOleMarshalObject keeps us single-threaded on the UI thread
    public class ExportedVisual : StandardOleMarshalObject, IWpfCrossChild
    {
        public readonly HwndSource source;
        private readonly UIElement RootVisual;
        private readonly SizeInsulator Insulator;
        private IWpfCrossHost? Host;

        public IKeyboardInputSink keyboardInputSink => source;

        public IntPtr Hwnd => source.Handle;

        public ExportedVisual(UIElement rootVisual)
        {
            this.RootVisual = rootVisual ?? throw new ArgumentNullException(nameof(rootVisual));
            rootVisual.Dispatcher.VerifyAccess();
            this.Insulator = new SizeInsulator(rootVisual, this);
            var parameters = new HwndSourceParameters("AddIn")
            {
                ParentWindow = new IntPtr(-3) /* HWND_MESSAGE */,
                WindowStyle = 0x40000000 /* WS_CHILD */,
                HwndSourceHook = new HwndSourceHook(sourceMessageFilter),
            };
            source = new HwndSource(parameters)
            {
                RootVisual = Insulator,
                CompositionTarget = { BackgroundColor = Colors.White },
                SizeToContent = SizeToContent.WidthAndHeight
            };
        }

        private IntPtr sourceMessageFilter(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 7 /* WM_SETFOCUS */:
                    OnActivated();
                    handled = false;
                    return IntPtr.Zero;
                case 8 /* WM_KILLFOCUS */:
                    OnInactivated();
                    handled = false;
                    return IntPtr.Zero;
                default:
                    handled = false;
                    return IntPtr.Zero;
            }
        }

        private void OnActivated()
        {
            RootVisual.Dispatcher.BeginInvoke(new Action(() =>
            {
                Host?.OnActivated();
            }));
        }

        private void OnInactivated()
        {
            FocusManager.SetFocusedElement(Insulator, null);
        }

        void IWpfCrossChild.Shutdown()
        {
            RootVisual.Dispatcher.VerifyAccess();
            ShutdownInternal();
            source.Dispose();
        }

        protected virtual void ShutdownInternal()
        {
            // no-op - implementation in descendants
        }

        bool IWpfCrossChild.HasFocusWithin => this.keyboardInputSink.HasFocusWithin();

        bool IWpfCrossChild.TabInto(/* FocusNavigationDirection */ int direction, ref bool wrapped)
        {
            RootVisual.Dispatcher.VerifyAccess();

            var req = new TraversalRequest((FocusNavigationDirection)direction) { Wrapped = wrapped };
            var result = this.keyboardInputSink.TabInto(req);
            wrapped = req.Wrapped;
            return result;
        }

        void IWpfCrossChild.ConnectToHost(IWpfCrossHost host)
        {
            RootVisual.Dispatcher.VerifyAccess();

            this.Host = host;
            keyboardInputSink.KeyboardInputSite = new SiteProxy(host, keyboardInputSink);
        }

        void IWpfCrossChild.Measure(ref double w, ref double h)
        {
            RootVisual.Dispatcher.VerifyAccess();

            this.Insulator.MeasureFromHost(ref w, ref h);
        }

        void IWpfCrossChild.Arrange(double w, double h)
        {
            RootVisual.Dispatcher.VerifyAccess();

            this.Insulator.TakeSize(new Size(w, h));
        }

        private class SizeInsulator : Decorator
        {
            private readonly new ExportedVisual Parent;
            private Size? LastChildSize;
            private Size HostConstraint;
            private Size NegotiatedSize;

            static SizeInsulator()
            {
                FocusManager.IsFocusScopeProperty.OverrideMetadata(typeof(SizeInsulator), new FrameworkPropertyMetadata(true));
            }

            public SizeInsulator(UIElement child, ExportedVisual parent)
            {
                this.Parent = parent;
                this.Child = child;
            }

            internal void MeasureFromHost(ref double w, ref double h)
            {
                var constraint = new Size(w, h);
                this.HostConstraint = constraint;

                Child.Measure(constraint);

                var result = Child.DesiredSize;
                LastChildSize = result;
                w = result.Width;
                h = result.Height;
            }

            protected override Size MeasureOverride(Size _unusedConstraint)
            {
                // on layout changed, we will recalculate the child size - need to notify parent
                var previous = LastChildSize;
                Child.Measure(HostConstraint);
                var newSize = Child.DesiredSize;
                LastChildSize = newSize;

                if (previous != null
                    && (!DoubleUtil.AreClose(previous.Value.Width, newSize.Width)
                        || !DoubleUtil.AreClose(previous.Value.Height, newSize.Height)))
                {
                    Parent.Host?.InvalidateMeasure();
                }

                // We defer actually changing until Host calls back with TakeSize
                return NegotiatedSize;
            }

            internal void TakeSize(Size size)
            {
                this.NegotiatedSize = size;
                InvalidateMeasure();
            }
        }

        private class SiteProxy : IKeyboardInputSite
        {
            private IWpfCrossHost? host;

            public IKeyboardInputSink? Sink { get; private set; }

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
                if (Sink is null) return;
                this.Sink.KeyboardInputSite = null;
                this.Sink = null;
                this.host = null;
            }
        }
    }
}
