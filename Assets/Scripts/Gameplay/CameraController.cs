using Assets.Scripts.Gameplay.Terrain;
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
        public TerrainManager Terrain;

        private bool _isPanning;
        private Vector3 _lastMousePos;
        private Vector3 _panTarget;
        private Quaternion _panRotation;
        private float _zoom = 0f;
        private Vector3 _center;

        void Awake()
        {
            if (Terrain != null)
            {
                Terrain.TerrainCreated += OnTerrainCreated;
            }
        }

        void Start ()
        {
            _panTarget = transform.position;
            _panRotation = transform.rotation;
        }

        private void OnTerrainCreated()
        {
            Debug.Log("Camera setup");
            var tc = Terrain.Center;
            _center = tc + new Vector3(0, 0, -5);
            _panTarget = new Vector3(tc.x, transform.position.y, tc.y);
            transform.position = _panTarget;
            Bounds = new Vector3(Terrain.Size.x, 0, Terrain.Size.z) * 0.5f;
        }

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
                _panTarget.x = Mathf.Clamp(_panTarget.x, _center.x - Bounds.x, _center.x + Bounds.x);
                _panTarget.z = Mathf.Clamp(_panTarget.z, _center.z - Bounds.z, _center.z + Bounds.z);
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
            }

            var dt = Time.deltaTime*10;
            transform.position = Vector3.Lerp(transform.position, _panTarget + new Vector3(0, -1f, 0.2f).normalized * _zoom, dt);
            transform.rotation = Quaternion.Lerp(transform.rotation, _panRotation * Quaternion.AngleAxis(-_zoom * 1f, Vector3.right), dt);
        }
    }
}
