using System.Linq;
using Assets.Scripts.Data;
using Assets.Scripts.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class UIDebugPanel : MonoBehaviour
    {
        private Dropdown _speciesSelect;
        private Button _spawnButton;
        private InputField _spawnAmount;

        private Species[] _species;

        void Start ()
        {
            _speciesSelect = transform.Find("SpeciesSelect").GetComponent<Dropdown>();
            _spawnButton = transform.Find("SpawnButton").GetComponent<Button>();
            _spawnAmount = transform.Find("SpawnAmount").GetComponent<InputField>();
            
            _species = Resources.LoadAll<Species>("Species");
            Debug.LogFormat("Loaded {0} species", _species.Length);
            _speciesSelect.ClearOptions();
            _speciesSelect.AddOptions(_species.Select(s => s.name).ToList());

            _spawnButton.onClick.AddListener(SpawnClick);
        }

        void SpawnClick()
        {
            long amount = 0;
            if (long.TryParse(_spawnAmount.text, out amount))
            {
                var specie = _species[_speciesSelect.value];
                GameManager.Instance.SpawnInSelectedCell(specie, amount);
            }   
        }
    }
}
