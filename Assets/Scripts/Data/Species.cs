using UnityEngine;

namespace Assets.Scripts.Data
{
    [CreateAssetMenu(menuName = "Biology/Species", fileName = "Species")]
    public class Species : ScriptableObject
    {
        [Header("General information")]
        public Sprite Icon;
        public string Name;
        public Group Group;
        public int Size;
        public float FoodValue;
        public float Agression;

        [Header("Survivability")]
        public ClimateCondition Climate;
        public float ReproductionRate;
        public float Hunger;
        public Group[] Feed;
        public Species[] Enemies;

        [Header("Mutations")]
        public Mutation[] Mutations;

        [Header("Migration")]
        public Migration[] Migrations;

        public bool IsInGroup(Group group)
        {
            if (Group == null)
                return false;

            if (this.Group == group)
                return true;

            return this.Group.IsInGroup(group);
        }
    }
}
