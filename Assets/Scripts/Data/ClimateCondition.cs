using System;

namespace Assets.Scripts.Data
{
    [Serializable]
    public struct ClimateCondition
    {
        public BiologyTerrain BiologyTerrain;
        public ClimateState MinClimate;
        public ClimateState MaxClimate;
    }
}