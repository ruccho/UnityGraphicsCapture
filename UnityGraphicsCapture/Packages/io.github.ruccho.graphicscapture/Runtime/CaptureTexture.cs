using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ruccho.GraphicsCapture
{
    [RequireComponent(typeof(Renderer))]
    public class CaptureTexture : MonoBehaviour
    {
        public ICaptureTarget CurrentTarget => client.CurrentTarget;
        
        private CaptureClient client = new CaptureClient();
        private Renderer targetRenderer = default;

        public void SetTarget(ICaptureTarget target)
        {
            try
            {
                client.SetTarget(target);
            }
            catch (CreateCaptureException e)
            {
                Debug.LogWarning("This target cannot be captured!");
            }
        }

        private void Update()
        {
            if (!targetRenderer) targetRenderer = GetComponent<Renderer>();
            if (!targetRenderer || !targetRenderer.material) return;

            var tex = client.GetTexture();
            var currentTex = targetRenderer.material.mainTexture;
            if (currentTex != tex)
                targetRenderer.material.mainTexture = tex;
        }

        private void OnDestroy()
        {
            client?.Dispose();
        }
    }
}