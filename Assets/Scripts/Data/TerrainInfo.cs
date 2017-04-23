using System;
using UnityEngine;

namespace Assets.Scripts.Data
{
    [Serializable]
    public class TerrainInfo
    {
        public string Name;
        public Sprite Icon;
        public TerrainType TerrainType;
        public float MinHeight = 0;
        public float MaxHeight = 0;
    }
}