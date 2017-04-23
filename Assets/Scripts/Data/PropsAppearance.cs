using System;
using UnityEngine;

namespace Assets.Scripts.Data
{
    [Serializable]
    public class PropsAppearance
    {
        public string ContainerName = "GenericAppearence";
        public ClimateCondition Condition;
        public Group RequiredGroup;
        public GameObject Prefab;
        public int CountMin = 1;
        public int CountMax = 1;
        public float Scale = 1f;
        public float ScaleSpread = 0.2f;
        public float Rotation = 0f;
        public float RotationSpread = 0f;
        public float PositionSpread = 0f;
        public Vector3 BasePosition = Vector3.zero;
    }
}