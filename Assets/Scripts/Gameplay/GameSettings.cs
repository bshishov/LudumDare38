using Assets.Scripts.Data;
using Assets.Scripts.Utils;
using UnityEngine;

namespace Assets.Scripts.Gameplay
{
    public class GameSettings : Singleton<GameSettings>
    {
        public override bool IsPersistent { get { return true; } }
        
        [Header("World")]
        public World World;
    }
}
