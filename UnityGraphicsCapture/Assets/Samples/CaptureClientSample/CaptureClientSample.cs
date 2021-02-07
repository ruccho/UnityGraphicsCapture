using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Ruccho.GraphicsCapture;
using UnityEngine.UI;

public class CaptureClientSample : MonoBehaviour
{
    [SerializeField] private RawImage previewImage = default;
    [SerializeField] private Dropdown captureTargetSelect = default;

    private CaptureClient client = new CaptureClient();
    private IEnumerable<ICaptureTarget> listedTargets;

    
    void Start()
    {
        UpdateTargetList();
        client.SetTarget(listedTargets.First());
    }

    public void UpdateTargetList()
    {
        var windows = Utils.GetTargets().Where(w => w.IsCapturable());

        listedTargets = windows;
        captureTargetSelect.options = listedTargets.Select(w => new Dropdown.OptionData(w.Description)).ToList();
    }


    public void SetTargetIndex(int index)
    {
        try
        {
            client.SetTarget(listedTargets.ElementAt(index));
        }
        catch (CreateCaptureException e)
        {
            Debug.LogWarning("This target cannot be captured.");
        }
    }

    void Update()
    {
        if (client != null)
        {
            previewImage.texture = client.GetTexture();
        }
    }

    private void OnDestroy()
    {
        client?.Dispose();
    }
}