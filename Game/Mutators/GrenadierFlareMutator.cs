using System.Collections.Generic;

namespace FracturedState.Game.Mutators
{
    public class GrenadierFlareMutator : IAbilityMutator
    {
        public int Cost => 1;
        
        public bool CanMutate(UnitManager unit)
        {
            return unit.Data.Name == "Grenadier";
        }

        public void Mutate(UnitManager unit)
        {
            var abilities = new List<string>(unit.Data.Abilities) {"GrenadierFlare"};
            unit.Data.Abilities = abilities.ToArray();
        }
    }
}