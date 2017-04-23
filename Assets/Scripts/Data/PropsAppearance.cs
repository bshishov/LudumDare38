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
        public uint MinCountInGroup = 0;
        public GameObject Prefab;
        public uint CountMin = 1;
        public uint CountMax = 1;
        public float Scale = 1f;
        public float ScaleSpread = 0.2f;
        public float Rotation = 0f;
        public float RotationSpread = 0f;
        public float PositionSpread = 0f;
        public Vector3 BasePosition = Vector3.zero;
    }
}