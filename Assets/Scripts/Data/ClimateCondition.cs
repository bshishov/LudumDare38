using System;
using Assets.Scripts.EditorExt;
using Assets.Scripts.Gameplay;
using UnityEngine;

namespace Assets.Scripts.Data
{
    [Serializable]
    public struct ClimateCondition
    {
        [SerializeField][EnumFlags] TerrainType Terrain;
        public ClimateState MinClimate;
        public ClimateState MaxClimate;

        public float CalcComfort(Cell cell)
        {
            return CalcComfort(cell.Climate, cell.TerrainType);
        }

        public float CalcComfort(ClimateState state, TerrainType terrainType)
        {
            if ((Terrain & terrainType) != terrainType)
                return 0f;
            var tempMod = Mathf.Abs(state.Temperature - MinClimate.Temperature) / (MaxClimate.Temperature - MinClimate.Temperature);
            var humidityMod = Mathf.Abs(state.Humidity - MinClimate.Humidity) / (MaxClimate.Humidity - MinClimate.Humidity);
            return Mathf.Clamp01(1f - Mathf.Abs(tempMod - 0.5f) * 2f) * Mathf.Clamp01(1f - Mathf.Abs(humidityMod - 0.5f) * 2f);
        }

        public bool Match(Cell cell)
        {
            if ((Terrain & cell.TerrainType) != cell.TerrainType)
                return false;
            if (cell.Climate.Temperature < MinClimate.Temperature)
                return false;
            if (cell.Climate.Temperature > MaxClimate.Temperature)
                return false;
            if (cell.Climate.Humidity < MinClimate.Humidity)
                return false;
            if (cell.Climate.Humidity > MaxClimate.Humidity)
                return false;

            return true;
        }
    }
}