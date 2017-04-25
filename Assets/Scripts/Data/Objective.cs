using UnityEngine;

namespace Assets.Scripts.Data
{
    [CreateAssetMenu(menuName = "Biology/Objective", fileName = "Objective")]
    public class Objective : ScriptableObject
    {
        public string Name;
        public string Description;
        public Sprite Icon;
        public Objective RequiredObjective;


        [Header("Triggers")]
        public Species RequiredSpecies;
        public float RequiredCount;
        public Spell RequiredSpell;
    }
}
