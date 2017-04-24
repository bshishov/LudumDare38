using UnityEngine;

namespace Assets.Scripts.Gameplay
{
    public class AnimalWalker : MonoBehaviour
    {
        private float _offset;

        void Start ()
        {
            _offset = Random.value * 200f;
        }
        
        void FixedUpdate()
        {
            var vel = Mathf.Pow(Mathf.Sin(Time.time + _offset), 6);
            transform.Translate(Vector3.forward * vel * 0.004f);
            transform.Rotate(Vector3.up, (1f - vel) * 84f * Time.deltaTime);
        }
    }
}
