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
            return HeightCurve.Evaluate(Generator.GetHeight(u, v));
        }
    }
}
