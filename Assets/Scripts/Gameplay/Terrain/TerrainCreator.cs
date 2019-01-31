using Assets.Scripts.Utils;
using com.heparo.terrain.toolkit;
using UnityEngine;

namespace Assets.Scripts.Gameplay.Terrain
{
    public static class TerrainCreator
    {
        public static void CreateTerrain(TerrainSettings settings, out GameObject terrainObj, out GameObject seaObj)
        {
            Debug.Log("Terrain creation started");
            var data = settings.ExistingTerrain;
            if (data == null)
            {
                Debug.LogFormat("Creating terrain from texture {0}", settings.HeightMap);
                Debug.LogFormat("Texture size: {0}x{1}", settings.HeightMap.width, settings.HeightMap.height);

                data = new TerrainData { heightmapResolution = settings.Resolution };


                var pixels = settings.HeightMap.GetPixels();
                var heights = new float[settings.HeightMap.height, settings.HeightMap.width];
                for (var i = 0; i < settings.HeightMap.height; i++)
                for (var j = 0; j < settings.HeightMap.width; j++)
                    heights[i, j] = pixels[i * settings.HeightMap.width + j].r;

                /*
                var heights = new float[data.heightmapHeight, data.heightmapWidth];
                for (var i = 0; i < data.heightmapHeight; i++)
                    for (var j = 0; j < data.heightmapWidth; j++)
                    {
                        var v = i / (data.heightmapHeight - 1f);
                        var u = j / (data.heightmapWidth - 1f);
                        heights[i, j] = HeightMap.GetPixelBilinear(u, v).r;
                    }
                    */

                data.SetHeights(0, 0, heights);
            }

            if (settings.ApplyHeightCurve)
            {
                Debug.Log("Applying height curve");
                ApplyHeightCurve(data, settings.HeightCurve);
            }

            

            // Terrain object
            terrainObj = UnityEngine.Terrain.CreateTerrainGameObject(data);
            var terrain = terrainObj.GetComponent<UnityEngine.Terrain>();
            terrain.materialType = UnityEngine.Terrain.MaterialType.Custom;
            terrain.materialTemplate = settings.Material;
            terrain.terrainData.size = settings.TerrainSize;

            if (settings.ApplyTilization)
                Tilize(terrain.terrainData, settings.TilesX, settings.TilesY, settings.TilizationFactor);

            if (settings.ApplyThermalErosion)
            {
                var toolkit = terrainObj.AddComponent<TerrainToolkit>();
                toolkit.FastThermalErosion(
                    settings.ThermalErosionSettings.Iterations,
                    settings.ThermalErosionSettings.MinSlope,
                    settings.ThermalErosionSettings.Blend);
            }

            if (settings.SmoothEdges)
            {
                Debug.Log("Smoothing edges");
                SmoothEdgesToGround(terrain.terrainData, settings.SmoothEdgeCurve, settings.Edge);
            }

            var size = terrain.terrainData.size;
            Debug.LogFormat("Terrain heightmap size: {0}x{1}", terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapHeight);
            Debug.LogFormat("Terrain actual size: x={0} y={1} z={2}", size.x, size.y, size.z);

            // Boundaries object
            Debug.LogFormat("Generating boundaries, padding={0}", settings.Padding);
            var boundsMesh = CreateBoundary(settings.Padding, size.x, size.z);
            var boundaryObj = new GameObject("Boundaries") { isStatic = true };
            var boundaryMeshFilter = boundaryObj.AddComponent<MeshFilter>();
            boundaryMeshFilter.sharedMesh = boundsMesh;

            var boundaryRenderer = boundaryObj.AddComponent<MeshRenderer>();
            boundaryRenderer.material = settings.Material;

            if (settings.GenerateSea)
            {
                Debug.Log("Generating sea");
                // Sea object
                seaObj = new GameObject("Sea") { isStatic = true };
                var seaMeshFilter = seaObj.AddComponent<MeshFilter>();
                seaMeshFilter.sharedMesh = Plane(Vector2.zero, Vector2.one, settings.SeaResolution, settings.SeaResolution, 0);

                var seaRenderer = seaObj.AddComponent<MeshRenderer>();
                seaRenderer.material = settings.SeaMaterial;

                seaObj.transform.position = new Vector3(-settings.Padding, settings.SeaLevel, -settings.Padding);
                seaObj.transform.localScale = new Vector3(2 * settings.Padding + size.x, 1f, 2 * settings.Padding + size.z);
            }
            else
            {
                seaObj = null;
            }
        }

