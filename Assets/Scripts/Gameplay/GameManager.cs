using Assets.Scripts.Utils;

namespace Assets.Scripts.Gameplay
{
    public class GameManager : Singleton<GameManager>
    {
        public const int Width = 10;
        public const int Height = 10;

        public Cell[][] Cells;

        void Start ()
        {
            BuildWorld();
        }
        
        void Update ()
        {
        }

        void BuildWorld()
        {
            
        }
    }
}
