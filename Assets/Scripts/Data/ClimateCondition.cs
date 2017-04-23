using System;
using Assets.Scripts.Gameplay;
using UnityEngine;

namespace Assets.Scripts.Data
{
    [Serializable]
    public struct ClimateCondition
    {
        public TerrainCondition Terrain;
        public ClimateState MinClimate;
        public ClimateState MaxClimate;

        public float CalcComfort(Cell cell)
        {
            return CalcComfort(cell.Climate, cell.TerrainType);
        }

        public float CalcComfort(ClimateState state, TerrainType terrainType)
        {
            if (Terrain == TerrainCondition.NotWater && terrainType == TerrainType.Water)
                return 0f;
            if (Terrain == TerrainCondition.OnlyHills && terrainType != TerrainType.Hills)
                return 0f;
            if (Terrain == TerrainCondition.OnlyMountains && terrainType != TerrainType.Mountains)
                return 0f;
            if (Terrain == TerrainCondition.OnlyPlains && terrainType != TerrainType.Plain)
                return 0f;
            if (Terrain == TerrainCondition.OnlyPlains && terrainType != TerrainType.Plain)
                return 0f;
            if (Terrain == TerrainCondition.OnlyWater && terrainType != TerrainType.Water)
                return 0f;
            if (Terrain == TerrainCondition.PlainsOrHills && (terrainType != TerrainType.Plain && terrainType != TerrainType.Hills))
                return 0f;

            var tempMod = (state.Temperature - MinClimate.Temperature)/(MaxClimate.Temperature - MinClimate.Temperature);
            return Mathf.Clamp01(1f - Mathf.Abs(tempMod - 0.5f));
        }

        public bool Match(Cell cell)
        {
            if (Terrain == TerrainCondition.NotWater && cell.TerrainType == TerrainType.Water)
                return false;
            if (Terrain == TerrainCondition.OnlyHills && cell.TerrainType != TerrainType.Hills)
                return false;
            if (Terrain == TerrainCondition.OnlyMountains && cell.TerrainType != TerrainType.Mountains)
                return false;
            if (Terrain == TerrainCondition.OnlyPlains && cell.TerrainType != TerrainType.Plain)
                return false;
            if (Terrain == TerrainCondition.OnlyPlains && cell.TerrainType != TerrainType.Plain)
                return false;
            if (Terrain == TerrainCondition.OnlyWater && cell.TerrainType != TerrainType.Water)
                return false;
            if (Terrain == TerrainCondition.PlainsOrHills && (cell.TerrainType != TerrainType.Plain && cell.TerrainType != TerrainType.Hills))
                return false;
            if (cell.Climate.Temperature < MinClimate.Temperature)
                return false;
            if (cell.Climate.Temperature > MaxClimate.Temperature)
                return false;

            return true;
        }
    }
}