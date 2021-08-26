using Esatto.Win32.Com;
using Esatto.Win32.Windows;
using OopWpfCommon;
using System;
using System.AddIn.Contract;
using System.AddIn.Pipeline;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OopWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btTake_Click(object sender, RoutedEventArgs e)
        {
            ccMain.Content = new CustomAddInHost(WpfOopAddinConstants.ProgID);
        }
    }

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

        protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == 0x3d /* WM_GETOBJECT */)
            {
                handled = false;
                return IntPtr.Zero;
            }
            return base.WndProc(hwnd, msg, wParam, lParam, ref handled);
        }

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
    /*
    internal class ComHwndContract : INativeHandleContract
    {
        private IntPtr hwnd;

        public ComHwndContract(string progid)
        {
            var inst = (IWpfOopAddin)ComInterop.CreateLocalServer(progid);
            this.hwnd = inst.Hwnd;
            this.Instance = inst;
        }

        public IntPtr GetHandle() => this.hwnd;

        int refcount;
        private IWpfOopAddin Instance;

        public int AcquireLifetimeToken() => refcount++;

        public int GetRemoteHashCode() => GetHashCode();

        public IContract QueryContract(string contractIdentifier) => null;

        public bool RemoteEquals(IContract contract) => false;

        public string RemoteToString() => this.ToString();

        public void RevokeLifetimeToken(int token)
        {
            refcount--;
            if (refcount == 0)
            {

                var instance = Instance;
                Instance = null;
                hwnd = (IntPtr)(-1);
                instance.Shutdown();
            }
            // no-op
        }
    }*/
}
