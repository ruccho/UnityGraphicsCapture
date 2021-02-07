using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace Ruccho.GraphicsCapture
{
    public class WindowInfo : ICaptureTarget
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowInfo(IntPtr hwnd, ref WINDOWINFO pwi);

        [DllImport("user32.dll")]
        private static extern IntPtr GetShellWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindowVisible(IntPtr hWnd);
        
        [DllImport("user32.dll")]
        private static extern IntPtr GetLastActivePopup(IntPtr hWnd);
        
        enum GetAncestorFlags
        {
            // Retrieves the parent window. This does not include the owner, as it does with the GetParent function.
            GetParent = 1,
            // Retrieves the root window by walking the chain of parent windows.
            GetRoot = 2,
            // Retrieves the owned root window by walking the chain of parent and owner windows returned by GetParent.
            GetRootOwner = 3
        }
        
        [DllImport("user32.dll", ExactSpelling = true)]
        private static extern IntPtr GetAncestor(IntPtr hwnd, GetAncestorFlags flags);

        private static readonly int WS_DISABLED = 0x8000000;
        private static readonly int WS_VISIBLE = 0x10000000;
        private static readonly int WS_EX_TOOLWINDOW = 0x00000080;

        private static WINDOWINFO GetWindowInfo(IntPtr hWnd, out int retCode)
        {
            var wi = new WINDOWINFO();
            wi.cbSize = Marshal.SizeOf(wi);
            retCode = GetWindowInfo(hWnd, ref wi);
            return wi;
        }


        public CaptureTargetType TargetType => CaptureTargetType.Window;
        public IntPtr Handle { get; }
        public string Description => Title;

        public string Title { get; private set; }
        public int Pid { get; }
        public string ClassName { get; }

        public WINDOWINFO Info { get; private set; }

        public WindowInfo(IntPtr handle)
        {
            if (handle == IntPtr.Zero) throw new NullReferenceException();

            Handle = handle;

            UpdateWindowTitle();

            var csb = new StringBuilder(256);
            if (GetClassName(Handle, csb, csb.Capacity) == 0)
            {
                ClassName = "";
            }
            else
                ClassName = csb.ToString();

            GetWindowThreadProcessId(Handle, out int pid);
            Pid = pid;
        }

        public bool UpdateWindowTitle()
        {
            int textLen = GetWindowTextLength(Handle);
            if (textLen == 0)
            {
                Title = "";
                return false;
            }
            else
            {
                var sb = new StringBuilder(textLen + 1);
                GetWindowText(Handle, sb, sb.Capacity);

                Title = sb.ToString();
                return true;
            }
        }

        public bool UpdateWindowInfo()
        {
            Info = GetWindowInfo(Handle, out int ret);
            return ret != 0;
        }
        
        private static IntPtr GetLastVisibleActivePopUpOfWindow(IntPtr hwnd) {
            for(;;) {
                IntPtr h = GetLastActivePopup(hwnd);
                if(IsWindowVisible(h)) {
                    return h;
                } else if (h == hwnd) {
                    return IntPtr.Zero;
                }
                hwnd = h;
            }
        }

        public bool IsCapturable()
        {
            UpdateWindowTitle();
            UpdateWindowInfo();

            if (string.IsNullOrEmpty(Title)) return false;
            if (Handle == GetShellWindow()) return false;
            if (!IsWindowVisible(Handle)) return false;
            if ((Info.dwStyle & WS_DISABLED) != 0) return false;
            if ((Info.dwStyle & WS_VISIBLE) == 0) return false;
            if ((Info.dwExStyle & WS_EX_TOOLWINDOW) != 0) return false;
            if (GetAncestor(Handle, GetAncestorFlags.GetRoot) != Handle) return false;

            var rootOwner = GetAncestor(Handle, GetAncestorFlags.GetRootOwner);
            var last = GetLastVisibleActivePopUpOfWindow(rootOwner);
            if (last != Handle) return false;

            return true;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WINDOWINFO
    {
        public int cbSize;
        public RECT rcWindow;
        public RECT rcClient;
        public int dwStyle;
        public int dwExStyle;
        public int dwWindowStatus;
        public uint cxWindowBorders;
        public uint cyWindowBorders;
        public short atomWindowType;
        public short wCreatorVersion;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;

        public int width
        {
            get { return right - left; }
        }

        public int height
        {
            get { return bottom - top; }
        }
    }
}