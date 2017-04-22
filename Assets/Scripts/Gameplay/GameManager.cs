using Assets.Scripts.Utils;
using UnityEngine;

namespace Assets.Scripts.Gameplay
{
    public class GameManager : Singleton<GameManager>
    {
        public const int Width = 10;
        public const int Height = 10;

        public Cell[,] Cells = new Cell[Width,Height];
        public GameObject CellPrefab;

        void Start ()
        {
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
        }

        void OnCellClick(Cell cell)
        {
            Debug.LogFormat("Clicked on cell {0}", cell.gameObject.name);    
        }

        void BuildWorld()
        {
            for (var i = 0; i < Width; i++)
            {
                for (var j = 0; j < Height; j++)
                {
                    var obj = (GameObject) Instantiate(CellPrefab, new Vector3(i - Width / 2, 0, j - Width / 2), Quaternion.identity, transform);
                    obj.name = string.Format("Cell-{0}-{1}", i, j);
                    Cells[i,j] = obj.GetComponent<Cell>();
                }
            }
        }
    }
}
