using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Data
{
    [CreateAssetMenu(menuName = "Biology/Terrain Types Collection", fileName = "Terrain Types Collection")]
    public class TerrainTypesCollection : ScriptableObject
    {
        public TerrainInfo[] TerrainTypes;

        public TerrainInfo GetTerrainInfo(TerrainType type)
        {
            return TerrainTypes.FirstOrDefault(t => t.TerrainType.Equals(type));
        }

        public TerrainInfo GetForHeight(float height)
        {
            return TerrainTypes.FirstOrDefault(terrainType => height >= terrainType.MinHeight && height <= terrainType.MaxHeight);
        }
    }
}
