using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace Itp.WpfCrossProcess
{
    internal static class NativeMethods
    {
        [DllImport(User32, ExactSpelling = true, CharSet = CharSet.Unicode)]
        internal static extern IntPtr SetParent(IntPtr hWnd, IntPtr hWndParent);

        private const string User32 = "user32.dll";
    }
}
