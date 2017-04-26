using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.Utils;
using UnityEngine;

namespace Assets.Scripts.Data
{
    [CreateAssetMenu(menuName = "Biology/Buff", fileName = "Buff")]
    public class Buff : ScriptableObject
    {
        public float Duration = 1f;
        public Sprite Icon;
        public GameObject EffectPrefab;
        public AudioClipWithVolume SoundEffect;

        [Header("Effect")]
        public float TempeartureChange;
        public float HumidityChange;
        public float ExctinctionRate;
        public float WaterLevelChangeRate;

        [Serializable]
        public struct Passanger
        {
            public Species Species;
            public long Count;
        }

        [Header("Life")]
        public Passanger[] Passangers;

    }
}
