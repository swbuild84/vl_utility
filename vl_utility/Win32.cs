using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace vl_utility
{
    public static class Win32
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName); // поиск окна по имени класса и заголовку

        [DllImport("coredll.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);
    }
}
