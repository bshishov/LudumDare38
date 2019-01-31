using System;
using System.Collections.Generic;
using System.Linq;
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
        public float Height;
        public ClimateState Climate;
        public GameObject SpeciesUIPrefab;
        public readonly Dictionary<Species, SpeciesState> SpeciesStates = new Dictionary<Species, SpeciesState>();
        public TerrainType TerrainType = TerrainType.Plain;
        public List<PropsAppearance> ActiveAppearances = new List<PropsAppearance>();

        [Header("Effects")]
        public GameObject NewSpeciesEffectPrefab;
        public GameObject ExctinctedEffectPrefab;
        public readonly SpeciesStatsTracker Tracker = new SpeciesStatsTracker();

        private Text _temperatureText;
        private Text _humidityText;

        private Shaker _shaker;
        private bool _isSelected;
        private TerrainInfo _terrain;
        private readonly List<BuffState> _activeBuffs = new List<BuffState>();

        private Chart _populationChart;
        private Chart _climateChart;
        private TrackSeries _temperatureSeries;
        private TrackSeries _humiditySeries;


        void Start ()
        {
            _shaker = GetComponent<Shaker>();

            if (Tracker != null)
            {
                Tracker.NewSpecies += TrackerOnNewSpecies;
                Tracker.Extincted += TrackerOnExtincted;
            }

            _temperatureSeries = new TrackSeries(100, "Temperature (F)", Color.red);
            _humiditySeries = new TrackSeries(100, "Humidity (%)", Color.blue);
        }

        public void InitialSetup(float height)
        {
            Height = height;
            _terrain = GameManager.Instance.World.TerrainTypes.GetForHeight(height);
            TerrainType = _terrain.TerrainType;
        }

        void BuildUI()
        {
            if (_terrain != null)
            {
                var terrainIconObj = GameObject.Find("Canvas/Sidebar/TerrainIcon");
                if (terrainIconObj != null)
                {
                    var img = terrainIconObj.GetComponent<Image>();
                    if (img != null && _terrain.Icon != null)
                        img.sprite = _terrain.Icon;
                }

                var terrainNameObj = GameObject.Find("Canvas/Sidebar/TerrainName");
                if (terrainNameObj != null)
                {
                    var txt = terrainNameObj.GetComponent<Text>();
                    if (txt != null)
                        txt.text = _terrain.Name;
                }
            }

            var temperature = GameObject.Find("Canvas/Sidebar/Info/Temperature");
            if (temperature != null)
                _temperatureText = temperature.GetComponent<Text>();

            var humidity = GameObject.Find("Canvas/Sidebar/Info/Humidity");
            if (humidity != null)
                _humidityText = humidity.GetComponent<Text>();

            var speciesPanel = GameObject.Find("Canvas/Sidebar/Species");
            
            foreach (Transform child in speciesPanel.transform)
                Destroy(child.gameObject);

            var states = SpeciesStates.Values.ToList();
            states.Sort((a, b) => b.Species.Size.CompareTo(a.Species.Size));
            foreach (var speciesState in states)
            {
                var statPanel = (GameObject) Instantiate(SpeciesUIPrefab, Vector3.zero, Quaternion.identity, speciesPanel.transform);
                speciesState.FillUIInPanel(statPanel);
            }

            UpdateUI();
        }

        public void ProcessStep(float dt)
        {
            // BUFFS PROCESSING
            foreach (var currentBuff in _activeBuffs)
                currentBuff.ProcessStep(dt);

            // Clear all inactive buffs
            _activeBuffs.RemoveAll(b => !b.IsActive);


            // SPECIES PROCESSING
            foreach (var speciesState in SpeciesStates.Values.ToList())
            {
                speciesState.ProcessStep(this);
                if (speciesState.Population < 1f)
                {
                    SpeciesStates.Remove(speciesState.Species);
                    Tracker.SpeciesDied(speciesState.Species);

                    if(_populationChart != null)
                        _populationChart.RemoveSeries(speciesState.Series);

                    if(_isSelected)
                        BuildUI();
                }
            }
            
            Tracker.Step();
            _temperatureSeries.AddPoint(GameManager.Instance.Step, Climate.Temperature);
            _humiditySeries.AddPoint(GameManager.Instance.Step, Climate.Humidity);

            if (_isSelected)
            {
                Debugger.Instance.Display("ActiveCell/Height", Height);
                Debugger.Instance.Display("ActiveCell/Temperature C", Climate.TemperatureAsCelsius());
                Debugger.Instance.Display("ActiveCell/Temperature F", Climate.Temperature);
                Debugger.Instance.Display("ActiveCell/Humidity", Climate.Humidity);
                Debugger.Instance.Display("ActiveCell/X", X);
                Debugger.Instance.Display("ActiveCell/Y", Y);

                foreach (var speciesState in SpeciesStates)
                {
                    var key = string.Format("ActiveCell/Species/{0}", speciesState.Key.Name);
                    Debugger.Instance.Display(key, speciesState.Value.GetVerboseCount());
                }
            }
        }

        public void OnSelect()
        {
            BuildUI();
            Shake();
            _isSelected = true;


            _climateChart = GameObject.Find("Canvas/ClimateChart").GetComponent<Chart>();
            if (_climateChart != null)
            {
                _climateChart.XAxisName = "Step";
                _climateChart.AddSeries(_temperatureSeries);
                _climateChart.AddSeries(_humiditySeries);
            }

            _populationChart = GameObject.Find("Canvas/PopulationChart").GetComponent<Chart>();
            if (_populationChart != null)
            {
                _populationChart.XAxisName = "Step";
                _populationChart.YAxisName = "Population, log(n)";

                foreach (var speciesState in SpeciesStates)
                {
                    _populationChart.AddSeries(speciesState.Value.Series);
                }
            }
        }

        public void OnUnSelect()
        {
            _isSelected = false;

            if (_populationChart != null)
            {
                _populationChart.ClearAll();
                _populationChart = null;
            }

            if (_climateChart != null)
            {
                _climateChart.ClearAll();
                _climateChart = null;
            }
        }

        public void Shake()
        {
            if (_shaker != null)
                _shaker.Shake(0.2f);
        }

        public void UpdateUI()
        {
            if(_temperatureText != null)
                _temperatureText.text = String.Format("°F {0:##0.#}\n°C {1:##0.#}", Climate.Temperature, Climate.TemperatureAsCelsius());
            //_temperatureText.text = String.Format("{0:##0.#}°C", Climate.TemperatureAsCelsius());

            if (_humidityText != null)
                _humidityText.text = String.Format("{0:##0.#}%", Climate.Humidity);
           
            foreach (var speciesState in SpeciesStates.Values)
            {
                speciesState.UpdateUI();
            }
        }

        public IEnumerable<Cell> EnumerateNeighbors()
        {
            if(X > 1)
                yield return GameManager.Instance.GetCellAt(X - 1, Y);

            if (X < GameManager.Instance.Width - 1)
                yield return GameManager.Instance.GetCellAt(X + 1, Y);

            if (Y > 1)
                yield return GameManager.Instance.GetCellAt(X, Y - 1);

            if (Y < GameManager.Instance.Height - 1)
                yield return GameManager.Instance.GetCellAt(X, Y + 1);
        }

        public Cell GetRandomNeighbor()
        {
            var x = Mathf.Round(UnityEngine.Random.Range(X - 1.8f, X + 1f));
            var y = Mathf.Round(UnityEngine.Random.Range(Y - 1.8f, Y + 1f));
            x = Mathf.Clamp(x, 0, GameManager.Instance.Width - 1f);
            y = Mathf.Clamp(y, 0, GameManager.Instance.Height - 1f);
            return GameManager.Instance.GetCellAt((int)x, (int)y);
        }

        public void AddSpecies(Species species, float amount, float averageAge = 0)
        {
            if (SpeciesStates.ContainsKey(species))
            {
                SpeciesStates[species].IncreasePopulation(amount, averageAge);
            }
            else
            {
                var state = new SpeciesState(species);
                state.IncreasePopulation(amount, averageAge);
                SpeciesStates.Add(species, state);

                if (_populationChart != null)
                    _populationChart.AddSeries(state.Series);

                if (Tracker != null)
                    Tracker.SpeciesBorn(species);
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

        public bool HasGroup(Group speciesGroup)
        {
            return SpeciesStates.Any(kvp => kvp.Key.IsInGroup(speciesGroup));
        }

        private void TrackerOnNewSpecies(Species species)
        {
            if(_isSelected)
                BuildUI();
            
            Shake();
            SpawnEffect(NewSpeciesEffectPrefab, 2f);
            GameManager.Instance.NewSpeciesOnCell(species, this);
        }

        private void TrackerOnExtincted(Species species)
        {
            if (_isSelected)
                BuildUI();

            SpawnEffect(ExctinctedEffectPrefab, 2f);
            GameManager.Instance.ExtinctedSpeciesOnCell(species, this);
        }

        private void SpawnEffect(GameObject prefab, float duration)
        {
            if(prefab == null)
                return;

            var go = (GameObject) Instantiate(prefab, transform, false);
            go.transform.position += new Vector3(0,1,0);
            Destroy(go, duration);
        }

        public void UpdateAppearance(List<PropsAppearance> validAppearances)
        {
            for (var index = 0; index < ActiveAppearances.Count; index++)
            {
                var currentAppearance = ActiveAppearances[index];
                if (!validAppearances.Contains(currentAppearance))
                {
                    var container = transform.Find(currentAppearance.name);
                    if (container != null)
                        Destroy(container.gameObject);

                    ActiveAppearances.Remove(currentAppearance);
                }
            }

            foreach (var appearance in validAppearances)
            {
                // If it is already applied
                if (ActiveAppearances.Contains(appearance))
                    continue;

                var container = transform.Find(appearance.name);
                if (container == null)
                {
                    container = new GameObject(appearance.name).transform;
                    container.SetParent(transform, false);
                }
                BuildAppearance(container, appearance);
                ActiveAppearances.Add(appearance);
            }
        }

        private void BuildAppearance(Transform container, PropsAppearance appearance)
        {
            var count = (int) UnityEngine.Random.Range(appearance.CountMin, appearance.CountMax);

            for (var i = 0; i < count; i++)
            {
                var rndPos = new Vector3(appearance.PositionSpread*UnityEngine.Random.value - 0.5f, 0,
                    appearance.PositionSpread*UnityEngine.Random.value - 0.5f);
                var pos = appearance.BasePosition + rndPos;
                var actualObject = (GameObject)Instantiate(appearance.Prefab, container, false);
                actualObject.transform.localPosition = pos;
                actualObject.transform.localScale *= appearance.Scale + UnityEngine.Random.value * appearance.ScaleSpread;
                actualObject.transform.Rotate(Vector3.up, appearance.Rotation + UnityEngine.Random.value * appearance.RotationSpread);
            }
        }

        public void ApplyBuff(Buff buff)
        {
            if (buff == null)
                return;

            // If this buff is already applied
            if (_activeBuffs.Any(b => b.Buff.Equals(buff)))
                return;
            
            _activeBuffs.Add(new BuffState(buff, this));
        }

        public Vector3 GetPlacementCorrection()
        {
            if (TerrainType == TerrainType.Water)
            {
                // Return dist to sea level
                return new Vector3(0, -transform.position.y, 0);
            }

            // TODO: estimate normal or raycast the terrain
            return Vector3.zero;
        }
    }
}
