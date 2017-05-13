using UnityEngine;

namespace Assets.Scripts.Utils
{
    public static class Statistics
    {
        public static float SoftRange(float x, float min, float max)
        {
            //const float factor = 0.2f; // ~0.5f on the bounds
            //const float factor = 0.15f; // ~0.4f on the bounds
            const float factor = 0.1f; // ~0.22f on the bounds

            var range = max - min;
            var median = min + range * 0.5f;
            return Mathf.Exp(-Mathf.Pow(x - median, 2) / (factor * 2f * range * range));
        }

        public static float RandomExp(float w = 1f)
        {
            return -Mathf.Log(1 - Random.value) * w;
        }
    }
}
