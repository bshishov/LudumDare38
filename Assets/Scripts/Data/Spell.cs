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
        public float DelayBeforeBuff;

        public uint HalfWidth
        {
            get { return EffectWidth/2; }
        }

        public uint HalfHeight
        {
            get { return EffectHeight / 2; }
        }

        public string GetSizeVerbose()
        {
            return string.Format("{0}x{1}", EffectWidth, EffectHeight);
        }
    }
}
