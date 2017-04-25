using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Data
{
    [CreateAssetMenu(menuName = "Biology/Objective", fileName = "Objective")]
    public class Objective : ScriptableObject
    {
        public string Name;
        public string Description;
        public Sprite Icon;
    }
}
