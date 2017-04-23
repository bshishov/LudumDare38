using UnityEngine;

namespace Assets.Scripts.Data
{
    [CreateAssetMenu(menuName = "Biology/Group", fileName = "Group")]
    public class Group : ScriptableObject
    {
        public string Name;
        public Sprite Icon;
        public Group ParentGroup;

        public bool IsInGroup(Group group)
        {
            if (this == group)
                return true;

            if (ParentGroup != null)
                return ParentGroup.IsInGroup(group);

            return false;
        }
    }
}
