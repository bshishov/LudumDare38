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

        [Header("Terrain")] [Tooltip("Height must be from -1 to 3")] public AnimationCurve HeightCurve =
            AnimationCurve.EaseInOut(0, -1, 1, 3);

        [SerializeField]
        public Generator Generator;

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

        public float GetTemperature(float step, float latitude, float height)
        {
            var val = Mathf.Clamp(TemperatureOverLattitude.Evaluate(latitude), MinTemperature, MaxTemperature);
            val += Mathf.Clamp(TemperatureOverYear.Evaluate(GetSeason(step)), MinTemperature, MaxTemperature);
            val += Mathf.Clamp(TemperatureOverHeight.Evaluate(height), MinTemperature, MaxTemperature);
            return Mathf.Clamp(val, MinTemperature, MaxTemperature);
        }

        public float GetHumidity(float step, float latitude, float height)
        {
            var val = Mathf.Clamp(HumidityOverLattitude.Evaluate(latitude), MinHumidity, MaxHumidity);
            val += Mathf.Clamp(HumidityOverYear.Evaluate(GetSeason(step)), MinHumidity, MaxHumidity);
            val += Mathf.Clamp(HumidityOverHeight.Evaluate(height), MinHumidity, MaxHumidity);
            return Mathf.Clamp(val, MinHumidity, MaxHumidity);
        }

        public float GetHeight(float u, float v)
        {
            return HeightCurve.Evaluate(Generator.GetHeight(u, v));
        }
    }
}
