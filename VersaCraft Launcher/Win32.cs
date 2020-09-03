using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VersaCraft_Launcher
{
    class Win32
    {
        public static readonly int GWL_STYLE = -16;

        public static readonly int WS_BORDER = 0x00800000;
        public static readonly int WS_DLGFRAME = 0x00400000;
        public static readonly int WS_SYSMENU = 0x00080000;
        public static readonly int WS_MINIMIZEBOX = 0x00020000;
        public static readonly int WS_MAXIMIZEBOX = 0x00010000;
        public static readonly int WS_THICKFRAME = 0x00040000;

        public static readonly int SW_MAXIMIZE = 3;
        public static readonly int SW_MINIMIZE = 6;

        public static readonly uint SWP_SHOWWINDOW = 0x0040;
        public static readonly uint SWP_FRAMECHANGED = 0x0020;


        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);


        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern IntPtr SetWindowLongPtr32(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, long dwNewLong);

        public static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, long dwNewLong)
        {
            if (IntPtr.Size == 8)
                return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
            else
                return SetWindowLongPtr32(hWnd, nIndex, (int)dwNewLong);
        }


        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        private static extern IntPtr GetWindowLongPtr32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        public static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size == 8)
                return GetWindowLongPtr64(hWnd, nIndex);
            else
                return GetWindowLongPtr32(hWnd, nIndex);
        }
    }
}