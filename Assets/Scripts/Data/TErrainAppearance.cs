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
            var appearances = new List<PropsAppearance>();

            foreach (var appearance in Appearances)
            {
                if (appearance.Condition.Match(cell))
                {
                    if (appearance.RequiredGroup != null)
                    {
                        if(cell.HasGroup(appearance.RequiredGroup))
                            appearances.Add(appearance);
                    }
                    else
                    {
                        appearances.Add(appearance);
                    }
                }
            }

            return appearances;
        }
    }
}
