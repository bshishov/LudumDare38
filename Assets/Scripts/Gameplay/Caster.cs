using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Assets.Scripts.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Gameplay
{
    public class Caster : MonoBehaviour
    {
        public GameObject SelectorPrefab;
        public GameObject SpellUIPrefab;

        public bool SelectionIsActive { get { return _selectionActive; } }

        public class SpellState
        {
            public Spell Spell;
            public float Cooldown;
            public Button SpellButton;
            public Text CooldownText;

            public void UpdateUI()
            {
                if (Cooldown <= 0f)
                {
                    if (!SpellButton.interactable)
                    {
                        SpellButton.interactable = true;
                        CooldownText.color = new Color(0, 0, 0, 0);
                    }
                }
                else
                {
                    if (SpellButton.interactable)
                    {
                        SpellButton.interactable = false;
                        CooldownText.color = Color.white;
                    }

                    CooldownText.text = string.Format("{0:###}", Cooldown);
                    Cooldown -= Time.deltaTime;
                }
            }
        }

        private readonly List<SpellState> _states = new List<SpellState>();
        private Spell[] _spells;
        private bool _selectionActive;
        private GameObject _currentSelector;
        private Spell _currentSpell;
        private Cell _lastHoveredCell;

        
        
        void Start ()
        {
            _spells = Resources.LoadAll<Spell>("Spells");
            Debug.LogFormat("Loaded {0} spells", _spells.Length);
            
            var spellsContainer = GameObject.Find("Canvas/SpellsPanel/Spells");
            if(spellsContainer == null)
                return;

            for (var index = 0; index < _spells.Length; index++)
            {
                var spell = _spells[index];
                var uiObj = (GameObject) Instantiate(SpellUIPrefab, spellsContainer.transform, false);
                uiObj.transform.Find("Size").GetComponent<Text>().text = spell.GetSizeVerbose();
                uiObj.transform.Find("Icon").GetComponent<Image>().sprite = spell.Icon;
                uiObj.GetComponent<Button>().onClick.AddListener(() => OnSpellIconClick(spell));


                var state = new SpellState
                {
                    Spell = spell,
                    Cooldown = 0f,
                    CooldownText = uiObj.transform.Find("Cooldown").GetComponent<Text>(),
                    SpellButton = uiObj.GetComponent<Button>()
                };
                _states.Add(state);
            }
        }

        void OnSpellIconClick(Spell spell)
        {
            DeactivateSelector();
            ActivateSelector(spell);
        }

        void Update ()
        {
            if (_selectionActive)
            {
                if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(-1))
                {
                    RaycastHit hit;
                    var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                    if (Physics.Raycast(ray, out hit))
                    {
                        var cell = hit.collider.gameObject.GetComponent<Cell>();
                        if (cell != null)
                        {
                            HoverCell(cell);
                        }
                    }

                    if (Input.GetMouseButtonDown(0))
                    {
                        Cast();
                    }
                }

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    DeactivateSelector();
                }
            }

            foreach (var spellState in _states)
            {
                spellState.UpdateUI();
            }
        }

        void HoverCell(Cell hovered)
        {
            var x = hovered.X;
            var y = hovered.Y;

            x = (int)Math.Max(x, _currentSpell.HalfWidth);
            x = (int)Math.Min(x, GameManager.Width - 1 - _currentSpell.HalfWidth);

            y = (int)Math.Max(y, _currentSpell.HalfHeight);
            y = (int)Math.Min(y, GameManager.Height - 1 - _currentSpell.HalfHeight);

            _lastHoveredCell = GameManager.Instance.Cells[x, y];

            _currentSelector.transform.position = new Vector3(_lastHoveredCell.transform.position.x, _currentSelector.transform.position.y, _lastHoveredCell.transform.position.z);
            //_currentSelector.transform.position = cell.transform.position;
            
        }

        public void ActivateSelector(Spell spell)
        {
            if(SelectorPrefab == null || spell == null)
                return;

            _currentSelector = (GameObject) Instantiate(SelectorPrefab);
            _currentSpell = spell;
            _currentSelector.transform.localScale = new Vector3(_currentSelector.transform.localScale.x * spell.EffectWidth, _currentSelector.transform.localScale.y, _currentSelector.transform.localScale.z * spell.EffectHeight);
            _selectionActive = true;

            RaycastHit hit;
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                var cell = hit.collider.gameObject.GetComponent<Cell>();
                if (cell != null)
                {
                    HoverCell(cell);
                }
            }
        }

        void DeactivateSelector()
        {
            _selectionActive = false;
            if (_currentSelector != null)
                Destroy(_currentSelector);
        }

        void Cast()
        {
            if (_currentSpell != null && _lastHoveredCell != null)
            {
                GameManager.Instance.ShowMessage(string.Format("Casting <color=yellow>{0}</color>", _currentSpell.Name));
                StartCoroutine(DelayedBuff(_currentSpell.DelayBeforeBuff, _lastHoveredCell.X, _lastHoveredCell.Y, _currentSpell));

                if (_currentSpell.Effect != null)
                {
                    var go = (GameObject)Instantiate(_currentSpell.Effect);
                    go.transform.position += _lastHoveredCell.transform.position;
                }
                
                DeactivateSelector();

                var state = _states.FirstOrDefault(s => s.Spell == _currentSpell);
                if (state != null)
                    state.Cooldown = _currentSpell.Cooldown;
            }
        }

        IEnumerator DelayedBuff(float delay, int centerX, int centerY, Spell spell)
        {
            yield return new WaitForSeconds(delay);

            // Center cell first (for proper notifications
            GameManager.Instance.Cells[centerX, centerY].ApplyBuff(spell.CellBuff);

            for (var i = 0; i < spell.EffectWidth; i++)
            {
                for (var j = 0; j < spell.EffectHeight; j++)
                {
                    var x = centerX - spell.HalfWidth + i;
                    var y = centerY - spell.HalfHeight + j;

                    // Skip the center cell
                    if (centerX == x && centerY == y)
                        continue;

                    var cell = GameManager.Instance.Cells[x, y];
                    cell.ApplyBuff(spell.CellBuff);
                }
            }
        }
    }
}

