using System;
using UnityEngine;

namespace Assets.Scripts.Data
{
    [Serializable]
    public struct Mutation
    {
        public Species Target;
        public ClimateCondition ClimateCondition;

        [Range(0, 1)]
        public float Chance;
    }
}