using UnityEngine;

namespace Assets.Scripts.Utils
{
    public class Rotator : MonoBehaviour
    {
        public Vector3 Rotation;

        void Update ()
        {
            transform.Rotate(Rotation * Time.deltaTime);
        }
    }
}
