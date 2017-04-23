using System;
using System.Linq;
using Assets.Scripts.Gameplay;
using UnityEngine;

namespace Assets.Scripts.Data
{
    [CreateAssetMenu(menuName = "Biology/Tile", fileName = "Tile")]
    public class TerrainAppearance : ScriptableObject
    {
        [Serializable]
        public class SurfaceAppearance
        {
            public ClimateCondition Condition;
            public GameObject SurfacePrefab;
        }

        [Serializable]
        public class TreeAppearance
        {
            public ClimateCondition Condition;
            public GameObject TreePrefab;
            public Vector3[] Positions;
            public float MinTrees = 0f;
            public float MaxTrees = 0f;
        }


        public SurfaceAppearance[] Surfaces;
        public TreeAppearance[] Trees;

        public SurfaceAppearance GetSurfaceAppearance(Cell cell)
        {
            return Surfaces.FirstOrDefault(surface => surface.Condition.Match(cell));
        }

        public TreeAppearance GetTreeAppearance(Cell cell)
        {
            return Trees.FirstOrDefault(surface => surface.Condition.Match(cell));
        }


        public void Construct(Cell cell)
        {
            var surfaceAppearence = GetSurfaceAppearance(cell);

            if (surfaceAppearence != null)
            {
                if (cell.LastSurfaceAppearance == null || cell.LastSurfaceAppearance != surfaceAppearence)
                {
                    foreach (Transform child in cell.transform)
                    {
                        Destroy(child.gameObject);
                    }                    

                    Instantiate(surfaceAppearence.SurfacePrefab, cell.transform, false);
                    cell.LastSurfaceAppearance = surfaceAppearence;
                }
            }

            var tree = GetTreeAppearance(cell);

        }
    }
}
