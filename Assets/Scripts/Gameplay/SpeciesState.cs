using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Gameplay
{
    public class SpeciesState
    {
        public readonly Species Species;
        public long Count;
        public GameObject UI;
        public float AverageAge = 0;

        private Text _countText;

        public SpeciesState(Species species)
        {
            Species = species;
        }

        public string GetVerboseCount()
        {
            if (Count < 1f)
                return "0";

            return string.Format("{0:##,###}", Count);
        }

        public float GetTotalFoodValue()
        {
            return Mathf.Max(Species.FoodValue * Count, 0f);
        }

        public void FillUIInPanel(GameObject panel)
        {
            UI = panel;
            var nameObj = UI.transform.FindChild("Name");
            if (nameObj != null)
            {
                var nameText = nameObj.GetComponent<Text>();
                if (nameText != null)
                {
                    nameText.text = Species.Name;
                }
            }


            var iconObj = UI.transform.FindChild("Icon");
            if (iconObj != null)
            {
                var icon = iconObj.GetComponent<Image>();
                if (icon != null)
                {
                    if (Species.Icon != null)
                    {
                        icon.sprite = Species.Icon;
                    }
                }
            }


            var groupObj = UI.transform.FindChild("Group");
            if (groupObj != null)
            {
                var groupText = groupObj.GetComponent<Text>();
                if (groupText != null)
                {
                    if (Species.Group != null)
                    {
                        if (!string.IsNullOrEmpty(Species.Group.Name))
                        {
                            groupText.text = Species.Group.Name;
                        }
                        else
                        {
                            groupText.text = Species.Group.name;
                        }
                    }
                }
            }

            var countObj = UI.transform.FindChild("Count");
            if (countObj != null)
            {
                _countText = countObj.GetComponent<Text>();
            }
        }

        public void UpdateUI()
        {
            if (_countText == null)
                return;

            _countText.text = GetVerboseCount();
        }

        public void MultiplyCount(float amount)
        {
            Count = (long)Mathf.Ceil(Count * amount * GameManager.Instance.TimeScale);
        }

        public void ChangeCount(float amount)
        {
            amount = amount > 0 ? Mathf.Floor(amount) : Mathf.Ceil(amount);

            Count = (long) Mathf.Ceil(Count + amount*GameManager.Instance.TimeScale);
        }

        public void ProcessStep(Cell cell)
        {
            // COMFORT
            var comfort = Species.Climate.CalcComfort(cell);
            MultiplyCount(comfort);

            if(Count < 1)
                return;


            // BATTLE FOR EATING
            // 0.5 because only "males" will participate. Or its just because same battle calculates twice
            var power = Count *  Species.Agression;
            foreach (var enemySpecies in Species.Enemies)
            {
                if (cell.SpeciesStates.ContainsKey(enemySpecies))
                {
                    var enemy = cell.SpeciesStates[enemySpecies];
                    var enemyPower = enemy.Count * enemySpecies.Agression;
                    var winRate = Mathf.Clamp01(Mathf.Log(power/(enemyPower + 0.1f)+1f) * 0.721348f);
                    ChangeCount(-Count * (1f - winRate));
                    enemy.ChangeCount(-enemy.Count * winRate);
                }
            }


            // HUNGER AND EATING
            var starving = 0f;
            if (Species.Hunger > 0f)
            {
                var statesToEat = new List<SpeciesState>();
                foreach (var foodGroup in Species.Feed)
                {
                    var foodSpecies = cell.GetFromGroup(foodGroup);
                    foreach (var foodSpecy in foodSpecies)
                    {
                        if(!statesToEat.Contains(foodSpecy))
                            statesToEat.Add(foodSpecy);
                    }
                }
                var totalFoodValueAvailable = statesToEat.Sum(state => state.GetTotalFoodValue());
                var foodNeeded = Count * Species.Hunger;
                var willEat = Mathf.Min(foodNeeded, totalFoodValueAvailable);
                var eated = 0f;

                if (totalFoodValueAvailable > 0f)
                {
                    foreach (var feed in statesToEat)
                    {
                        var willEatFromSpecies = willEat * feed.GetTotalFoodValue()/totalFoodValueAvailable;
                        eated += willEatFromSpecies;
                        feed.ChangeCount(-willEatFromSpecies);
                    }
                }
                var deficit = foodNeeded - eated;
            
                starving = deficit/Species.Hunger;
            }

            // DEATH FROM STARVING
            ChangeCount(-starving);
            if (Count < 1)
                return;

            // MIGRATION
            var cellsForMigration = cell.EnumeratNeighbours().ToList();
            var isMigrated = false;
            while (cellsForMigration.Count > 0)
            {
                var target = cellsForMigration[Mathf.FloorToInt(Random.value*cellsForMigration.Count)];
                
                foreach (var migration in Species.Migrations)
                {
                    if (Random.value < migration.Chance)
                    {
                        if (migration.ClimateCondition.CalcComfort(cell) > 0.5f)
                        {
                            var migrated = Count * migration.CountFactor;
                            target.AddSpecies(Species, migrated);
                            ChangeCount(-migrated);
                            isMigrated = true;
                            break;
                        }
                    }
                }
                if (isMigrated)
                {
                    break;
                }
                else
                {
                    cellsForMigration.Remove(target);
                }
            }
            

            if (Count < 1)
                return;

            // REPRODUCTION
            var maxCap = (long)Mathf.Floor(10000000000f / (Species.Size + 1f));
            ChangeCount(Count * Species.ReproductionRate * comfort);
            if (Count > maxCap)
                Count = (long)maxCap;

            if (Count < 1)
                return;

            // MUTATION
            foreach (var mutation in Species.Mutations)
            {
                if (Random.value < mutation.Chance)
                {
                    var mutationComfort = mutation.ClimateCondition.CalcComfort(cell);
                    // Ceil in case if there is a ONE individual and mutation procs
                    var willMutate = mutation.CountFactor * Count * mutationComfort;

                    if (willMutate > 0.2f)
                    {
                        cell.AddSpecies(mutation.Target, willMutate);
                        ChangeCount(-willMutate);
                    }
                }
            }
        }
    }
}
