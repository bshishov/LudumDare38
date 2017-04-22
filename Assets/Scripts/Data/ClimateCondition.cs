using System;

namespace Assets.Scripts.Data
{
    [Serializable]
    public struct ClimateCondition
    {
        public TerrainCondition Terrain;
        public ClimateState MinClimate;
        public ClimateState MaxClimate;
    }
}