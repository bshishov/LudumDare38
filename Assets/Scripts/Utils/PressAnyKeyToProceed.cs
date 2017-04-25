using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Utils
{
    public class PressAnyKeyToProceed : MonoBehaviour
    {
        public string NextLevel;
        
        void Start ()
        {
        }
	
        
        void Update ()
        {
            if (Input.anyKey)
            {
                if(!string.IsNullOrEmpty(NextLevel))
                    SceneManager.LoadScene(NextLevel);
            }
        }
    }
}
