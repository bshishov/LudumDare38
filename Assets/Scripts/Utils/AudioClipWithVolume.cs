using System;
using UnityEngine;

namespace Assets.Scripts.Utils
{
    [Serializable]
    public struct AudioClipWithVolume
    {
        public AudioClip Clip;

        [Range(0f, 1f)]
        public float VolumeModifier;
    }
}
