using System;
using UnityEngine;

namespace Assets.Scripts.Data
{
    [Serializable]
    public struct Migration
    {
        public ClimateCondition ClimateCondition;

        [Range(0, 1)]
        public float Chance;

        [Range(0, 1)]
        public float CountFactor;
    }
}