        public static Mesh CreateBoundary(float padding, float terrainWidth, float terrainHeight, float y = 0f)
        {
            var fullWidth = terrainWidth + 2 * padding;
            var fullHeight = terrainHeight + 2 * padding;

            /* VERTICES
             
             0 --------- 1  
             |   4---5   |  \
             |   | T |   |  |  Terrain Height
             |   7---6   |  /
             3 --------- 2
                     
                 <---> Terrain Width             
             */


            var vertices = new Vector3[]
            {
                // Outer,
                // Clockwise, starting from top-left
                new Vector3(-padding, y, terrainHeight + padding),
                new Vector3(terrainWidth + padding, y, terrainHeight + padding),
                new Vector3(terrainWidth + padding, y, -padding),
                new Vector3(-padding, y, -padding),

                // Inner
                // Clockwise, starting from top-left
                new Vector3(0, y, terrainHeight),
                new Vector3(terrainWidth, y, terrainHeight),
                new Vector3(terrainWidth, y, 0),
                new Vector3(0, y, 0)
            };

            // Uv coordinates
            var uvs = new Vector2[]
            {
                // Outer,
                // Clockwise, starting from top-left
                new Vector2(-padding / terrainWidth, 1 + padding / terrainHeight),
                new Vector2(1 + padding / terrainWidth, 1 + padding / terrainHeight),
                new Vector2(1 + padding / terrainWidth, -padding / terrainHeight),
                new Vector2(-padding / terrainWidth, -padding / terrainHeight),

                // Inner
                // Clockwise, starting from top-left
                new Vector2(0, 1),
                new Vector2(1, 1),
                new Vector2(1, 0),
                new Vector2(0, 0),
            };

            // All normals facing up
            var normals = new Vector3[vertices.Length];
            for (var i = 0; i < vertices.Length; i++)
                normals[i] = Vector3.up;

            // Triangles, CW defined 
            var triangles = new int[]
            {
                0, 5, 4,
                0, 1, 5,
                1, 6, 5,
                1, 2, 6,
                2, 7, 6,
                2, 3, 7,
                3, 4, 7,
                3, 0, 4
            };

            //Bounds
            var bounds = new Bounds(new Vector3(terrainWidth * 0.5f, y, terrainHeight * 0.5f), new Vector3(fullWidth, 0, fullHeight));

            return new Mesh
            {
                name = "Boundary",
                vertices = vertices,
                uv = uvs,
                triangles = triangles,
                normals = normals,
                bounds = bounds
            };
        }

        public static void ApplyHeightCurve(TerrainData data, AnimationCurve curve)
        {
            const int chunkSize = 32;
            foreach (var chunk in new ChunkIterator(data.heightmapWidth, data.heightmapHeight, chunkSize, chunkSize))
            {
                var heights = data.GetHeights(chunk.Start.x, chunk.Start.y, chunk.Width, chunk.Height);
                for (var i = 0; i < chunk.Height; i++)
                for (var j = 0; j < chunk.Width; j++)
                    heights[i, j] = curve.Evaluate(heights[i, j]);

                data.SetHeights(chunk.Start.x, chunk.Start.y, heights);
            }
        }

        public static void Tilize(TerrainData data, int w, int h, float factor=0.5f)
        {
            var chunkSizeX = data.heightmapWidth / w;
            var chunkSizeY = data.heightmapWidth / h;
            var r4 = 0.5f * 0.5f * 0.5f * 0.5f;

            foreach (var chunk in new ChunkIterator(data.heightmapWidth, data.heightmapHeight, chunkSizeX, chunkSizeY))
            {
                var heights = data.GetHeights(chunk.Start.x, chunk.Start.y, chunk.Width, chunk.Height);
                var heightSum = 0f;
                for (var i = 0; i < chunk.Height; i++)
                for (var j = 0; j < chunk.Width; j++)
                {
                    heightSum += heights[i, j];
                }

                var avgHeight = heightSum / (chunk.Width * chunk.Height);

                for (var i = 0; i < chunk.Height; i++)
                for (var j = 0; j < chunk.Width; j++)
                {
                    var x = j / (chunk.Width - 1f) - 0.5f;
                    var y = i / (chunk.Height - 1f) - 0.5f;

                    var squircleMask = Mathf.Clamp01(1 - (x * x * x * x / r4 + y * y * y * y / r4));

                    heights[i, j] = Mathf.Lerp(heights[i, j], avgHeight, Mathf.Clamp01(squircleMask * factor));
                }

                data.SetHeights(chunk.Start.x, chunk.Start.y, heights);
            }
        }

