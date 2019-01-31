using Assets.Scripts.Utils;
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
        public int EffectWidth;
        public int EffectHeight;
        public float DelayBeforeBuff;
        public AudioClipWithVolume Sound;

        public int HalfWidth
        {
            get { return EffectWidth/2; }
        }

        public int HalfHeight
        {
            get { return EffectHeight / 2; }
        }

        public string GetSizeVerbose()
        {
            return string.Format("{0}x{1}", EffectWidth, EffectHeight);
        }
    }
}
