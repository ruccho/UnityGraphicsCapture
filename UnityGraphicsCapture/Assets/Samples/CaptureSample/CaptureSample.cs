using System.Collections.Generic;
using System.Linq;
using Ruccho.GraphicsCapture;
using UnityEngine;
using UnityEngine.UI;

public class CaptureSample : MonoBehaviour
{
    [SerializeField]
    private RawImage previewImage = default; 

    private Capture capture = default;

    private void Start()
    {
        IEnumerable<ICaptureTarget> targets = Utils.GetTargets();
        var target = targets.First();
        Debug.Log(target.Description);
        capture = new Capture(target);
        capture.Start();
    }

    private void Update()
    {
        if(capture != null)
        {
            //Call GetTexture() every frame to update Unity's texture from native texture.
            previewImage.texture = capture.GetTexture();
        }
    }

    private void OnDestroy()
    {
        //Don't forget to stop capturing manually.
        capture?.Dispose();
    }
}