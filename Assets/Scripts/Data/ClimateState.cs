using System;

namespace Assets.Scripts.Data
{
    [Serializable]
    public struct ClimateState
    {
        public float Temperature;
        public float Humidity;

        public float TemperatureAsCelsius()
        {
            return (Temperature - 32f) / 1.8f;
        }
    }
}
