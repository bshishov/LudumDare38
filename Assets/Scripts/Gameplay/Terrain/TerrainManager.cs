using System;
using Assets.Scripts.Data;
using Assets.Scripts.Utils;
using UnityEngine;

namespace Assets.Scripts.Gameplay.Terrain
{
    public class TerrainManager : MonoBehaviour
    {
        public bool IsCreated { get; private set; }
        public event Action TerrainCreated;
        public Vector3 Center
        {
            get { return _terrainSize * 0.5f; }
        }

        public Vector3 Size
        {
            get { return _terrainSize; }
        }

        public float SeaLevel
        {
            get { return _seaLevel; }
        }

        private UnityEngine.Terrain _terrain;
        private GameObject _seaObj;
        private Debugger.Logger _logger;
        private Texture2D _stateTexture;
        private int _width;
        private int _height;
        private Vector2 _cellSize;
        private Vector3 _terrainSize;
        private float _seaLevel;

        void Start()
        {
            _logger = Debugger.Instance.GetLogger("Terrain/Log");
        }
    
        public void CreateTerrain(TerrainSettings settings, int width, int height)
        {
            if(IsCreated)
                return;

            _logger.LogFormat("Creating terrain from settings");
            GameObject terrainObj;
            TerrainCreator.CreateTerrain(settings, out terrainObj, out _seaObj);
            _terrain = terrainObj.GetComponent<UnityEngine.Terrain>();
            _seaLevel = settings.SeaLevel;
            _width = width;
            _height = height;
            _terrainSize = _terrain.terrainData.size;
            _cellSize = new Vector2(_terrainSize.x / _width, _terrainSize.z / _height);
            
            Debugger.Instance.Display("Terrain/CellSize", _cellSize.ToString());
            Debugger.Instance.Display("Terrain/Size", _terrainSize.ToString());

            IsCreated = true;

            _stateTexture = new Texture2D(width, height, TextureFormat.RGFloat, false)
            {
                wrapMode = TextureWrapMode.Clamp
            };
            _terrain.materialTemplate.SetTexture("_State", _stateTexture);

            Debug.Log("Terrain created");
            if (TerrainCreated != null)
                TerrainCreated();
        }

        public void SetStateToTile(int x, int y, ClimateState state)
        {
            // [IMPORTANT] keep the settings with the corresponding shader values
            var c = new Color
            {
                r = Mathf.Clamp01(state.TemperatureAsCelsius() / 200f + 0.5f),
                g = Mathf.Clamp01(state.Humidity / 100f)
            };
            _stateTexture.SetPixel(x, y, c);
        }

        public void UpdateStateMap()
        {
            Debugger.Instance.Display("Terrain/State", _stateTexture);
            Debugger.Instance.Display("Terrain/State/Width", _stateTexture.width.ToString());
            Debugger.Instance.Display("Terrain/State/Height", _stateTexture.height.ToString());
            _stateTexture.Apply();
        }

        public void SelectCell(int x, int y)
        {
            Shader.SetGlobalVector("_Selection", new Vector4(
                _cellSize.x * (x + 0.5f), 
                _cellSize.y * (y + 0.5f), 
                0, _cellSize.x));
        }

        public Vector2Int CellCoordsFromUV(Vector2 uv)
        {
            var x = Mathf.FloorToInt(uv.x * _width);
            var y = Mathf.FloorToInt(uv.y * _height);
            return new Vector2Int(x, y);
        }

        public Vector2Int CellCoordsFromWorld(Vector3 world)
        {
            var tSize = _terrain.terrainData.size;
            var x = Mathf.FloorToInt((world.x / tSize.x) * _width);
            var y = Mathf.FloorToInt((world.z / tSize.z) * _height);
            return new Vector2Int(x, y);
        }

        public Vector3 CellToWorldOnTerrain(int x, int y)
        {
            var c = new Vector3(
                _cellSize.x * (x + 0.5f),
                _terrainSize.y * 0.5f,
                _cellSize.y * (y + 0.5f));
            var height = _terrain.SampleHeight(c) - _seaLevel;
            return new Vector3(c.x, height, c.z);
        }
    }
}
