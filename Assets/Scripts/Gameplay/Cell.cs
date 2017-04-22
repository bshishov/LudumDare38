using System.Collections.Generic;
using Assets.Scripts.Data;
using UnityEngine;

namespace Assets.Scripts.Gameplay
{
    public class Cell : MonoBehaviour
    {
        public Species InitialTestSpecies;
        public ClimateState Climate;
        public readonly List<SpeciesState> SpeciesStates = new List<SpeciesState>();

        void Start ()
        {
            InitialTest();
        }

        void Update ()
        {
        }

        void InitialTest()
        {
            var state = new SpeciesState(InitialTestSpecies) {Count = 100};
            SpeciesStates.Add(state);
        }
    }
}
