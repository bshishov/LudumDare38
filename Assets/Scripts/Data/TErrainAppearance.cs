using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Gameplay;
using UnityEngine;

namespace Assets.Scripts.Data
{
    [CreateAssetMenu(menuName = "Biology/Tile", fileName = "Tile")]
    public class TerrainAppearance : ScriptableObject
    {
        public PropsAppearance[] Appearances;

        public List<PropsAppearance> GetAppearancesFor(Cell cell)
        {
            return Appearances.Where(surface => surface.Condition.Match(cell)).ToList();
        }
    }
}
