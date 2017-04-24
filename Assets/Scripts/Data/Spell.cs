using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Data
{
    [CreateAssetMenu(menuName = "Biology/Spell", fileName = "Spell")]
    public class Spell : ScriptableObject
    {
        public string Name;
        public Sprite Icon;
        public float Cooldown;
        public GameObject Effect;
        public Buff CellBuff;
        public uint EffectWidth;
        public uint EffectHeight;
    }
}
