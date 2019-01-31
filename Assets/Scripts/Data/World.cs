using Assets.Scripts.Gameplay.Terrain;
using UnityEngine;

namespace Assets.Scripts.Data
{
    [CreateAssetMenu(menuName = "Biology/World", fileName = "World")]
    public class World : ScriptableObject
    {
        private const float MinTemperature = -148;
        private const float MaxTemperature = 212;
        private const float MinHumidity = 0;
        private const float MaxHumidity = 100;

        [Header("General")] public string Name;
        public Sprite Icon;
        public int Width = 30;
        public int Height = 30;

        [Header("Terrain")]
        [Tooltip("Height must be from -1 to 3")]
        public AnimationCurve HeightCurve = AnimationCurve.EaseInOut(0, -1, 1, 3);

        [SerializeField]
        public Generator Generator;

        [SerializeField]
        public TerrainSettings TerrainSettings;

        [SerializeField]
        public TerrainTypesCollection TerrainTypes;

        [Header("Climate")] [Range(100f, 10000f)] public float StepsPerYear = 3000f;

        [Header("Temperature (°F)")]
        public float BaseTemperature = 0f;
        public AnimationCurve TemperatureOverYear = AnimationCurve.EaseInOut(0, 0, 1, 100);

        [Tooltip("0 is the North, 1.0 is the south")] public AnimationCurve TemperatureOverLattitude =
            AnimationCurve.EaseInOut(0, 0, 1, 100);

        [Tooltip("0 is the North, 1.0 is the south")] public AnimationCurve TemperatureOverHeight =
            AnimationCurve.EaseInOut(-1, 0, 3, 100);

        [Header("Humidity (%)")]
        public float BaseHumidity = 0f;
        public AnimationCurve HumidityOverYear = AnimationCurve.EaseInOut(0, 0, 1, 100);

        [Tooltip("0 is the North, 1.0 is the south")] public AnimationCurve HumidityOverLattitude =
            AnimationCurve.EaseInOut(0, 0, 1, 100);

        [Tooltip("0 is the North, 1.0 is the south")] public AnimationCurve HumidityOverHeight =
            AnimationCurve.EaseInOut(-1, 0, 3, 100);

        public float GetSeason(float step)
        {
            return (step%StepsPerYear)/StepsPerYear;
        }

        public float GetTemperature(float step, float latitude, float height)
        {
            //  t = t_base + t_over_lat + t_over_year + t_over_height
            var val = BaseTemperature;
            val += TemperatureOverLattitude.Evaluate(latitude);
            val += TemperatureOverYear.Evaluate(GetSeason(step));
            val += TemperatureOverHeight.Evaluate(height);
            return Mathf.Clamp(val, MinTemperature, MaxTemperature);
        }

        public float GetHumidity(float step, float latitude, float height)
        {
            //  h = h_base + h_over_lat + h_over_year + h_over_height
            var val = BaseHumidity;
            val += HumidityOverLattitude.Evaluate(latitude);
            val += HumidityOverYear.Evaluate(GetSeason(step));
            val += HumidityOverHeight.Evaluate(height);
            return Mathf.Clamp(val, MinHumidity, MaxHumidity);
        }

        public float GetHeight(float u, float v)
        {
            return HeightCurve.Evaluate(Generator.GetHeight(u, v));
        }
    }
}
