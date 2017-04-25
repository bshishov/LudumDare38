using UnityEngine;

namespace Assets.Scripts.UI
{
    public class UICircleMover : MonoBehaviour
    {
        public float Speed = 0.5f;
        public float Radius = 1f;

        private Vector3 _initialPosition;
        private float _offset;
        
        void Start ()
        {
            _offset = Random.value * 100f;
            _initialPosition = transform.position;
        }
	
        
        void Update ()
        {
            var x = Mathf.Sin(Time.time * Speed + _offset) * Radius;
            var y = Mathf.Cos(Time.time * Speed + _offset) * Radius;

            transform.position = _initialPosition + new Vector3(x, y);
        }
    }
}
