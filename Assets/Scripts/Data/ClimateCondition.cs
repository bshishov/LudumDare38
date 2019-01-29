using System;
#if UNITY_EDITOR
using Assets.Scripts.EditorExt;
#endif
using Assets.Scripts.Gameplay;
using Assets.Scripts.Utils;
using UnityEngine;

namespace Assets.Scripts.Data
{
    [Serializable]
    public struct ClimateCondition
    {
        public enum ConditionType
        {
            Soft,
            Strict,
            DoubleSigmoid10,
            DoubleSigmoid20,
            DoubleSigmoid50,
            DoubleSigmoid100
        }

        [SerializeField]

#if UNITY_EDITOR
        [EnumFlags]
#endif
        TerrainType Terrain;
        public ClimateState MinClimate;
        public ClimateState MaxClimate;
        public ConditionType MatchType;
        

        public float CalcComfort(Cell cell)
        {
            return CalcComfort(cell.Climate, cell.TerrainType);
        }

        public float CalcComfort(ClimateState state, TerrainType terrainType)
        {
            if ((Terrain & terrainType) == 0)
                return 0f;

            if (MatchType == ConditionType.Soft)
            {
                return Statistics.SoftRange(state.Temperature, MinClimate.Temperature, MaxClimate.Temperature) *
                       Statistics.SoftRange(state.Humidity, MinClimate.Humidity, MaxClimate.Humidity);
            }

            if (MatchType == ConditionType.DoubleSigmoid10)
            {
                return Statistics.DoubleSigmoidRange(state.Temperature, MinClimate.Temperature, MaxClimate.Temperature, 10f) *
                       Statistics.DoubleSigmoidRange(state.Humidity, MinClimate.Humidity, MaxClimate.Humidity, 10f);
            }

            if (MatchType == ConditionType.DoubleSigmoid20)
            {
                return Statistics.DoubleSigmoidRange(state.Temperature, MinClimate.Temperature, MaxClimate.Temperature, 20f) *
                       Statistics.DoubleSigmoidRange(state.Humidity, MinClimate.Humidity, MaxClimate.Humidity, 20f);
            }

            if (MatchType == ConditionType.DoubleSigmoid50)
            {
                return Statistics.DoubleSigmoidRange(state.Temperature, MinClimate.Temperature, MaxClimate.Temperature, 50f) *
                       Statistics.DoubleSigmoidRange(state.Humidity, MinClimate.Humidity, MaxClimate.Humidity, 50f);
            }

            if (MatchType == ConditionType.DoubleSigmoid100)
            {
                return Statistics.DoubleSigmoidRange(state.Temperature, MinClimate.Temperature, MaxClimate.Temperature, 100f) *
                       Statistics.DoubleSigmoidRange(state.Humidity, MinClimate.Humidity, MaxClimate.Humidity, 100f);
            }

            // MatchType == ConditionType.Strict
            if (state.Temperature < MinClimate.Temperature)
                return 0f;
            if (state.Temperature > MaxClimate.Temperature)
                return 0f;
            if (state.Humidity < MinClimate.Humidity)
                return 0f;
            if (state.Humidity > MaxClimate.Humidity)
                return 0f;

            return 1f;
        }

        public bool Match(Cell cell)
        {
            if ((Terrain & cell.TerrainType) == 0)
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