using UnityEngine;

namespace Assets.Scripts.Utils
{
    [RequireComponent(typeof(Camera))]
    public class TurnOnDepthBuffer : MonoBehaviour
    {
        void Start()
        {
            var mainCamera = GetComponent<Camera>();
            if (mainCamera != null)
            {
                if (mainCamera.depthTextureMode == DepthTextureMode.None)
                    mainCamera.depthTextureMode = DepthTextureMode.Depth;
            }
        }
    }
}
