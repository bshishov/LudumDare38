using System.Linq;
using Assets.Scripts.Data;
using Assets.Scripts.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Gameplay
{
    public class GameManager : Singleton<GameManager>
    {
        public const int Width = 20;
        public const int Height = 20;
        public const int CellsCount = Width * Height;
        public const float TileOffset = 0f;
        public const float SeasonSteps = 1000f;
        public const float UpdateInterval = 0.3f;
        public const int CellsPerUpdate = 40;
        

        public const float MeanNorthTemp = 0f; // Fahrenheit
        public const float MeanSouthTemp = 80f; // Fahrenheit

        public Cell[,] Cells = new Cell[Width,Height];
        public GameObject CellPrefab;
        public Group TreesGroup;
        public Group GrassGroup;
        public TerrainAppearance Appearance;
        public GameObject MessagePrefab;

        private Cell _selected;
        private float _step;
        private float _updateTime = 0f;
        private int _lastIndex = 0;
        private GameObject _messagesPanel;
        private TerrainGenerator _terrain;

        void Start ()
        {
            _terrain = GetComponent<TerrainGenerator>();
            BuildWorld();
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
                    SpeciesProcessing(cell);
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
            cell.OnClick();
            _selected = cell;
        }

        void BuildWorld()
        {
            for (var i = 0; i < Width; i++)
            {
                for (var j = 0; j < Height; j++)
                {
                    var obj = (GameObject) Instantiate(CellPrefab, new Vector3(i - Width / 2 + i * TileOffset, 0, j - Width / 2 + j * TileOffset), Quaternion.identity, transform);
                    obj.name = string.Format("Cell-{0}-{1}", i, j);

                    var cell = obj.GetComponent<Cell>();
                    cell.X = i;
                    cell.Y = j;
                    var height = _terrain.GetHeightFromTile(i, j);
                    if (height < 0f)
                    {
                        cell.TerrainType = TerrainType.Water;
                    }
                    else if(height < 0.4f)
                    {
                        cell.TerrainType = TerrainType.Plain;
                    }
                    else if (height < 0.7f)
                    {
                        //cell.TerrainType = TerrainType.Hills;
                        cell.TerrainType = TerrainType.Plain;
                    }
                    else
                    {
                        cell.TerrainType = TerrainType.Mountains;
                    }

                    cell.InitialTest();
                    Cells[i,j] = cell;
                }
            }
        }

        void UpdateAppearence(Cell cell)
        {
            if(Appearance == null)
                return;
            
            //Appearance.Construct(cell);

            if (TreesGroup != null)
            {
                var treesSpecies = cell.GetFromGroup(TreesGroup);
                var totalTrees = treesSpecies.Sum(sp => sp.Count);
            }
            
        }

        void ClimateProcessing(Cell cell)
        {
            var distFromNorth = 1f - cell.Y/(float) Height;
            var baseTemp = MeanNorthTemp + distFromNorth * (MeanSouthTemp - MeanNorthTemp);
            var seasonAmp = 30f + 5f * Random.value;
            var coeefYear = _step/SeasonSteps;
            var seasonTemp = baseTemp + Mathf.Sin(Mathf.PI * coeefYear) * seasonAmp;

            cell.Climate.Temperature = seasonTemp;
            _terrain.SetStateToTile(cell.X, cell.Y, cell.Climate);
        }

        void SpeciesProcessing(Cell cell)
        {
            foreach (var speciesState in cell.SpeciesStates.Values.ToList())
            {
                speciesState.Process(cell);
                if (speciesState.Count < 1f)
                {
                    cell.SpeciesStates.Remove(speciesState.Species);
                }
            }
        }

        void AfterStep()
        {
            // TODO: DETECT NEW AND EXTINCTED SPECIES
            _terrain.UpdateStateMap();
            ShowMessage(string.Format("Step {0}", _step));
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
    }
}
