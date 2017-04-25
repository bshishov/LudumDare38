using System;
using UnityEngine;

namespace Assets.Scripts.Data
{
    [Serializable]
    public class ClimateState
    {
        [Range(-148, 212)]
        public float Temperature;
        public float Humidity;

        public float TemperatureAsCelsius()
        {
            return (Temperature - 32f) / 1.8f;
        }
    }
}
