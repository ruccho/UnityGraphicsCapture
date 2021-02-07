using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ruccho.GraphicsCapture
{
    public interface ICaptureTarget
    {
        CaptureTargetType TargetType { get; }
        
        IntPtr Handle { get; }
        
        string Description { get; }

        bool IsCapturable();
    }

    public enum CaptureTargetType
    {
        Window, Monitor
    }
}