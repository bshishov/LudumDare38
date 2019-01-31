using UnityEngine;

namespace Assets.Scripts.Utils
{
    public class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        public virtual bool IsPersistent
        {
            get { return false; }
        }

        public virtual bool AssertSingleInstance
        {
            get { return true; }
        }

        protected static T _instance;

        // Returns the instance of this singleton
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = (T)FindObjectOfType(typeof(T));
                    if (_instance == null)
                    {
                        Debug.LogError("An instance of " + typeof(T) +
                                       " is needed in the scene, but there is none.");
                    }
                    else
                    {
                        if (_instance.IsPersistent)
                        {
                            DontDestroyOnLoad(_instance);
                        }
                    }
                }

                return _instance;
            }
        }

        protected virtual void Awake()
        {
            if(AssertSingleInstance && _instance != null && _instance != this)
                DestroyImmediate(this);
        }
    }
}