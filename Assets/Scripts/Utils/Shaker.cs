using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Utils
{
    public class Shaker : MonoBehaviour
    {
        public Vector3 Amplitude = new Vector3(0, 0.5f, 0);
        public float ShakeTime = 1f;

        private bool _isShaking;
        private float _currentShakeTime = 0f;
        private Vector3 _position;
        private float _modifier = 1f;


        void Start ()
        {
            _position = transform.position;
        }

        public void Shake(float modifier = 1f)
        {
            _isShaking = true;
            _currentShakeTime = 0f;
            _modifier = modifier;
            StartCoroutine(ShakeRoutine());
        }

        public IEnumerator ShakeRoutine()
        {
            while (_currentShakeTime < ShakeTime)
            {
                _currentShakeTime += Time.deltaTime;
                var mod = 1f - _currentShakeTime / ShakeTime;
                var s = Mathf.Sin(_currentShakeTime * 20f);
                transform.position = _position + Amplitude * s * mod * mod * _modifier;
                yield return new WaitForEndOfFrame();
            }

            yield return null;
        }
    }
}
