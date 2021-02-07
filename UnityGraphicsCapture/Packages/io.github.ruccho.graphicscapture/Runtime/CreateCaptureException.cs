using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ruccho.GraphicsCapture
{
    public class CreateCaptureException : System.Exception
    {
        internal CreateCaptureException()
        {
        }

        internal CreateCaptureException(string message) : base(message)
        {
        }
    }
}