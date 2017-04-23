using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Utils
{
    [RequireComponent(typeof(Text))]
    public class FadeThenDestroy : MonoBehaviour
    {
        public float ShowTime = 1f;
        public float FadeTime = 1f;

        private bool _isShowing = true;
        private float _currentTime = 0f;
        private Text _text;
        
        void Start ()
        {
            _isShowing = true;
            _text = GetComponent<Text>();
        }
        
        void Update ()
        {
            if (_isShowing)
            {
                _currentTime += Time.deltaTime;
                if (_currentTime > ShowTime)
                {
                    _isShowing = false;
                    _currentTime = 0;
                    _text.CrossFadeColor(new Color(0f, 0f, 0f, 0f), FadeTime, false, true);
                    Destroy(gameObject, FadeTime);
                }
            }
            else
            {
                //_text.color = new Color(_text.color.r, _text.color.g, _text.color.b, _text.color.a - Time.deltaTime);
            }
        }
    }
}
