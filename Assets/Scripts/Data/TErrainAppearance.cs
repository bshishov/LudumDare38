using System;
using System.Linq;
using Assets.Scripts.Gameplay;
using UnityEngine;

namespace Assets.Scripts.Data
{
    [CreateAssetMenu(menuName = "Biology/Tile", fileName = "Tile")]
    public class TerrainAppearance : ScriptableObject
    {
        public enum TreesCount
        {
            None,
            Few,
            Many,
        }

        [Serializable]
        public class TreeAppearance
        {
            public ClimateCondition Condition;
            public GameObject TreePrefab;
        }
        
        public TreeAppearance[] Trees;

        public TreeAppearance GetTreeAppearance(Cell cell)
        {
            return Trees.FirstOrDefault(surface => surface.Condition.Match(cell));
        }

        public void Construct(Cell cell)
        {
            var tree = GetTreeAppearance(cell);
            if (tree != null && cell.LastTreeAppearance != tree)
            {
                var treesContainer = cell.transform.FindChild("Trees");
                if (treesContainer != null)
                    Destroy(treesContainer.gameObject);


                treesContainer = new GameObject("Trees").transform;
                treesContainer.SetParent(cell.transform, false);

                for (var i = 0; i < 4; i++)
                {
                    var rndPos = new Vector3(UnityEngine.Random.value - 0.5f, 0.5f, UnityEngine.Random.value - 0.5f);
                    var actualTree = (GameObject)Instantiate(tree.TreePrefab, treesContainer, false);
                    actualTree.transform.localPosition = rndPos;
                    actualTree.transform.localScale *= 0.5f * (1f - UnityEngine.Random.value * 0.3f);
                    actualTree.transform.Rotate(Vector3.up, UnityEngine.Random.value * 180f);
                }
                cell.LastTreeAppearance = tree;
            }
        }
    }
}
