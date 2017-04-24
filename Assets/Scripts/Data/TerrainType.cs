using System;

namespace Assets.Scripts.Data
{
    [Flags]
    public enum TerrainType
    {
        Water = 0,
        Plain = 1,
        Hills = 2,
        Mountains = 4,
        ShallowWater = 8,
    }
}