using System;

namespace Assets.Scripts.Data
{
    [Flags]
    public enum TerrainCondition
    {
        Any,
        OnlyWater,
        OnlyPlains,
        OnlyHills,
        OnlyMountains,
        PlainsOrHills,
        NotWater,
    }
}