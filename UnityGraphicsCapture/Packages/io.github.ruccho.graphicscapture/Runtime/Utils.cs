using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Ruccho.GraphicsCapture
{
    public static class Utils
    {
        private delegate bool EnumWindowsDelegate(IntPtr hWnd, IntPtr lparam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumWindows(EnumWindowsDelegate lpEnumFunc, IntPtr lparam);

        delegate bool EnumMonitorsDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

        [DllImport("user32.dll")]
        private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, EnumMonitorsDelegate lpfnEnum,
            IntPtr dwData);

        public static IEnumerable<ICaptureTarget> GetTargets(bool includeMonitor = true,
            bool includeNonCapturableWindows = false)
        {
            IEnumerable<ICaptureTarget> targets = GetTopWindows(includeNonCapturableWindows);
            if (includeMonitor)
                targets = targets.Concat(GetMonitors());

            return targets;
        }

        public static IEnumerable<WindowInfo> GetTopWindows(bool includeNonCapturableWindows = false)
        {
            var windows = new List<WindowInfo>();
            EnumWindows((hwnd, lparam) =>
            {
                var wi = new WindowInfo(hwnd);
                if (includeNonCapturableWindows || wi.IsCapturable())
                    windows.Add(wi);
                return true;
            }, IntPtr.Zero);
            return windows;
        }

        public static IEnumerable<MonitorInfo> GetMonitors()
        {
            var monitors = new List<MonitorInfo>();
            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero,
                (IntPtr monitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr data) =>
                {
                    var mi = new MonitorInfo(monitor);

                    monitors.Add(mi);
                    return true;
                }, IntPtr.Zero);
            return monitors;
        }
    }
}