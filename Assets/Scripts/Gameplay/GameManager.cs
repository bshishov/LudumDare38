using System;
using System.Linq;
using Assets.Scripts.Data;
using Assets.Scripts.Utils;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Gameplay
{
    public class GameManager : Singleton<GameManager>
    {
        public const int Width = 20;
        public const int Height = 20;
        public const int CellsCount = Width * Height;
        public const float SeasonSteps = 1000f;
        public const float UpdateInterval = 0.3f;
        public const int CellsPerUpdate = 40;

        public const float MeanNorthTemp = 0f; // Fahrenheit
        public const float MeanSouthTemp = 80f; // Fahrenheit

        public Cell[,] Cells = new Cell[Width,Height];
        public GameObject CellPrefab;
        public TerrainAppearance Appearance;
        public TerrainTypesCollection Terrains;

        [Header("Visuals")]
        public GameObject MessagePrefab;
        public GameObject PointerPlus;
        public GameObject PointerCross;


        public readonly SpeciesStatsTracker Tracker = new SpeciesStatsTracker();

        private Cell _selected;
        private float _step;
        private float _updateTime = 0f;
        private int _lastIndex = 0;
        private GameObject _messagesPanel;
        private TerrainGenerator _terrain;
        private Cell _lastEventCell;

        void Start ()
        {
            _terrain = GetComponent<TerrainGenerator>();
            BuildWorld();

            if (Tracker != null)
            {
                Tracker.NewSpecies += TrackerOnNewSpecies;
                Tracker.Extincted += TrackerOnExtincted;
            }
        }

        void Update ()
        {
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit))
                {
                    var cell = hit.collider.gameObject.GetComponent<Cell>();
                    if (cell != null)
                        OnCellClick(cell);
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
                    cell.Step();
                    UpdateAppearence(cell);
                }

                _lastIndex = i;

                if (_lastIndex >= CellsCount - 1)
                {
                    AfterStep();
                    _step += 1f;
                    _updateTime = 0f;
                    _lastIndex = 0;
                }

                if (_selected != null)
                    _selected.UpdateUI();
            }
        }

        void OnCellClick(Cell cell)
        {
            Debug.LogFormat("Clicked on cell {0}", cell.gameObject.name);

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

                    var obj = (GameObject) Instantiate(CellPrefab, new Vector3(i - Width * 0.5f + 0.5f, height - 0.7f, j - Width * 0.5f + 0.5f), Quaternion.identity, transform);
                    obj.name = string.Format("Cell-{0}-{1}", i, j);

                    var cell = obj.GetComponent<Cell>();
                    cell.X = i;
                    cell.Y = j;

                    cell.InitialSetup(height);
                    Cells[i,j] = cell;
                }
            }
        }

        void UpdateAppearence(Cell cell)
        {
            if (Appearance != null)
            {
                cell.UpdateAppearance(Appearance);
            }
        }

        void ClimateProcessing(Cell cell)
        {
            var distFromNorth = 1f - cell.Y/(float) Height;
            var baseTemp = MeanNorthTemp + distFromNorth * (MeanSouthTemp - MeanNorthTemp);
            var seasonAmp = 25f + 5f * Random.value;
            var coeefYear = _step/SeasonSteps;
            var seasonTemp = baseTemp + Mathf.Sin(Mathf.PI * coeefYear) * seasonAmp;

            cell.Climate.Temperature = seasonTemp;
            _terrain.SetStateToTile(cell.X, cell.Y, cell.Climate);
        }
       

        void AfterStep()
        {
            Tracker.Step();
            _terrain.UpdateStateMap();
            //ShowMessage(string.Format("Step {0}", _step));
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
        }

        private void TrackerOnExtincted(Species species)
        {
            if(_lastEventCell != null)
                ShowSpeciesPointer(PointerCross, species, _lastEventCell.transform);
            ShowMessage(string.Format("Species <color=red>{0}</color> extincted", species.Name));
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
    }
}
