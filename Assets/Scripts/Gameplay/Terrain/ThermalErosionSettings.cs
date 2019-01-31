using System;
using UnityEngine;

namespace Assets.Scripts.Gameplay.Terrain
{
    [Serializable]
    public class ThermalErosionSettings
    {
        [Range(1, 100)]
        public int Iterations = 10;

        public float MinSlope = 1.0f;

        [Range(0, 1)]
        public float Blend;
    }
}