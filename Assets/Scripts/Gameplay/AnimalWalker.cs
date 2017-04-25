using UnityEngine;

namespace Assets.Scripts.Gameplay
{
    public class AnimalWalker : MonoBehaviour
    {
        public Vector3 ForwardDirection = Vector3.forward;
        private float _offset;

        void Start ()
        {
            _offset = Random.value * 200f;
        }
        
        void FixedUpdate()
        {
            var vel = Mathf.Pow(Mathf.Sin(Time.time + _offset), 6);
            transform.Translate(ForwardDirection * vel * 0.004f);
            transform.Rotate(Vector3.up, (1f - vel) * 0.5f * 84f * Time.deltaTime);
        }
    }
}
