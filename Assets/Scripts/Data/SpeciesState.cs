using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Data
{
    public class SpeciesState
    {
        public readonly Species Species;
        public float Count;
        public GameObject UI;

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
            return Mathf.Max(Species.FoodValue*Count, 0f);
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
                        groupText.text = Species.Group.Name;
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

        public void Process(Cell cell)
        {
            // COMFORT
            var comfort = Species.Climate.CalcComfort(cell);
            Count = Count * comfort;
            if(Count < 1f)
                return;

            // HUNGER AND EATING
            var starving = 0f;
            var willMigrate = Count * 0.2f;

            if (Species.Hunger > 0f)
            {
                var statesToEat = new List<SpeciesState>();
                foreach (var foodGroup in Species.Feed)
                {
                    statesToEat.AddRange(cell.GetFromGroup(foodGroup));
                }
                var totalFoodValueAvailable = statesToEat.Sum(state => state.GetTotalFoodValue());
                var foodNeeded = Count * Species.Hunger;
                var willEat = Mathf.Min(foodNeeded, totalFoodValueAvailable);
                var eated = 0f;
                if (totalFoodValueAvailable > 0f)
                {
                    foreach (var state in statesToEat)
                    {
                        var willEatFromSpecies = willEat*state.GetTotalFoodValue()/totalFoodValueAvailable;
                        eated += willEatFromSpecies;
                        state.Count -= willEatFromSpecies;
                    }
                }
                var deficit = foodNeeded - eated;
            
                starving = deficit/Species.Hunger;
                willMigrate = deficit*(eated/foodNeeded)*Count;
            }


            // REPRODUCTION
            var maxCap = 10000000000f / (Species.Size + 1f);
            Count += Count * Species.ReproductionRate;
            if (Count > maxCap)
                Count = maxCap;

            if (Count < 1f)
                return;

            // MIGRATION
            var migrated = 0f;
            foreach (var migration in Species.Migrations)
            {
                var target = cell.GetRandomNeighbour();

                if (migration.ClimateCondition.CalcComfort(cell) > 0.5f)
                {
                    migrated = willMigrate * migration.Chance;
                    target.AddSpecies(Species, migrated);
                    Count -= migrated;
                    willMigrate -= migrated;
                    break;
                }
            }

            if (Count < 1f)
                return;

            // DEATH FROM STARVING
            Count = Count - (starving - migrated);

            // MUTATION 
            foreach (var mutation in Species.Mutations)
            {
                if (mutation.ClimateCondition.CalcComfort(cell) > 0.5f)
                {
                    var willMutate = mutation.Chance*Count;
                    cell.AddSpecies(mutation.Target, willMutate);
                    Count -= willMutate;
                }
            }
        }
    }
}
