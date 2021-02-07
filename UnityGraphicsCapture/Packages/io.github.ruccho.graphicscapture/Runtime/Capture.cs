using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Ruccho.GraphicsCapture
{
    public class Capture : IDisposable
    {
        [DllImport("GraphicsCapture")]
        private static extern IntPtr CreateCaptureFromWindow(IntPtr hWnd);
        
        [DllImport("GraphicsCapture")]
        private static extern IntPtr CreateCaptureFromMonitor(IntPtr hMon);

        [DllImport("GraphicsCapture")]
        private static extern void StartCapture(IntPtr capture);

        [DllImport("GraphicsCapture")]
        private static extern void CloseCapture(IntPtr capture);

        [DllImport("GraphicsCapture")]
        private static extern int GetWidth(IntPtr capture);

        [DllImport("GraphicsCapture")]
        private static extern int GetHeight(IntPtr capture);

        [DllImport("GraphicsCapture")]
        private static extern IntPtr GetTexturePtr(IntPtr capture);

        private IntPtr SelfPtr { get; } = IntPtr.Zero;

        private int prevWidth = -1;
        private int prevHeight = -1;
        private IntPtr prevPtr = IntPtr.Zero;
        
        private Texture2D CurrentTexture { get; set; }

        public bool IsCapturing { get; private set; }

        private bool IsDisposed { get; set; }

        public Capture(ICaptureTarget target)
        {
            switch(target.TargetType)
            {
                case CaptureTargetType.Window:
                    SelfPtr = CreateCaptureFromWindow(target.Handle);
                    break;
                case CaptureTargetType.Monitor:
                    SelfPtr = CreateCaptureFromMonitor(target.Handle);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            if (SelfPtr == IntPtr.Zero)
            {
                throw new CreateCaptureException("CreateCapture() returned null. Failed to create Capturing instance.");
            }
        }

        public void Start()
        {
            if(IsDisposed) throw new ObjectDisposedException(nameof(Capture));
            if(IsCapturing) throw new InvalidOperationException();
            StartCapture(SelfPtr);
            IsCapturing = true;
        }

        public Texture2D GetTexture()
        {
            if (!IsCapturing || IsDisposed) return null;
            
            int width = GetWidth(SelfPtr);
            int height = GetHeight(SelfPtr);
            IntPtr tex = GetTexturePtr(SelfPtr);

            if (width <= 0 || height <= 0 || tex == IntPtr.Zero) return null;

            if(prevPtr != tex)
            {

                if (CurrentTexture == null || width != prevWidth || height != prevHeight)
                {
                    CurrentTexture =
                        Texture2D.CreateExternalTexture(width, height, TextureFormat.BGRA32, false, false, tex);
                    CurrentTexture.filterMode = FilterMode.Bilinear;
                }
                else
                {
                    CurrentTexture.UpdateExternalTexture(tex);
                }
                
                prevWidth = width;
                prevHeight = height;
                prevPtr = tex;
            }
            
            return CurrentTexture;
        }

        public void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;
            
            if (SelfPtr != IntPtr.Zero)
                CloseCapture(SelfPtr);
            IsCapturing = false;
        }
        
        #if UNITY_EDITOR

        ~Capture()
        {
            if (IsDisposed || SelfPtr == IntPtr.Zero) return;
            Debug.LogWarning("[GraphicsCapture] A capture object has not been disposed! Make sure capture object to be disposed to stop unexpected capturing.");
        }
        
        #endif
    }
}