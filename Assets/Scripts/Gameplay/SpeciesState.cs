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

        // This function executes for each species in each state
        public void ProcessStep(Cell cell)
        {
            // COMFORT
            // Comfort - float [0 .. 1] -  коэффициент, показывающий, насколько приятно жить этому виду в этих климатических условиях
            // 0 - некомфортно
            // 1 - комфортно
            var comfort = Species.Climate.CalcComfort(cell);

            // Пусть те кому не комфортно - сдохнут. Если "комфорт" = 0.3, то выживет 30% популяции
            MultiplyCount(comfort);

            // Если все сдохли - дальше не считать
            if(Count < 1)
                return;


            // BATTLE FOR EATING
            // 0.5 because only "males" will participate. Or its just because same battle calculates twice
            // считается понятие "силы" (боевой) для вида в клетке как количество умноженое на агрессию
            var power = Count *  Species.Agression;

            // Для каждого врага этого вида
            foreach (var enemySpecies in Species.Enemies)
            {
                // Если на клетке имеется "враг"
                if (cell.SpeciesStates.ContainsKey(enemySpecies))
                {
                    // "узнать" врага
                    var enemy = cell.SpeciesStates[enemySpecies];

                    // Рассчитать силу врага как количество индивидов врага в текущей клетке умноженное на их агрессию
                    var enemyPower = enemy.Count * enemySpecies.Agression;

                    // Считаем винрейт (от 0 до 1 float). 1 - полная победа, 0 - полное поражение
                    var winRate = Mathf.Clamp01(Mathf.Log(power/(enemyPower + 0.1f)+1f) * 0.721348f);

                    // Изменяем количество индивидов в зависимости от винрейта. winrate = 0 - все сдохли, winrate = 1 - все выжили
                    ChangeCount(-Count * (1f - winRate));

                    // Точно также меняем количество индивидов у врага
                    enemy.ChangeCount(-enemy.Count * winRate);
                }
            }

            // Если все сдохли - дальше не считать
            if (Count < 1)
                return;

            // HUNGER AND EATING
            // Если нам нужно питаться
            if (Species.Hunger > 0f)
            {
                // список еды (список состояний вида)
                var statesToEat = new List<SpeciesState>();

                // для каждой группы существ, которые мы можем съесть
                foreach (var foodGroup in Species.Feed)
                {
                    // получить из текущей клетки всех видов этой группы
                    var foodSpecies = cell.GetFromGroup(foodGroup);

                    // для каждого вида из группы
                    foreach (var foodSpecy in foodSpecies)
                    {
                        // добавить их в список еды
                        if(!statesToEat.Contains(foodSpecy))
                            statesToEat.Add(foodSpecy);
                    }
                }

                // Сколько всего еды доступно?
                var totalFoodValueAvailable = statesToEat.Sum(state => state.GetTotalFoodValue());

                // Сколько еды нам нужно?
                var foodNeeded = Count * Species.Hunger;

                // Сколько мы еды съедим? Суммарно, в ед. еды
                var willEat = Mathf.Min(foodNeeded, totalFoodValueAvailable);

                // Определим переменную, сколько мы съели всего (в ед. еды)
                var eated = 0f;

                // Если есть что есть
                if (totalFoodValueAvailable > 0f)
                {
                    // Для каждого съедобного вида считаем, сколько мы от вида откусим
                    foreach (var feed in statesToEat)
                    {
                        // Считаем, сколько откусим от конкретного вида, считается как отношение еды, которое дает конкретный вид, к общему числу
                        var willEatFromSpecies = willEat * feed.GetTotalFoodValue()/totalFoodValueAvailable;

                        // добавляем к общему числу съеденных
                        eated += willEatFromSpecies;

                        // уменьшаем популяцию съедобного вида
                        feed.ChangeCount(-willEatFromSpecies / feed.Species.FoodValue);
                    }
                }

                // Считаем дефицит в ед. еды (сколько нам еды не хватило?)
                var deficit = foodNeeded - eated;
            
                // Считаем сколько индивидов голодает
                // Голодающие умирают
                ChangeCount(-(deficit / Species.Hunger));
            }

            // Если все сдохли - дальше не считать
            if (Count < 1)
                return;

            // MIGRATION
            // Соберем все соседние клетки в массив
            var cellsForMigration = cell.EnumeratNeighbours().ToList();

            // Флаг, показывающий, мигрировали ли мы или нет
            var isMigrated = false;

            // Пока есть еще куда мигрировать
            while (cellsForMigration.Count > 0)
            {
                // Берем рандомную клетку-соседа
                var target = cellsForMigration[Mathf.FloorToInt(Random.value*cellsForMigration.Count)];
                
                // Для каждой возможной миграции текущего вида
                foreach (var migration in Species.Migrations)
                {
                    // Прокнул ли шанс
                    if (Random.value < migration.Chance)
                    {
                        // Если условия ТЕКУЩЕЙ клетки - ОК (комфорт больше 0.5)
                        if (migration.ClimateCondition.CalcComfort(cell) > 0.5f)
                        {
                            // Количество мигрировавших = количество индивидов умноженное на процент миграции
                            var migrated = Count * migration.CountFactor;

                            // В клетку соседа добавляем существ в количичестве migrated
                            target.AddSpecies(Species, migrated);

                            // Из текущей клетки убираем существ
                            ChangeCount(-migrated);

                            // ставим флаг что мигрировали
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


            // Если все сдохли - дальше не считать
            if (Count < 1)
                return;

            // REPRODUCTION
            // Сичтаем максимально-возможное количество индивидов в этой клетке (в зависимости от Size)
            var maxCap = (long)Mathf.Floor(10000000000f / (Species.Size + 1f));

            // Прибавляем к популяции 
            ChangeCount(Count * Species.ReproductionRate * comfort);

            // Если больше максимума, то количество = максимум
            if (Count > maxCap)
                Count = (long)maxCap;


            // Если все сдохли - дальше не считать
            if (Count < 1)
                return;

            // MUTATION
            // Для каждой возможной мутации
            foreach (var mutation in Species.Mutations)
            {
                // Если прокнуло
                if (Random.value < mutation.Chance)
                {
                    // Считаем комофртны ли условия для мутации
                    var mutationComfort = mutation.ClimateCondition.CalcComfort(cell);

                    // Считаем сколько индивидов мутирует
                    var willMutate = mutation.CountFactor * Count * mutationComfort;

                    // Если мутирует больше 0.2 индивидов
                    if (willMutate > 0.2f)
                    {
                        // Добавить новый вид в клетку в количестве willMutate
                        cell.AddSpecies(mutation.Target, willMutate);

                        // убираем willMutate индивидов текущего вида
                        ChangeCount(-willMutate);
                    }
                }
            }
        }
    }
}
