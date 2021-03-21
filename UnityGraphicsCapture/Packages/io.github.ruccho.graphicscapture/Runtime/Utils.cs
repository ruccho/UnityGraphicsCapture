using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using AOT;
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
            //var windows = new List<WindowInfo>();
            var allWindows = EnumWindows().Select(hWnd => new WindowInfo(hWnd));

            if (includeNonCapturableWindows) return allWindows;
            return allWindows.Where(w => w.IsCapturable());
        }
        
        private static bool isEnumeratingWindows = false;
        private static readonly List<IntPtr> enumeratedWindows = new List<IntPtr>();

        private static IEnumerable<IntPtr> EnumWindows()
        {
            if(isEnumeratingWindows) throw new InvalidOperationException("Only one EnumWindows() can be called at the same time.");
            isEnumeratingWindows = true;
            
            enumeratedWindows.Clear();
            
            try
            {
                EnumWindows(EnumWindowsCallback, IntPtr.Zero);
            }
            finally
            {
                isEnumeratingWindows = false;
            }

            return enumeratedWindows.ToArray();
        }
        
        [MonoPInvokeCallback(typeof(EnumWindowsDelegate))]
        private static bool EnumWindowsCallback(IntPtr hwnd, IntPtr lparam)
        {
            enumeratedWindows.Add(hwnd);
            return true;
        }

        public static IEnumerable<MonitorInfo> GetMonitors()
        {
            return EnumDisplayMonitors().Select(monitor => new MonitorInfo(monitor));
        }
        
        private static bool isEnumeratingMonitors = false;
        private static readonly List<IntPtr> enumeratedMonitors = new List<IntPtr>();
        
        private static IEnumerable<IntPtr> EnumDisplayMonitors()
        {
            if(isEnumeratingMonitors) throw new InvalidOperationException("Only one EnumDisplayMonitors() can be called at the same time.");
            isEnumeratingMonitors = true;
            
            enumeratedMonitors.Clear();
            
            try
            {
                EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, EnumDisplayMonitorsCallback, IntPtr.Zero);
            }
            finally
            {
                isEnumeratingMonitors = false;
            }

            return enumeratedMonitors.ToArray();
        }
        
        [MonoPInvokeCallback(typeof(EnumMonitorsDelegate))]
        private static bool EnumDisplayMonitorsCallback(IntPtr monitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr data)
        {
            enumeratedMonitors.Add(monitor);
            return true;
        }
    }
}