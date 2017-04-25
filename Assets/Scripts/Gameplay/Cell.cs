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
        public Species InitialTestSpecies;
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
        private readonly List<BuffState> _currentBuffs = new List<BuffState>();



        void Start ()
        {
            _shaker = GetComponent<Shaker>();

            if (Tracker != null)
            {
                Tracker.NewSpecies += TrackerOnNewSpecies;
                Tracker.Extincted += TrackerOnExtincted;
            }
        }

        void Update ()
        {
        }

        public void InitialSetup(float height)
        {
            Height = height;
            _terrain = GameManager.Instance.Terrains.GetForHeight(height);
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
        }

        public void ProcessStep()
        {
            // BUFFS PROCESSING
            foreach (var currentBuff in _currentBuffs)
            {
                currentBuff.ProcessStep();
            }

            // Clear all buffs
            _currentBuffs.RemoveAll(b => !b.IsActive);


            // SPECIES PROCESSING
            foreach (var speciesState in SpeciesStates.Values.ToList())
            {
                speciesState.ProcessStep(this);
                if (speciesState.Count < 1f)
                {
                    SpeciesStates.Remove(speciesState.Species);
                    Tracker.SpeciesDied(speciesState.Species);

                    if(_isSelected)
                        BuildUI();
                }
            }
            
            Tracker.Step();
        }

        public void OnSelect()
        {
            BuildUI();
            Shake();
            _isSelected = true;
        }

        public void OnUnSelect()
        {
            _isSelected = false;
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

        public IEnumerable<Cell> EnumeratNeighbours()
        {
            if(X > 1)
                yield return GameManager.Instance.Cells[X - 1, Y];

            if (X < GameManager.Width - 1)
                yield return GameManager.Instance.Cells[X + 1, Y];

            if (Y > 1)
                yield return GameManager.Instance.Cells[X, Y - 1];

            if (Y < GameManager.Height - 1)
                yield return GameManager.Instance.Cells[X, Y + 1];
            yield break;
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
                SpeciesStates[species].ChangeCount(count);
            }
            else
            {
                var state = new SpeciesState(species);
                state.ChangeCount(count);
                SpeciesStates.Add(species, state);

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
                    var container = transform.FindChild(currentAppearance.name);
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

                var container = transform.FindChild(appearance.name);
                if (container == null)
                {
                    container = new GameObject(appearance.name).transform;
                    container.SetParent(transform, false);
                    container.position += new Vector3(0, 0.5f, 0);
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
            if (_currentBuffs.Any(b => b.Buff.Equals(buff)))
            {
                return;
            }

            var buffState = new BuffState(buff, this);
            _currentBuffs.Add(buffState);
        }

        public Vector3 GetPlacementCorrecrion()
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
