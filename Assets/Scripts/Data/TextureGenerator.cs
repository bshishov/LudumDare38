using System;
using UnityEngine;

namespace Assets.Scripts.Data
{
    [Serializable]
    [CreateAssetMenu(menuName = "Biology/Generators/Texure", fileName = "Texture Generator")]
    public class TextureGenerator : Generator
    {
        public Texture2D HeightMap;
        public override float GetHeight(float u, float v)
        {
            return HeightMap.GetPixelBilinear(u, v).r;
        }
    }
}