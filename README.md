# UnityGraphicsCapture

UnityGraphicsCapture is an utility to get window/monitor image as Texture2D through WinRT Windows.Graphics.Capture API.

## Requirements
- Windows 10 (1803 or later)
- Tested on Unity 2019.4

## Getting started

### Installation

This package is provided as UPM package.

1. Open Package Manager window.
2. Click `+` > `Add package from git URL...`
3. Enter `https://github.com/ruccho/UnityGraphicsCapture.git?path=/UnityGraphicsCapture/Packages/io.github.ruccho.graphicscapture`

### `CaptureTexture` component
`CaptureTexture` (in namespace `Ruccho.GraphicsCapture`) is a component that get window/monitor texture and set it to a renderer. You can try it by attaching `CaptureTexture` to primitive objects with renderer such as Quad. During playing, capture target can be chosen from inspector of `CaptureTexture`.

![image](https://user-images.githubusercontent.com/16096562/107139638-298fd100-6960-11eb-8fd2-abe8315bca6a.png)



If you want to set capture target from your scripts, use `CaptureTexture.SetTarget(ICaptureTarget target)`. All capturable `ICaptureTarget` can be enumerated by `Ruccho.GraphicsCapture.Utils.GetTargets()`.

```csharp
using Ruccho.GraphicsCapture;

IEnumerable<ICaptureTarget> targets = Utils.GetTargets();
GetComponent<CaptureTexture>().SetTarget(targets.First());
```

### Capture / CaptureClient

If you just want to get Texture2D of windows/monitors, use `CaptureClient` or `Capture` class.  When using them, make sure to `Dispose()` them to stop capturing explicitly. 

Both `Capture` and `CaptureClient`, call `GetTexture()` to get texture of capture target. It needs to be called every frame to update Unity's texture from native DirectX texture.

#### Capture

`Capture` is the simplest class to capture windows/monitors and it needs to be instantiated by each session for single capture target.

```csharp:CaptureSample.cs
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
```

#### CaptureClient

`CaptureClient` is more high-level class than `Capture`. It can switch capture targets on one instance by `CaptureClient.SetTarget(ICaptureTarget target)`.

```csharp:CaptureClientSample.cs
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ruccho.GraphicsCapture;
using UnityEngine;
using UnityEngine.UI;

public class CaptureClientSample : MonoBehaviour
{
    [SerializeField] private RawImage previewImage = default;

    private CaptureClient client = new CaptureClient();

    private void Start()
    {
        StartCoroutine(ChangeTarget());
    }

    private IEnumerator ChangeTarget()
    {
        for(int i = 0; ; i++)
        {
            IEnumerable<ICaptureTarget> targets = Utils.GetTargets();
            int targetIndex = i % targets.Count();
            var target = targets.ElementAt(targetIndex);

            bool failed = false;
            try
            {
                client.SetTarget(target);
            }
            catch (CreateCaptureException e)
            {
                //SetTarget() throws CreateCaptureException when it failed to start capturing.
                failed = true;
            }

            if (failed)
            {
                yield return null;
                continue;
            }
            
            yield return new WaitForSeconds(3f);
        }
    }

    private void Update()
    {
        //Call GetTexture() every frame to update Unity's texture from native texture.
        previewImage.texture = client.GetTexture();
    }

    private void OnDestroy()
    {
        //Don't forget to stop capturing manually.
        client?.Dispose();
    }
}
```

## Background

UnityGraphicsCapture uses Windows.Graphics.Capture API, a member of Windows Runtime (WinRT), so it cannot be used directly from managed code on Windows standalone build. UnityGraphicsCapture has a native plugin implemented with C++/WinRT. It gets `ID3D11Texture*` from WinRT and passes it through Unity's low-level native plugin interface. Unity accesses the texture directly.

## References

microsoft/Windows.UI.Composition-Win32-Samples  
https://github.com/Microsoft/Windows.UI.Composition-Win32-Samples/tree/master/dotnet/WPF/ScreenCapture

robmikh/Win32CaptureSample  
https://github.com/robmikh/Win32CaptureSample