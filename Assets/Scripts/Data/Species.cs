using UnityEngine;

namespace Assets.Scripts.Data
{
    [CreateAssetMenu(menuName = "Biology/Species", fileName = "Species")]
    public class Species : ScriptableObject
    {
        [Header("General information")]
        public Texture2D Icon;
        public string Name;
        public Group Group;

        [Range(0, 10)]
        public int Size;

        [Header("Survivability")]
        public ClimateCondition Climate;
        public float ReproductionRate;

        [Header("Food")]
        public Food[] Feed;

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