        public static void SmoothEdgesToGround(TerrainData data, AnimationCurve curve, float edge = 0.1f)
        {
            var w = data.heightmapWidth;
            var h = data.heightmapHeight;
            var wf = Mathf.CeilToInt(w * edge);
            var hf = Mathf.CeilToInt(h * edge);

            // Corners
            SmoothEdgePart(data, curve, 0, h - hf, wf, hf, 0, 1, 0, 0);         // Top Left corner
            SmoothEdgePart(data, curve, w - wf, h - hf, wf, hf, 1, 0, 0, 0);    // Top right corner
            SmoothEdgePart(data, curve, 0, 0, wf, hf, 0, 0, 0, 1);              // Bottom left corner
            SmoothEdgePart(data, curve, w - wf, 0, wf, hf, 0, 0, 1, 0);         // Bottom right corner

            // Edges    
            SmoothEdgePart(data, curve, 0, hf, wf, h - 2 * hf, 0, 1, 0, 1);       // Left edge
            SmoothEdgePart(data, curve, w - wf, hf, wf, h - 2 * hf, 1, 0, 1, 0);  // Right edge
            SmoothEdgePart(data, curve, wf, h - hf, w - 2 * wf, hf, 1, 1, 0, 0);  // Top edge
            SmoothEdgePart(data, curve, wf, 0, w - 2 * wf, hf, 0, 0, 1, 1);       // Bottom edge
        }

        public static void SmoothEdgePart(TerrainData data, AnimationCurve curve, int x1, int y1, int w, int h, float bottomLeft, float bottomRight, float topLeft, float topRight)
        {
            var heights = data.GetHeights(x1, y1, w, h);
            for (var i = 0; i < h; i++)
            {
                for (var j = 0; j < w; j++)
                {
                    // Biliniear interpolation
                    var a = Mathf.Lerp(bottomLeft, bottomRight, (float)j / w);
                    var b = Mathf.Lerp(topLeft, topRight, (float)j / w);
                    var f = Mathf.Lerp(a, b, (float)i / h);

                    heights[i, j] = curve.Evaluate(f) * heights[i, j];
                }
            }
            data.SetHeights(x1, y1, heights);
        }

        public static Mesh Plane(Vector2 uvStart, Vector2 uvEnd, int heightSubdiv = 1, int widthSubdiv = 1, float height = 0f)
        {
            var w = widthSubdiv + 1;
            var h = heightSubdiv + 1;

            var vertices = new Vector3[w * h];
            var triangles = new int[(w - 1) * (h - 1) * 2 * 3];
            var uvs = new Vector2[vertices.Length];
            var normals = new Vector3[w * h];

            var vi = 0;
            var tr = 0;
            for (var i = 0; i < h; i++)
            {
                var y = (float)i / (h - 1);

                for (var j = 0; j < w; j++)
                {
                    var x = (float)j / (w - 1);
                    var uv = new Vector2(
                        Mathf.Lerp(uvStart.x, uvEnd.x, x),
                        Mathf.Lerp(uvStart.y, uvEnd.y, y));

                    normals[vi] = Vector3.up;
                    vertices[vi] = new Vector3(x, height, y);
                    uvs[vi] = uv;

                    if (i < h - 1 && j < w - 1)
                    {
                        triangles[6 * tr + 0] = vi;
                        triangles[6 * tr + 2] = vi + 1;
                        triangles[6 * tr + 1] = vi + w;

                        triangles[6 * tr + 5] = vi + 1;
                        triangles[6 * tr + 3] = vi + w;
                        triangles[6 * tr + 4] = vi + w + 1;
                        tr++;
                    }

                    vi++;
                }
            }

            return new Mesh
            {
                name = "Plane",
                vertices = vertices,
                triangles = triangles,
                uv = uvs,
                normals = normals,
                bounds = new Bounds(new Vector3(0.5f, height, 0.5f), new Vector3(1f, 0.1f, 1f))
            };
        }
    }
}