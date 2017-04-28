using UnityEngine;

namespace Assets.Scripts.Data
{
    public abstract class Generator : ScriptableObject
    {
        public abstract float GetHeight(float u, float v);
    }
}