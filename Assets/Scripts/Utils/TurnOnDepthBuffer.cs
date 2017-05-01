using UnityEngine;

namespace Assets.Scripts.Utils
{
    [RequireComponent(typeof(Camera))]
    public class TurnOnDepthBuffer : MonoBehaviour
    {
        void Start()
        {
            GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;
        }
    }
}
