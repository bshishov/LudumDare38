using System;
using UnityEngine;

namespace Assets.Scripts.Data
{
    [Serializable]
    [CreateAssetMenu(menuName = "Biology/Generators/Procedural", fileName = "Procedural Generator")]
    public class ProceduralGenerator : Generator
    {
        public float Strength = 1f;
        public float Fequency = 1f;
        [Range(1, 8)]
        public int Octaves = 1;
        [Range(1f, 4f)]
        public float Lacunarity = 2f;
        [Range(0f, 1f)]
        public float Persistence = 0.5f;
        public bool Damping;
        public float Offset = 0f;

        public bool OverrideSeed = false;
        public float Seed = 0f;

        private float _seed;

        void OnEnable()
        {
            _seed = OverrideSeed ? Seed : UnityEngine.Random.value;
        }

        public override float GetHeight(float u, float v)
        {
            float amplitude = Damping ? Strength / Fequency : Strength;
            var s = Noise.Sum(Noise.Perlin3D, new Vector3(u, v, _seed * 5f), Fequency, Octaves, Lacunarity, Persistence);
            return s * amplitude + Offset;
        }
    }
}