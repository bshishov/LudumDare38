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
            return Mathf.Max(Species.FoodValue * Count * 0.9f, 0f);
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

        public void MultiplyCount(float amount)
        {
            Count = (long)Mathf.Ceil(Count * amount * GameManager.Instance.TimeScale);
        }

        public void ChangeCount(float amount)
        {
            Count = (long)Mathf.Ceil(Count + amount * GameManager.Instance.TimeScale);
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
            var power = 0.5f * Count *  Species.Agression;
            foreach (var enemySpecies in Species.Enemies)
            {
                if (!cell.SpeciesStates.ContainsKey(enemySpecies))
                {
                    var enemy = cell.SpeciesStates[enemySpecies];
                    var enemyPower = enemy.Count*0.5f*enemySpecies.Agression;
                    var winRate = Mathf.Clamp01(Mathf.Log(power/(enemyPower + 0.1f)) * 0.721348f);
                    ChangeCount(-0.5f * Count * (1f - winRate));
                    enemy.ChangeCount(-0.5f * enemy.Count * winRate);
                }
            }


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
                    foreach (var feed in statesToEat)
                    {
                        var willEatFromSpecies = willEat * feed.GetTotalFoodValue()/totalFoodValueAvailable;
                        eated += willEatFromSpecies;
                        feed.ChangeCount(-willEatFromSpecies);
                    }
                }
                var deficit = foodNeeded - eated;
            
                starving = deficit/Species.Hunger;
                willMigrate = deficit*(eated/foodNeeded) * Count;
            }


            // REPRODUCTION
            var maxCap = (long)Mathf.Floor(10000000000f / (Species.Size + 1f));
            ChangeCount(Count * Species.ReproductionRate * comfort);
            if (Count > maxCap)
                Count = (long)maxCap;

            if (Count < 1)
                return;

            // MIGRATION
            long migrated = 0;
            foreach (var migration in Species.Migrations)
            {
                var target = cell.GetRandomNeighbour();

                if (migration.ClimateCondition.CalcComfort(cell) > 0.5f)
                {
                    migrated = (long)Mathf.Floor(willMigrate * migration.Chance);
                    target.AddSpecies(Species, migrated);
                    ChangeCount(-migrated);
                    willMigrate -= migrated;
                    break;
                }
            }

            if (Count < 1)
                return;

            // DEATH FROM STARVING
            ChangeCount(-(starving - migrated));

            // MUTATION 
            foreach (var mutation in Species.Mutations)
            {
                if (mutation.ClimateCondition.CalcComfort(cell) > 0.5f)
                {
                    var willMutate = mutation.Chance * Count;
                    cell.AddSpecies(mutation.Target, willMutate);
                    ChangeCount(-willMutate);
                }
            }
        }
    }
}
