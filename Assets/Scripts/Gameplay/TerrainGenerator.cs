using Assets.Scripts.Data;
using Assets.Scripts.Utils;
using UnityEngine;

namespace Assets.Scripts.Gameplay
{
    [RequireComponent(typeof(MeshFilter))]
    public class TerrainGenerator : MonoBehaviour
    {
        public Texture2D HeightMap;
        public AnimationCurve HeighCurve = AnimationCurve.Linear(0, 0, 1f, 1f);

        public float MinHeight = -1f;
        public float MaxHeight = 2f;

        public int Width { get { return GameManager.Instance.Width; } }
        public int Height { get { return GameManager.Instance.Height; } }

        private float[] _tileHeightMap;
        private Color32[] _stateMap;
        private MeshFilter _meshFilter;
        private Texture2D _stateTexture;
        private MeshRenderer _renderer;

        void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
        }

        void Start()
        {
            _stateTexture = new Texture2D(Width, Height, TextureFormat.RGFloat, false)
            {
                wrapMode = TextureWrapMode.Clamp
            };
            _renderer = GetComponent<MeshRenderer>();
            _renderer.material.SetTexture("_State", _stateTexture);
        }
        
        void Update ()
        {
        }

        private float GetHeight(float u, float v)
        {
            return GameManager.Instance.World.GetHeight(u, v);
        }

        public void Generate()
        {
            GameManager.Instance.ShowMessage("<color=yellow>Building world. Please wait...</color>");

            if (HeightMap == null)
            {
                Debug.LogWarning("Height texture is not set");
                return;
            }

            var w = HeightMap.width;
            var h = HeightMap.height;
            var verticies = new Vector3[HeightMap.width * HeightMap.height];
            var triangles = new int[(w) * (h) * 2 * 3];
            var uv = new Vector2[verticies.Length];
            _stateMap = new Color32[verticies.Length];
            var vertexIdx = 0;
            for (var i = 0; i < HeightMap.width; i++)
            {
                for (var j = 0; j < HeightMap.width; j++)
                {
                    var u = (float) i/(HeightMap.width - 1); //+ Random.value * 0.01f;
                    var v = (float) j/(HeightMap.height - 1); // + Random.value * 0.01f;

                    var height = GetHeight(u, v);


                    verticies[vertexIdx] = new Vector3((u - 0.5f) * Width, height, (v - 0.5f) * Height);
                    uv[vertexIdx] = new Vector2(u,v);

                    if (i < w - 1 && j < h - 1)
                    {
                        triangles[6*vertexIdx + 0] = vertexIdx;
                        triangles[6*vertexIdx + 1] = vertexIdx + 1;
                        triangles[6*vertexIdx + 2] = vertexIdx + w;

                        triangles[6*vertexIdx + 5] = vertexIdx + 1;
                        triangles[6*vertexIdx + 4] = vertexIdx + w;
                        triangles[6*vertexIdx + 3] = vertexIdx + w + 1;
                    }

                    vertexIdx++;
                }
            }


            
            var mesh = new Mesh 
            {
                name = "Generated From Height",
                vertices = verticies,
                triangles = triangles,
                uv = uv,
                colors32 = _stateMap
            };

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            _meshFilter.mesh = mesh;

            var meshCollider = GetComponent<MeshCollider>();
            if (meshCollider != null)
            {
                meshCollider.sharedMesh = mesh;
                meshCollider.convex = true;
            }


            // TILES
            _tileHeightMap = new float[Width * Height];
            var pixelsPerTileX = (float)HeightMap.width / Width;
            var pixelsPerTileY = (float)HeightMap.height / Height;

            for (var i = 0; i < Width; i++)
            {
                for (var j = 0; j < Height; j++)
                {
                    var sumHeight = 0f;
                    var count = 0;

                    for (var k = 0; k < pixelsPerTileX; k++)
                    {
                        for (var l = 0; l < pixelsPerTileY; l++)
                        {
                            var u = (i *  pixelsPerTileX + k) / HeightMap.width;
                            var v = (j * pixelsPerTileX + l) / HeightMap.height;
                            
                            sumHeight += GetHeight(u, v);
                            count++;
                        }
                    }

                    var averageHeight = sumHeight / count;
                    _tileHeightMap[j*Width + i] = averageHeight;
                }
            }
            
            GameManager.Instance.ShowMessage("<color=yellow>World building complete</color>");
        }

        public float GetHeightFromTile(int x, int y)
        {
            return _tileHeightMap[y*Width + x];
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

            // Old - vertex based
            var color = new Color32
            {
                r = (byte) (int) (Mathf.Clamp(state.Temperature + 100f, 0, 255f)),
                g = (byte) (int) (Mathf.Clamp(state.Humidity, 0, 255f))
            };

            var pixelsPerTileX = (float)HeightMap.width / Width;
            var pixelsPerTileY = (float)HeightMap.height / Height;

            for (var k = 0; k < pixelsPerTileX; k++)
            {
                for (var l = 0; l < pixelsPerTileY; l++)
                {
                    var i = (int)(x * pixelsPerTileX) + k;
                    var j = (int)(y * pixelsPerTileX) + l;

                    _stateMap[i*HeightMap.height + j] = color;
                }
            }
        }

        public void UpdateStateMap()
        {
            Debugger.Instance.Display("Map/State", _stateTexture);
            Debugger.Instance.Display("Map/Width", _stateTexture.width.ToString());
            Debugger.Instance.Display("Map/Height", _stateTexture.height.ToString());
            _stateTexture.Apply();
            _meshFilter.mesh.colors32 = _stateMap;
        }

        public void SelectCell(int x, int y)
        {
            _renderer.material.SetInt("_SelectedCell", y * Width + x);
        }
    }
}
