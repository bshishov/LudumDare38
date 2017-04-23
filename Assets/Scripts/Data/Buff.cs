using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Data
{
    public class Buff : ScriptableObject
    {
        public float Duration = 1f;
        public Sprite Icon;
        public GameObject EffectPrefab;
        public uint AreaWidth;
        public uint AreaHeight;
    }
}
