using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Gameplay
{
    public class ObjectiveManager : MonoBehaviour
    {
        public GameObject UIPrefab;
        public Objective[] Objectives;

        private Transform _container;
        private readonly Dictionary<Objective, GameObject> _objectiveObjects = new Dictionary<Objective, GameObject>();

        private int _currentObjectiveIndex = 0;
	
        void Start ()
        {
            _container = GameObject.Find("Canvas/Objectives").transform;

            if (Objectives.Length > 0)
            {
                StartCoroutine(AddObjectiveAfter(Objectives[0], 1f));
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

            _objectiveObjects.Add(objective, obj);
            _currentObjectiveIndex++;
        }

        public void NextObjective()
        {
            if(_currentObjectiveIndex < Objectives.Length)
                AddObjective(Objectives[_currentObjectiveIndex]);
        }

        public void CompleteLastObjective()
        {
            Complete(Objectives[_currentObjectiveIndex - 1]);
        }

        public void Complete(Objective objective)
        {
            if (_objectiveObjects.ContainsKey(objective))
            {
                var panel = _objectiveObjects[objective];
                Destroy(panel);
                _objectiveObjects.Remove(objective);

                NextObjective();
            }
        }

        void Update()
        {

        }
    }
}
