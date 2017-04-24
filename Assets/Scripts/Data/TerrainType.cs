using System;

namespace Assets.Scripts.Data
{
    [Flags]
    public enum TerrainType
    {
        Water = 1,
        Plain = 2,
        Hills = 4,
        Mountains = 8,
        ShallowWater = 16,
    }
}