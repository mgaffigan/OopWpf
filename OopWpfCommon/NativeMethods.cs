using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Itp.WpfCrossProcess
{
    internal static class NativeMethods
    {
        public enum GWLParameter
        {
            GWL_EXSTYLE = -20,
            GWL_HINSTANCE = -6,
            GWL_HWNDPARENT = -8,
            GWL_ID = -12,
            GWL_STYLE = -16,
            GWL_USERDATA = -21,
            GWL_WNDPROC = -4
        }

        [DllImport(User32, CharSet = CharSet.Unicode)]
        public static extern int SetWindowLong(IntPtr windowHandle, GWLParameter nIndex, int dwNewLong);

        private const string User32 = "user32.dll";
    }
}
