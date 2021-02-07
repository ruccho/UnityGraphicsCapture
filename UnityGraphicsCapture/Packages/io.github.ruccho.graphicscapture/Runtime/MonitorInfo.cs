using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Ruccho.GraphicsCapture
{
    public class MonitorInfo : ICaptureTarget
    {

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        private const int CCHDEVICENAME = 32;
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct MonitorInfoEx
        {
            public int Size;
            public RECT Monitor;
            public RECT WorkArea;
            public uint Flags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
            public string DeviceName;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfoEx lpmi);
        
        public CaptureTargetType TargetType => CaptureTargetType.Monitor;
        public IntPtr Handle { get; }
        public string Description { get; }
        
        public MonitorInfoEx Info { get; private set; }

        public MonitorInfo(IntPtr handle)
        {
            Handle = handle;

            UpdateMonitorInfo();

            Description = Info.DeviceName;

        }

        public void UpdateMonitorInfo()
        {
            
            MonitorInfoEx mi = new MonitorInfoEx();
            mi.Size = Marshal.SizeOf(mi);
            if (GetMonitorInfo(Handle, ref mi))
            {
                Info = mi;
            }
            
        }
        

        public bool IsCapturable() => true;
    }
}