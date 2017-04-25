using UnityEngine;

namespace Assets.Scripts.Data
{
    [CreateAssetMenu(menuName = "Biology/Objective", fileName = "Objective")]
    public class Objective : ScriptableObject
    {
        public string Name;
        public string Description;
        public Sprite Icon;

        [Header("Requirements")]
        public Objective RequiredObjective;
        public Species RequiredSpecies;
        public float RequiredCount;
    }
}
