using System;
using UnityEngine;

namespace Assets.Scripts.Data
{
    [CreateAssetMenu(menuName = "Biology/Appearance", fileName = "Appearance")]
    public class PropsAppearance : ScriptableObject
    {
        public GameObject Prefab;

        [Header("Requirements")]
        public ClimateCondition Condition;
        public Species Species;
        public uint MinCount = 0;
        

        [Range(1, 10)]
        public uint CountMin = 1;
        [Range(1, 10)]
        public uint CountMax = 1;

        [Header("Placement")]
        public float Scale = 1f;
        public float ScaleSpread = 0.2f;
        public float Rotation = 0f;
        public float RotationSpread = 0f;
        public float PositionSpread = 0f;
        public Vector3 BasePosition = Vector3.zero;
    }
}