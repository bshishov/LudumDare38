using UnityEngine;

namespace Assets.Scripts.Utils
{
    public class ToggleUIByKey : MonoBehaviour
    {
        public KeyCode Key;
        public bool StateAtStart = true;

        private CanvasGroup _canvasGroup;

        void Start ()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                gameObject.AddComponent(typeof(CanvasGroup));
                _canvasGroup = GetComponent<CanvasGroup>();
            }

            if(StateAtStart)
                EnableCanvas();
            else
                DisableCanvas();
        }


        void Update()
        {
            if (Input.GetKeyDown(Key) && _canvasGroup != null)
            {
                if (_canvasGroup.interactable)
                    DisableCanvas();
                else
                    EnableCanvas();
            }
        }

        void EnableCanvas()
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
        }

        void DisableCanvas()
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
        }
    }
}
