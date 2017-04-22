using System.Collections.Generic;
using Assets.Scripts.Data;
using UnityEngine;

namespace Assets.Scripts.Gameplay
{
    public class Cell : MonoBehaviour
    {
        public ClimateState Climate;
        public readonly List<SpeciesState> SpeciesStates = new List<SpeciesState>();

        void Start ()
        {
        }

        void Update ()
        {
        }
    }
}
