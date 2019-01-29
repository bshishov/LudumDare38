using Assets.Scripts.Data;
using UnityEngine;

namespace Assets.Scripts.Gameplay
{
    public class BuffState
    {
        public readonly Buff Buff;
        public bool IsActive { get { return Remaining > -Buff.DecayTime; } }
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

            GameManager.Instance.PlayAudio(Buff.SoundEffect);
        }

        public void ProcessStep(float dt)
        {
            // Buffs are updated after global climate calculations and before species in cell
            // So climate can be easily overriden by the buff
            if (Remaining > 0f)
            {
                _cell.Climate.Temperature = Mathf.Clamp(_cell.Climate.Temperature + Buff.TempeartureChange, -100, 200);
                _cell.Climate.Humidity = Mathf.Clamp(_cell.Climate.Humidity + Buff.HumidityChange, 0, 100);
                if (Buff.ExctinctionRate > 0f)
                {
                    foreach (var speciesState in _cell.SpeciesStates)
                    {
                        speciesState.Value.Population -= (long)Mathf.Floor(speciesState.Value.Population * Buff.ExctinctionRate);
                    }
                }
            }

            // Updates are called once in a while (approx. 1 / sec)
            // as GameManager decides
            Remaining -= GameManager.Instance.TimeScale * dt;

            // If we are in the decay step
            // linearly decay the buff climate effects
            if (Remaining < 0)
            {
                var k = 1 - Mathf.Clamp01(Mathf.Abs(Remaining) / Buff.DecayTime);
                
                _cell.Climate.Temperature = Mathf.Clamp(_cell.Climate.Temperature + k * Buff.TempeartureChange, -100, 200);
                _cell.Climate.Humidity = Mathf.Clamp(_cell.Climate.Humidity + k * Buff.HumidityChange, 0, 100);

                // Effect has to be removed right after main buff duration is gone
                // But decay is still happening
                if (_effect != null)
                {
                    GameObject.Destroy(_effect);
                }
            }
        }
    }
}
