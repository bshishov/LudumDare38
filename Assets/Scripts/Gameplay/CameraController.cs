using UnityEngine;

namespace Assets.Scripts.Gameplay
{
    public class CameraController : MonoBehaviour
    {
        public float PanSpeed = 1f;
        public float ZoomSpeed = 10f;
        public float MaxZoom = 10f;
        public float MinZoom = -1f;
        public Vector3 Bounds = new Vector3(10, 0, 10);

        public float MinTiltShift = 1f;
        public float MaxTiltShift = 7f;

        private bool _isPanning;
        private Vector3 _lastMousePos;
        private Vector3 _panTarget;
        private Quaternion _panRotation;
        private float _zoom = 0f;

        private UnityStandardAssets.ImageEffects.TiltShift _tiltShift; 

        void Start ()
        {
            _panTarget = transform.position;
            _panRotation = transform.rotation;

            _tiltShift = GetComponent<UnityStandardAssets.ImageEffects.TiltShift>();
        }
	
        // Update is called once per frame
        void Update ()
        {
            // Pan by RMB
            if (Input.GetMouseButton(1))
            {
                if (!_isPanning)
                {
                    _isPanning = true;
                    _lastMousePos = Input.mousePosition;
                }

                var delta = Input.mousePosition - _lastMousePos;
                _panTarget -= new Vector3(delta.x / Screen.width, 0, delta.y / Screen.height) * PanSpeed / (10 + _zoom);
                _panTarget.x = Mathf.Clamp(_panTarget.x, -Bounds.x, Bounds.x);
                _panTarget.z = Mathf.Clamp(_panTarget.z, -Bounds.z, Bounds.z);
                _lastMousePos = Input.mousePosition;
            }
            else
            {
                _isPanning = false;
            }

            if (Mathf.Abs(Input.GetAxis("Mouse ScrollWheel")) > 0)
            {
                var scroll = Input.GetAxis("Mouse ScrollWheel");
                _zoom += scroll * ZoomSpeed;
                _zoom = Mathf.Clamp(_zoom, MinZoom, MaxZoom);

                _tiltShift.blurArea = Mathf.Lerp(MinTiltShift, MaxTiltShift, (_zoom - MinZoom)/(MaxZoom - MinZoom));
            }

            var dt = Time.deltaTime*10;
            transform.position = Vector3.Lerp(transform.position, _panTarget + new Vector3(0, -1f, 0.2f).normalized * _zoom, dt);
            transform.rotation = Quaternion.Lerp(transform.rotation, _panRotation * Quaternion.AngleAxis(-_zoom * 1f, Vector3.right), dt);
        }
    }
}
