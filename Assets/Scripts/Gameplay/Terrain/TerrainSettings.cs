using System;
using com.heparo.terrain.toolkit;
using UnityEngine;

namespace Assets.Scripts.Gameplay.Terrain
{
    [Serializable]
    public class TerrainSettings
    {
        public Material Material;
        public TerrainData ExistingTerrain;

        [Header("From HeightMap")]
        public Vector3 TerrainSize = new Vector3(512f, 100f, 512f);
        public int Resolution = 512;
        public Texture2D HeightMap;

        [Header("Padding")]
        public float Padding = 20f;

        [Header("Smooth edges")]
        public bool SmoothEdges = true;
        public AnimationCurve SmoothEdgeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [Range(0f, 1f)]
        public float Edge = 0.1f;

        [Header("Post Processing")]
        public bool ApplyHeightCurve = true;
        public AnimationCurve HeightCurve = AnimationCurve.Linear(0, 0, 1, 1);
        public bool ApplyTilization = false;
        public float TilizationFactor = 0.5f;
        public int TilesX = 30;
        public int TilesY = 30;
        public bool ApplyThermalErosion = false;
        [SerializeField]
        public ThermalErosionSettings ThermalErosionSettings;


        [Header("Sea")]
        public bool GenerateSea = true;
        public float SeaLevel = 1f;
        public int SeaResolution = 64;
        public Material SeaMaterial;
    }
}