using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Gameplay
{
    public class ObjectiveManager : MonoBehaviour
    {
        public GameObject UIPrefab;

        private Objective[] _objectives;
        private Transform _container;
        private readonly List<Objective> _completed = new List<Objective>();
        private readonly Dictionary<Objective, GameObject> _currentObjectives = new Dictionary<Objective, GameObject>();

        private int _currentObjectiveIndex = 0;
        private Caster _caster;
	
        void Start ()
        {
            _objectives = Resources.LoadAll<Objective>("Objectives");
            Debug.LogFormat("Loaded {0} objectives", _objectives.Length);

            _container = GameObject.Find("Canvas/Objectives").transform;

            if (_objectives.Length > 0)
            {
                StartCoroutine(AddObjectiveAfter(_objectives[0], 1f));
            }

            _caster = GetComponent<Caster>();
            _caster.OnSpellCasted += CasterOnOnSpellCasted;

            GameManager.Instance.Tracker.NewSpecies += TrackerOnNewSpecies;
        }

        private void TrackerOnNewSpecies(Species species)
        {
            foreach (var objective in _currentObjectives.Keys.ToList())
            {
                if (objective.RequiredSpecies != null && objective.RequiredSpecies == species)
                    Complete(objective);
            }
        }

        private void CasterOnOnSpellCasted(Spell spell, Cell cell)
        {
            foreach (var objective in _currentObjectives.Keys.ToList())
            {
                if(objective.RequiredSpell != null && objective.RequiredSpell == spell)
                    Complete(objective);
            }
        }

        IEnumerator AddObjectiveAfter(Objective objective, float delay)
        {
            yield return new WaitForSeconds(delay);
            AddObjective(objective);
        }

        public void AddObjective(Objective objective)
        {
            if(objective == null || UIPrefab == null)
                return;

            var obj = (GameObject) Instantiate(UIPrefab, _container);

            obj.transform.Find("Icon").GetComponent<Image>().sprite = objective.Icon;
            obj.transform.Find("Name").GetComponent<Text>().text = objective.Name;
            obj.transform.Find("Description").GetComponent<Text>().text = objective.Description;

            _currentObjectives.Add(objective, obj);
            _currentObjectiveIndex++;

            Debug.LogFormat("New objective {0}", objective.Name);
        }

        public void Complete(Objective objective)
        {
            Debug.LogFormat("Objective {0} completed", objective.Name);
            if (_currentObjectives.ContainsKey(objective))
            {
                var panel = _currentObjectives[objective];
                Destroy(panel, 2f);
                _currentObjectives.Remove(objective);

                _completed.Add(objective);

                var nextObjectives = _objectives.Where(o => o.RequiredObjective == objective && !_completed.Contains(o));

                foreach (var obj in nextObjectives)
                {
                    StartCoroutine(AddObjectiveAfter(obj, 4f));
                }
            }
        }
    }
}
