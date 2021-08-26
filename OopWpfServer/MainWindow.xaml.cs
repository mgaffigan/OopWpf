using Esatto.Win32.Com;
using OopWpfCommon;
using System;
using System.AddIn.Contract;
using System.AddIn.Pipeline;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

namespace OopWpfServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ClassObjectRegistration reg;

        public MainWindow()
        {
            InitializeComponent();

            this.reg = new ClassObjectRegistration(typeof(WpfAddinHost).GUID,
                ComInterop.CreateClassFactoryFor(() => new WpfAddinHost()),
                CLSCTX.LOCAL_SERVER, REGCLS.MULTIPLEUSE | REGCLS.SUSPENDED);
            ComInterop.CoResumeClassObjects();
        }
    }

    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId(WpfOopAddinConstants.ProgID)]
    // StandardOleMarshalObject keeps us single-threaded on the UI thread
    public class WpfAddinHost : StandardOleMarshalObject, IWpfOopAddin
    {
        private readonly Remote hostedRemote;
        // Actually System.AddIn.Pipeline.AddInHwndSourceWrapper 
        // field: _hwndSource
        private readonly INativeHandleContract reference;
        private readonly HwndSource hwndSource;
        private readonly IKeyboardInputSink keyboardInputSink;
        private readonly int token;

        public IntPtr Hwnd => reference.GetHandle();
        
        public WpfAddinHost()
        {
            this.hostedRemote = new Remote();
            this.reference = FrameworkElementAdapters.ViewToContractAdapter(hostedRemote);
            // Dirty hack, but I don't want to re-implement .
            this.hwndSource = (HwndSource)reference.GetType().GetField("_hwndSource", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(reference);
            this.keyboardInputSink = hwndSource;
            this.token = reference.AcquireLifetimeToken();
        }

        public void Shutdown()
        {
            this.reference.RevokeLifetimeToken(token);
        }

        public bool HasFocusWithin() => this.keyboardInputSink.HasFocusWithin();

        public bool TabInto(/* FocusNavigationDirection */ int direction, ref bool wrapped)
        {
            var req = new TraversalRequest((FocusNavigationDirection)direction) { Wrapped = wrapped };
            var result = this.keyboardInputSink.TabInto(req);
            wrapped = req.Wrapped;
            return result;
        }

        public void ConnectToHost(IWpfOopAddinHost host)
        {
            keyboardInputSink.KeyboardInputSite = new SiteProxy(host, keyboardInputSink);
        }

        private class SiteProxy : IKeyboardInputSite
        {
            private IWpfOopAddinHost host;

            public SiteProxy(IWpfOopAddinHost host, IKeyboardInputSink keyboardInputSink)
            {
                this.host = host;
                this.Sink = keyboardInputSink;
            }

            public IKeyboardInputSink Sink { get; private set; }

            public bool OnNoMoreTabStops(TraversalRequest request)
            {
                var wrapped = request.Wrapped;
                var result = host?.OnNoMoreTabStops((int)request.FocusNavigationDirection, ref wrapped) ?? false;
                request.Wrapped = wrapped;
                return result;
            }

            public void Unregister()
            {
                Sink.KeyboardInputSite = null;
                Sink = null;
                host = null;
            }
        }
    }
}
