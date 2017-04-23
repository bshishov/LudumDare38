using Assets.Scripts.Data;
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

        public int Width = 10;
        public int Height = 10;

        private float[] _tileHeightMap;
        private Color32[] _stateMap;
        private MeshFilter _meshFilter;

        void Start ()
        {
            _meshFilter = GetComponent<MeshFilter>();

            GameManager.Instance.ShowMessage("<color=yellow>Building world. Please wait...</color>");
            Generate();
            GameManager.Instance.ShowMessage("<color=yellow>World building complete</color>");
        }
        
        void Update ()
        {
		
        }

        private float GetHeight(float u, float v)
        {
            var pixel = HeightMap.GetPixelBilinear(u, v);
            return MinHeight + (MaxHeight - MinHeight) * HeighCurve.Evaluate(pixel.r + Random.value * 0.01f);
        }

        private void Generate()
        {
            if(HeightMap == null)
                return;

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
                    var u = (float)i / HeightMap.width + Random.value * 0.01f;
                    var v = (float)j / HeightMap.height + Random.value * 0.01f;

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
            _meshFilter.mesh = mesh;

            var meshCollider = GetComponent<MeshCollider>();
            if (meshCollider != null)
            {
                meshCollider.sharedMesh = mesh;
                meshCollider.convex = true;
            }


            // TILES
            _tileHeightMap = new float[Width * Height];
            var pixelsPerTileX = HeightMap.width / Width;
            var pixelsPerTileY = HeightMap.height / Height;

            for (var i = 0; i < Width; i++)
            {
                for (var j = 0; j < Height; j++)
                {
                    var sumHeight = 0f;

                    for (var k = 0; k < pixelsPerTileX; k++)
                    {
                        for (var l = 0; l < pixelsPerTileY; l++)
                        {
                            var u = (float)((i * pixelsPerTileX) + k) / HeightMap.width;
                            var v = (float)((j * pixelsPerTileX) + l) / HeightMap.height;
                            
                            sumHeight += GetHeight(u, v);
                        }
                    }

                    var averageHeight = sumHeight/(pixelsPerTileX*pixelsPerTileY);
                    _tileHeightMap[j*Width + i] = averageHeight;
                }
            }
        }

        public float GetHeightFromTile(int x, int y)
        {
            return _tileHeightMap[y*Width + x];
        }

        public void SetStateToTile(int x, int y, ClimateState state)
        {
            var color = new Color32
            {
                r = (byte) (int) (Mathf.Clamp(state.Temperature + 100f, 0, 255f)),
                g = (byte) (int) (Mathf.Clamp(state.Humidity, 0, 255f))
            };

            var pixelsPerTileX = HeightMap.width / Width;
            var pixelsPerTileY = HeightMap.height / Height;

            for (var k = 0; k < pixelsPerTileX; k++)
            {
                for (var l = 0; l < pixelsPerTileY; l++)
                {
                    var i = (x * pixelsPerTileX) + k;
                    var j = (y * pixelsPerTileX) + l;

                    _stateMap[i*HeightMap.height + j] = color;
                }
            }
        }

        public void UpdateStateMap()
        {
            _meshFilter.mesh.colors32 = _stateMap;
        }
    }
}
