using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace SpeckleAutoCAD.Helpers.WinAPI
{
    internal static class WinApi
    {
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);
    }
}
