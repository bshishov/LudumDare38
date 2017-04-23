using System;
using System.Collections.Generic;
using Assets.Scripts.Data;
using Assets.Scripts.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Gameplay
{
    public class Cell : MonoBehaviour
    {
        public int X;
        public int Y;
        public Species InitialTestSpecies;
        public ClimateState Climate;
        public GameObject SpeciesUIPrefab;
        public readonly Dictionary<Species, SpeciesState> SpeciesStates = new Dictionary<Species, SpeciesState>();
        public TerrainType TerrainType = TerrainType.Plain;
        public TerrainAppearance.TreeAppearance LastTreeAppearance;

        private Text _temperatureText;
        private Text _humidityText;

        private Shaker _shaker;

        void Start ()
        {
            _shaker = GetComponent<Shaker>();
        }

        void Update ()
        {
        }

        public void InitialTest()
        {
            /*
            if(UnityEngine.Random.value < 0.1)
                TerrainType = TerrainType.Mountains;

            if (UnityEngine.Random.value < 0.1)
                TerrainType = TerrainType.Water;
                */

            if (X == 0 && Y == 0)
            {
                var state = new SpeciesState(InitialTestSpecies) {Count = 100};
                SpeciesStates.Add(state.Species, state);
                Climate.Temperature = 10.0f;
                Climate.Humidity = 10.0f;
            }
        }

        void BuildUI()
        {
            var temperature = GameObject.Find("Canvas/Sidebar/Info/Temperature");
            if (temperature != null)
                _temperatureText = temperature.GetComponent<Text>();

            var humidity = GameObject.Find("Canvas/Sidebar/Info/Humidity");
            if (humidity != null)
                _humidityText = humidity.GetComponent<Text>();

            var speciesPanel = GameObject.Find("Canvas/Sidebar/Species");
            
            foreach (Transform child in speciesPanel.transform)
                Destroy(child.gameObject);

            foreach (var speciesState in SpeciesStates.Values)
            {
                var statPanel = (GameObject) Instantiate(SpeciesUIPrefab, Vector3.zero, Quaternion.identity, speciesPanel.transform);
                speciesState.FillUIInPanel(statPanel);
            }
        }

        public void OnClick()
        {
            BuildUI();
            Shake();
        }

        public void Shake()
        {
            if (_shaker != null)
                _shaker.Shake(0.2f);
        }

        public void UpdateUI()
        {
            
            if(_temperatureText != null)
                _temperatureText.text = String.Format("{0:##0.#}K", Climate.Temperature);
            //_temperatureText.text = String.Format("{0:##0.#}°C", Climate.TemperatureAsCelsius());

            if (_humidityText != null)
                _humidityText.text = String.Format("{0:##0.#}%", Climate.Humidity);

            foreach (var speciesState in SpeciesStates.Values)
            {
                speciesState.UpdateUI();
            }
        }

        public Cell GetRandomNeighbour()
        {
            var x = Mathf.Round(UnityEngine.Random.Range(X - 1.8f, X + 1f));
            var y = Mathf.Round(UnityEngine.Random.Range(Y - 1.8f, Y + 1f));
            x = Mathf.Clamp(x, 0, GameManager.Width - 1f);
            y = Mathf.Clamp(y, 0, GameManager.Height - 1f);
            return GameManager.Instance.Cells[(int)x, (int)y];
        }

        public void AddSpecies(Species species, float count)
        {
            if (SpeciesStates.ContainsKey(species))
            {
                SpeciesStates[species].Count += count;
            }
            else
            {
                SpeciesStates.Add(species, new SpeciesState(species) {Count = count});
                //BuildUI();
                Shake();
            }
        }

        public List<SpeciesState> GetFromGroup(Group group)
        {
            var states = new List<SpeciesState>();
            foreach (var kvp in SpeciesStates)
            {
                if (kvp.Key.IsInGroup(group))
                {
                    if(!states.Contains(kvp.Value))
                        states.Add(kvp.Value);
                }
            }
            return states;
        }
    }
}
