using System;

namespace Assets.Scripts.Data
{
    public class SpeciesState
    {
        public readonly Species Species;
        public float Count;

        public SpeciesState(Species species)
        {
            Species = species;
        }

        public string GetVerboseCount()
        {
            if (Count < 1f)
                return "0";

            return string.Format("{0}", Count);
        }
    }
}
