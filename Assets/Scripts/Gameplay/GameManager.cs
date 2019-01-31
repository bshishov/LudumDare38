using System;
using System.Collections.Generic;
using Assets.Scripts.Data;
using Assets.Scripts.Gameplay.Terrain;
using Assets.Scripts.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts.Gameplay
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(TerrainGenerator))]
    [RequireComponent(typeof(Caster))]
    public class GameManager : Singleton<GameManager>
    {
        // Not persistent singleton
        public override bool IsPersistent { get { return false; } }

        public int Width
        {
            get { return World.Width; }
        }

        public int Height
        {
            get { return World.Height; }
        }

        public const float UpdateInterval = 0.3f;
        public const int CellsPerUpdate = 20;

        public World World { get; private set; }
        public GameObject CellPrefab;

        [Range(0f, 10f)]
        public float TimeScale = 1f;

        [Header("Visuals")]
        public GameObject MessagePrefab;
        public GameObject PointerPlus;
        public GameObject PointerCross;

        [Header("Sound")]
        public AudioClipWithVolume NewSpecies;
        public AudioClipWithVolume SpeciesExtinct;
        public AudioClipWithVolume Click;

        [Header("Terrain")]
        public TerrainManager Terrain;

        public float Step { get { return _step; } }
        public readonly SpeciesStatsTracker Tracker = new SpeciesStatsTracker { StepToForget = 20f };

        private Cell[,] _cells;
        private Cell _selected;
        private float _step;
        private float _updateTime = 0f;
        private float _deltaTime = 0f;

        private int _lastIndex = 0;
        private GameObject _messagesPanel;
        private Cell _lastEventCell;
        private PropsAppearance[] _appearances;
        private Caster _caster;
        private AudioSource _audio;
        private Debugger.Logger _logger;
        private Debugger.Logger _speciesLogger;

        void Start ()
        {
            // Logging
            _logger = Debugger.Instance.GetLogger("Game/Log");
            _speciesLogger = Debugger.Instance.GetLogger("Species/Log");

            // Load world from game settings
            World = GameSettings.Instance.World;
            _cells = new Cell[Width, Height];
            _logger.LogFormat("Loading world {0}", World.Name);

            _audio = GetComponent<AudioSource>();
            BuildWorld();

            ShowMessage("Loading resources");
            _appearances = Resources.LoadAll<PropsAppearance>("Appearance");
            _logger.LogFormat("Loaded {0} appearances", _appearances.Length);

            if (Tracker != null)
            {
                Tracker.NewSpecies += TrackerOnNewSpecies;
                //Tracker.Extincted += TrackerOnExtincted;
            }

            _caster = GetComponent<Caster>();
            _caster.OnSpellCasted += CasterOnOnSpellCasted;


            Debugger.Instance.Display("ActiveCell/Spawn", new Vector2(200, 80), CheatSpawnUI);

        }

        private string _cheatSpawnName;
        private string _cheatSpawnAmount;

        private void CheatSpawnUI(Rect rect)
        {
            GUILayout.BeginArea(rect);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Name");
            _cheatSpawnName = GUILayout.TextField(_cheatSpawnName);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Amount");
            _cheatSpawnAmount = GUILayout.TextField(_cheatSpawnAmount);
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Spawn"))
            {
                long amount = 0;
                if (long.TryParse(_cheatSpawnAmount, out amount))
                {
                    var specie = Resources.Load<Species>("Species/" + _cheatSpawnName);
                    if(specie != null)
                        GameManager.Instance.SpawnInSelectedCell(specie, amount);
                }
            }
            GUILayout.EndArea();
        }

        private void CasterOnOnSpellCasted(Spell spell, Cell cell)
        {
            if (spell.Name == "Meteor")
            {
                _audio.PlayDelayed(2f);
            }
        }

        void Update ()
        {
            if (Input.GetMouseButtonDown(0) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(-1))
            {
                if (!_caster.SelectionIsActive)
                {
                    RaycastHit hit;
                    var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                    if (Physics.Raycast(ray, out hit))
                    {
                        PlayAudio(Click);
                        if (hit.collider is TerrainCollider)
                        {
                            Debug.DrawRay(hit.point, hit.normal * 3f, Color.red, 5f);
                            //var coords = Terrain.CellCoordsFromUV(hit.textureCoord);
                            var coords = Terrain.CellCoordsFromWorld(hit.point);
                            var cell = _cells[coords.x, coords.y];
                            if (cell != null)
                                OnCellClick(cell);
                        }
                    }
                }
            }
            

            _updateTime += Time.deltaTime;
            if (_updateTime > UpdateInterval)
            {
                var i = 0;
                for (i = _lastIndex; 
                    (i < _lastIndex + CellsPerUpdate) && (i < Width * Height); i++)
                {
                    var x = i % Width;
                    var y = i / Width;
                    var cell = _cells[x, y];
                    ClimateProcessing(cell);
                    cell.ProcessStep(_deltaTime);
                    UpdateAppearance(cell);

                    // Update UI after the cell is processed
                    if(cell == _selected)
                        _selected.UpdateUI();
                }

                _lastIndex = i;

                if (_lastIndex >= Width * Height - 1)
                {
                    AfterAllCellsUpdated();
                    _step += 1f * TimeScale;
                    _deltaTime = _updateTime;
                    _updateTime = 0f;
                    _lastIndex = 0;
                }
            }

            Debugger.Instance.Display("Game/Step", _step);
            Debugger.Instance.Display("Game/DT", _deltaTime);
        }

        void OnCellClick(Cell cell)
        {
            Debugger.Instance.LogFormat("Game/Log", "Clicked on cell {0}. Height = {1}", cell.gameObject.name, cell.Height);

            if (_selected != null)
                _selected.OnUnSelect();

            cell.OnSelect();
            _selected = cell;
            Terrain.SelectCell(cell.X, cell.Y);
        }

        void BuildWorld()
        {
            Terrain.CreateTerrain(World.TerrainSettings, Width, Height);

            for (var i = 0; i < Width; i++)
            {
                for (var j = 0; j < Height; j++)
                {                    
                    var cellPos = Terrain.CellToWorldOnTerrain(i, j);
                    var cellObjPost = new Vector3(cellPos.x, Mathf.Max(Terrain.SeaLevel, cellPos.y), cellPos.z);

                    var obj = (GameObject) Instantiate(CellPrefab, cellObjPost, Quaternion.identity, transform);
                    obj.name = string.Format("Cell-{0}-{1}", i, j);

                    var cell = obj.GetComponent<Cell>();
                    cell.X = i;
                    cell.Y = j;

                    cell.InitialSetup(cellPos.y);
                    _cells[i,j] = cell;
                }
            }
        }

        void UpdateAppearance(Cell cell)
        {
            Terrain.SetStateToTile(cell.X, cell.Y, cell.Climate);
            cell.UpdateAppearance(GetAppearancesFor(cell));
        }

        void ClimateProcessing(Cell cell)
        {
            var latitude = 1f - cell.Y/(float) Height;
            cell.Climate.Temperature = World.GetTemperature(_step, latitude, cell.Height);
            cell.Climate.Humidity = World.GetHumidity(_step, latitude, cell.Height);
        }

        void AfterAllCellsUpdated()
        {
            Tracker.Step();
            Terrain.UpdateStateMap();
        }

        public void ShowMessage(string text)
        {
            if(_messagesPanel == null)
                _messagesPanel = GameObject.Find("Canvas/Messages");

            if(MessagePrefab == null)
                return;

            var messageObj = (GameObject)GameObject.Instantiate(MessagePrefab, _messagesPanel.transform, false);
            var textCmp = messageObj.GetComponent<Text>();
            textCmp.text = text;
        }

        private void TrackerOnNewSpecies(Species species)
        {
            if (_lastEventCell != null)
                ShowSpeciesPointer(PointerPlus, species, _lastEventCell.transform);
            _speciesLogger.LogFormat("[step={0}] New species: {1}", _step, species.Name);
            ShowMessage(string.Format("New species evolved <color=yellow>{0}</color>", species.Name));
            PlayAudio(NewSpecies);
        }

        private void TrackerOnExtincted(Species species)
        {
            if(_lastEventCell != null)
                ShowSpeciesPointer(PointerCross, species, _lastEventCell.transform);
            ShowMessage(string.Format("Species <color=red>{0}</color> extincted", species.Name));
            PlayAudio(SpeciesExtinct);
        }

        public void ShowSpeciesPointer(GameObject pointerPrefab, Species species, Transform target)
        {
            if(pointerPrefab == null)
                return;

            var canvas = GameObject.Find("Canvas");
            if(canvas == null)
                return;

            var obj = (GameObject) Instantiate(pointerPrefab, canvas.transform, false);
            var iconObj = obj.transform.Find("Icon");
            if (iconObj != null)
            {
                var iconImage = iconObj.GetComponent<Image>();
                if (iconImage)
                {
                    iconImage.sprite = species.Icon;
                }
            }

            var scr = obj.GetComponent<UIPointingEvent>();
            scr.Target = target;
        }

        public void NewSpeciesOnCell(Species species, Cell cell)
        {
            _lastEventCell = cell;
            Tracker.SpeciesBorn(species);
        }

        public void ExtinctedSpeciesOnCell(Species species, Cell cell)
        {
            _lastEventCell = cell;
            Tracker.SpeciesDied(species);
        }

        public void SpawnInSelectedCell(Species species, long amount)
        {
            if(_selected == null)
                return;

            _selected.AddSpecies(species, amount);
        }

        public List<PropsAppearance> GetAppearancesFor(Cell cell)
        {
            var appearances = new List<PropsAppearance>();

            foreach (var appearance in _appearances)
            {
                if (appearance.Condition.Match(cell))
                {
                    if (appearance.Species != null)
                    {
                        if (cell.SpeciesStates.ContainsKey(appearance.Species))
                        {
                            var count = cell.SpeciesStates[appearance.Species].Population;
                            if (count > appearance.MinCount)
                                appearances.Add(appearance);
                        }
                    }
                    else
                    {
                        appearances.Add(appearance);
                    }
                }
            }

            return appearances;
        }

        public void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void ToggleMute()
        {
            AudioListener.volume = 1 - AudioListener.volume;
        }

        public void PlayAudio(AudioClipWithVolume clip)
        {
            if(clip.Clip != null)
                _audio.PlayOneShot(clip.Clip, clip.VolumeModifier);
        }

        // Global population
        private readonly Dictionary<Species, long> _globalPopulation = new Dictionary<Species, long>();
        public void ModifyPopulation(Species species, long amount)
        {
            if (_globalPopulation.ContainsKey(species))
            {
                // Existing species
                var population = _globalPopulation[species];
                if (population + amount <= 0)
                {
                    _speciesLogger.LogFormat("[step={0}] Extinction of {1}", _step, species.Name);
                    population = 0;
                }
                else
                {
                    population += amount;
                }
                _globalPopulation[species] = population;
            }
            else
            {
                _globalPopulation.Add(species, amount);
                _speciesLogger.LogFormat("[step={0}] New species {1}, amount={2}", _step, species.Name, amount);
            }

            Debugger.Instance.Display(String.Format("Species/Global/{0}", species.Name), _globalPopulation[species]);
        }

        public Cell GetCellAt(int x, int y)
        {
            return _cells[x, y];
        }

        public Cell GetCellAt(Vector2Int pos)
        {
            return _cells[pos.x, pos.y];
        }

        public Cell GetCellAt(Vector3 worldPosition)
        {
            var coords = Terrain.CellCoordsFromWorld(worldPosition);
            return _cells[coords.x, coords.y];
        }
    }
}
