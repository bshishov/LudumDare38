using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Utils
{
    public class UIPointingEvent : MonoBehaviour
    {
        public Transform Target;
        public float FadeSpeed = 0.5f;
        public float Duration = 2f;
        
        
        private Image _image;
        
        void Start()
        {
            _image = GetComponent<Image>();
            if (_image != null)
            {
                _image.canvasRenderer.SetAlpha(0f);
                //_image.color = new Color32(255, 255, 255,0);
                _image.CrossFadeAlpha(1f, FadeSpeed, false);
            }

            StartCoroutine(FadeCoroutine());
        }

        IEnumerator FadeCoroutine()
        {
            yield return new WaitForSeconds(Duration);
            _image.CrossFadeAlpha(0f, FadeSpeed, false);
            Destroy(gameObject, FadeSpeed);
        }

        void Update()
        {
            var rect = GetComponent<RectTransform>();
            var pos = new Vector3(Target.position.x, 0, Target.position.z);
            var viewportPosition = Camera.main.WorldToViewportPoint(pos);
            rect.anchorMin = viewportPosition;
            rect.anchorMax = viewportPosition;
        }
    }
}
