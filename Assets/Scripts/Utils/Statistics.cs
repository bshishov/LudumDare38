using UnityEngine;

namespace Assets.Scripts.Utils
{
    public static class Statistics
    {
        public static float SoftRange(float x, float min, float max)
        {
            /*
                         _ _ _ _     ____ 1.0
                      | /       \ |
                      |/         \|  ____ 0.22
                     /|           |\      
                   /  |           |  \ __ 0.0
                     min         max
            */

            //const float factor = 0.2f; // ~0.5f on the bounds
            //const float factor = 0.15f; // ~0.4f on the bounds
            const float factor = 0.1f; // ~0.22f on the bounds

            var range = max - min;
            var median = min + range * 0.5f;
            return Mathf.Exp(-Mathf.Pow(x - median, 2) / (factor * 2f * range * range));
        }

        /// <summary>
        /// Returns the value that indicates whether x is in range with smooth edges.
        /// Returns 1 at the center of range and 0 outside. Edges are smoothed.
        /// Range is normalized so edge smoothing factor is independent of min and max
        /// parameters.
        /// NOTE result on the bounds is always 0.5 !!!
        /// </summary>
        /// <param name="x">Value to sample</param>
        /// <param name="min">Left edge</param>
        /// <param name="max">Right edge</param>
        /// <param name="factor">Strength factor. The higher the value - the more strict are the edges</param>
        /// <returns></returns>
        public static float DoubleSigmoidRange(float x, float min, float max, float factor=20f)
        {
            // Scale x, min and max to 0..1 range
            // And also scale by factor
            x = factor * (x - min) / (max - min);

            // This is two sigmoid functions 1 / (1 + e^-x) centered at 0 and 1 respectfully
            // Second function has inverted x.
            // x is scaled by factor
            var left = 1 + Mathf.Exp(-x);
            var right = 1 + Mathf.Exp(x - factor);

            // Both functions are combined together
            return 1 / (left * right);
        }

        public static float RandomExp(float w = 1f)
        {
            return -Mathf.Log(1 - Random.value) * w;
        }
    }
}
