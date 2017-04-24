using Assets.Scripts.Data;
using UnityEngine;

namespace Assets.Scripts.Gameplay
{
    public class BuffState
    {
        public readonly Buff Buff;
        public bool IsActive { get { return Remaining > 0f; } }
        public float Remaining;

        private readonly Cell _cell;
        private readonly GameObject _effect;

        public BuffState(Buff buff, Cell cell)
        {
            _cell = cell;
            Buff = buff;
            Remaining = buff.Duration;

            if (buff.EffectPrefab)
            {
                _effect = (GameObject) GameObject.Instantiate(buff.EffectPrefab, cell.transform, false);
            }

            // DEPLOY PASSANGERS
            foreach (var passanger in buff.Passangers)
            {
                cell.AddSpecies(passanger.Species, passanger.Count);
            }
        }

        public void ProcessStep()
        {
            if (Remaining > 0f)
            {
                _cell.Climate.Temperature = Mathf.Clamp(_cell.Climate.Temperature + Buff.TempeartureChange, -100, 200);
                _cell.Climate.Humidity = Mathf.Clamp(_cell.Climate.Humidity + Buff.HumidityChange, 0, 100);
                if (Buff.ExctinctionRate > 0f)
                {
                    foreach (var speciesState in _cell.SpeciesStates)
                    {
                        speciesState.Value.Count -= speciesState.Value.Count * Buff.ExctinctionRate;
                    }
                }
            }

            Remaining -= 1f * GameManager.Instance.TimeScale;

            if (Remaining <= 0f)
            {
                if (_effect != null)
                {
                    GameObject.Destroy(_effect);
                }
            }
        }
    }
}
