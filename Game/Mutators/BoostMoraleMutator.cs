using System.Collections.Generic;

namespace FracturedState.Game.Mutators
{
    public class BoostMoraleMutator : IAbilityMutator
    {
        public int Cost => 1;
        
        public bool CanMutate(UnitManager unit)
        {
            return unit.Data.Name == "Conscript" || unit.Data.Name == "Knight";
        }

        public void Mutate(UnitManager unit)
        {
            var abilities = new List<string>(unit.Data.Abilities) {"BoostMorale"};
            unit.Data.Abilities = abilities.ToArray();
        }
    }
}