using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Reflection;
using System.Diagnostics;

namespace SpeckleAutoCAD.UI
{
    public class SpeckleAutoCADAppWindowHost : HwndHost
    {
        public SpeckleAutoCADAppWindowHost(IntPtr hwnd)
        {
            hostedWindow = hwnd;
        }

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            var href = new HandleRef(this, hostedWindow);
            SetWindowLongPtr64(href, GWL_STYLE, (IntPtr)WS_CHILD);
            SetParent(hostedWindow, hwndParent.Handle);
            return href;

        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            
        }

        public string GetAssemblyDirectory()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return System.IO.Path.GetDirectoryName(path);
        }

        private IntPtr hostedWindow;

        public const int GWLP_HWNDPARENT = (-8);
        public const int GWL_STYLE = (-16);
        public const int WS_CHILD = 0x40000000;

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(HandleRef hWnd, int nIndex, IntPtr dwNewLong);
    }
}
