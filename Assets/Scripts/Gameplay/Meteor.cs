using UnityEngine;

namespace Assets.Scripts.Gameplay
{
    public class Meteor : MonoBehaviour
    {
        public Vector3 Speed;
        public Vector3 Acceleration;
        public Vector3 RotationSpeed;
        public GameObject EffectOnHit;

        private Vector3 _speed;
        private bool _hit;

        void Start ()
        {
            _speed = Speed;
        }
	
        void Update ()
        {
            if (transform.position.y > 0f)
            {
                transform.Translate(_speed, Space.World);
                _speed += Acceleration;

                transform.Rotate(RotationSpeed);
            }
            else
            {
                transform.Translate(_speed * 0.02f, Space.World);

                if (!_hit)
                {
                    OnHit();
                    _hit = true;
                }
            }
        }

        void OnHit()
        {
            if (EffectOnHit != null)
            {
                var go = Instantiate(EffectOnHit, transform.position + Vector3.up * 1.5f, Quaternion.identity);
                Destroy(go, 3f);
            }
        }
    }
}
