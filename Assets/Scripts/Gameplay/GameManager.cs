using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Data;
using Assets.Scripts.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Gameplay
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(TerrainGenerator))]
    [RequireComponent(typeof(Caster))]
    public class GameManager : Singleton<GameManager>
    {
        public const int Width = 30;
        public const int Height = 30;
        public const int CellsCount = Width * Height;
        public const float UpdateInterval = 0.1f;
        public const int CellsPerUpdate = 60;

        public World World;
        public Cell[,] Cells = new Cell[Width,Height];
        public GameObject CellPrefab;
        public TerrainTypesCollection Terrains;

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


        public float Step { get { return _step; } }
        public readonly SpeciesStatsTracker Tracker = new SpeciesStatsTracker { StepToForget = 20f };

        private Cell _selected;
        private float _step;
        private float _updateTime = 0f;
        private float _deltaTime = 0f;

        private int _lastIndex = 0;
        private GameObject _messagesPanel;
        private TerrainGenerator _terrain;
        private Cell _lastEventCell;
        private PropsAppearance[] _appearances;
        private Caster _caster;
        private AudioSource _audio;

        void Start ()
        {
            _audio = GetComponent<AudioSource>();
            _terrain = GetComponent<TerrainGenerator>();
            BuildWorld();

            ShowMessage("Loading resources");
            _appearances = Resources.LoadAll<PropsAppearance>("Appearance");
            Debug.LogFormat("Loaded {0} appearances", _appearances.Length);

            if (Tracker != null)
            {
                Tracker.NewSpecies += TrackerOnNewSpecies;
                //Tracker.Extincted += TrackerOnExtincted;
            }

            _caster = GetComponent<Caster>();
            _caster.OnSpellCasted += CasterOnOnSpellCasted;
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
                        var cell = hit.collider.gameObject.GetComponent<Cell>();
                        if (cell != null)
                            OnCellClick(cell);
                    }
                }
            }
            

            _updateTime += Time.deltaTime;
            if (_updateTime > UpdateInterval)
            {
                var i = 0;
                for (i = _lastIndex; 
                    (i < _lastIndex + CellsPerUpdate) && (i < CellsCount); i++)
                {
                    var x = i % Width;
                    var y = i / Width;
                    var cell = Cells[x, y];
                    ClimateProcessing(cell);
                    cell.ProcessStep(_deltaTime);
                    UpdateAppearance(cell);

                    // Update UI after the cell is processed
                    if(cell == _selected)
                        _selected.UpdateUI();
                }

                _lastIndex = i;

                if (_lastIndex >= CellsCount - 1)
                {
                    AfterAllCellsUpdated();
                    _step += 1f * TimeScale;
                    _deltaTime = _updateTime;
                    _updateTime = 0f;
                    _lastIndex = 0;
                }
            }
        }

        void OnCellClick(Cell cell)
        {
            Debug.LogFormat("Clicked on cell {0}. Height = {1}", cell.gameObject.name, cell.Height);

            if (_selected != null)
                _selected.OnUnSelect();

            cell.OnSelect();
            _selected = cell;
            _terrain.SelectCell(cell.X, cell.Y);
        }

        void BuildWorld()
        {
            _terrain.Generate();

            for (var i = 0; i < Width; i++)
            {
                for (var j = 0; j < Height; j++)
                {
                    var height = _terrain.GetHeightFromTile(i, j);

                    var obj = (GameObject) Instantiate(CellPrefab, new Vector3(i - Width * 0.5f + 0.5f, Mathf.Max(0, height) - 0.5f, j - Width * 0.5f + 0.5f), Quaternion.identity, transform);
                    obj.name = string.Format("Cell-{0}-{1}", i, j);

                    var cell = obj.GetComponent<Cell>();
                    cell.X = i;
                    cell.Y = j;

                    cell.InitialSetup(height);
                    Cells[i,j] = cell;
                }
            }
        }

        void UpdateAppearance(Cell cell)
        {
            _terrain.SetStateToTile(cell.X, cell.Y, cell.Climate);
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
            _terrain.UpdateStateMap();
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
    }
}
