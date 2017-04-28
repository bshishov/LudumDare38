using System;
using UnityEngine;

namespace Assets.Scripts.Data
{
    [CreateAssetMenu(menuName = "Biology/World", fileName = "World")]
    public class World : ScriptableObject
    {
        [Header("General")] public string Name;
        public Sprite Icon;

        [Header("Terrain")] [Tooltip("Height must be from -1 to 3")] public AnimationCurve HeightCurve =
            AnimationCurve.EaseInOut(0, -1, 1, 3);

        [SerializeField]
        public TextureGenerator TextureGenerator;

        [SerializeField]
        public ProceduralGenerator ProceduralGenerator;

        [Header("Climate")] [Range(100f, 10000f)] public float StepsPerYear = 3000f;

        [Header("Temperature (°F)")] public AnimationCurve TemperatureOverYear = AnimationCurve.EaseInOut(0, 0, 1, 100);

        [Tooltip("0 is the North, 1.0 is the south")] public AnimationCurve TemperatureOverLattitude =
            AnimationCurve.EaseInOut(0, 0, 1, 100);

        [Tooltip("0 is the North, 1.0 is the south")] public AnimationCurve TemperatureOverHeight =
            AnimationCurve.EaseInOut(-1, 0, 3, 100);

        [Header("Humidity (%)")] public AnimationCurve HumidityOverYear = AnimationCurve.EaseInOut(0, 0, 1, 100);

        [Tooltip("0 is the North, 1.0 is the south")] public AnimationCurve HumidityOverLattitude =
            AnimationCurve.EaseInOut(0, 0, 1, 100);

        [Tooltip("0 is the North, 1.0 is the south")] public AnimationCurve HumidityOverHeight =
            AnimationCurve.EaseInOut(-1, 0, 3, 100);

        public float GetSeason(float step)
        {
            return (step%StepsPerYear)/StepsPerYear;
        }

        public float GetTemperature(float step, float lattitude, float height)
        {
            var val = TemperatureOverLattitude.Evaluate(lattitude);
            val += TemperatureOverYear.Evaluate(GetSeason(step));
            val += TemperatureOverHeight.Evaluate(height);
            return Mathf.Clamp(val, -148, 212);
        }

        public float GetHumidity(float step, float lattitude, float height)
        {
            var val = HumidityOverLattitude.Evaluate(lattitude);
            val += HumidityOverYear.Evaluate(GetSeason(step));
            val += HumidityOverHeight.Evaluate(height);
            return Mathf.Clamp(val, 0f, 100f);
        }

        public float GetHeight(float u, float v)
        {
            if(ProceduralGenerator != null)
                return HeightCurve.Evaluate(ProceduralGenerator.GetHeight(u, v));

            if (TextureGenerator != null)
                return HeightCurve.Evaluate(TextureGenerator.GetHeight(u, v));

            return 0f;
        }
    }

    public interface IGenerator
    {
        float GetHeight(float u, float v);
    }

    [Serializable]
    [CreateAssetMenu(menuName = "Biology/Generators/Texure", fileName = "Texture Generator")]
    public class TextureGenerator : ScriptableObject, IGenerator
    {
        public Texture2D HeightMap;
        public float GetHeight(float u, float v)
        {
            return HeightMap.GetPixelBilinear(u, v).r;
        }
    }

    [Serializable]
    [CreateAssetMenu(menuName = "Biology/Generators/Procedural", fileName = "Procedural Generator")]
    public class ProceduralGenerator : ScriptableObject, IGenerator
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

        public float GetHeight(float u, float v)
        {
            float amplitude = Damping ? Strength / Fequency : Strength;
            var s = Noise.Sum(Noise.Perlin3D, new Vector3(u, v, _seed * 5f), Fequency, Octaves, Lacunarity, Persistence);
            return s * amplitude + Offset;
        }
    }
}
