using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.Data;

namespace Assets.Scripts.Gameplay
{
    public class SpeciesStatsTracker
    {
        public float StepToForget = 10f;
        public List<Species> KnownSpecies = new List<Species>();
        public Dictionary<Species, float> ForgetDictionary = new Dictionary<Species, float>();
        
        public event Action<Species> NewSpecies;
        public event Action<Species> Extincted;

        private float _steps;
        private float _check = 20f;

        public void Step()
        {
            _steps += 1f * GameManager.Instance.TimeScale;
            if (ForgetDictionary.Keys.Count > 0 && _steps > _check)
            {
                foreach (var key in ForgetDictionary.Keys.ToList())
                {
                    if (_steps > ForgetDictionary[key])
                    {
                        ForgetDictionary.Remove(key);
                        KnownSpecies.Remove(key);

                        if (Extincted != null)
                            Extincted(key);
                    }
                }
            }
        }

        public void SpeciesBorn(Species species)
        {
            if (species == null)
                return;

            if(KnownSpecies.Contains(species))
                return;

            KnownSpecies.Add(species);
            if (NewSpecies != null)
                NewSpecies(species);
        }

        public void SpeciesDied(Species species)
        {
            if (ForgetDictionary.ContainsKey(species))
            {
                ForgetDictionary[species] = _steps + StepToForget;
                _check = _steps + StepToForget;
            }
            else
            {
                ForgetDictionary.Add(species, _steps + StepToForget);
                _check = _steps + StepToForget;
            }
        }
    }
}
