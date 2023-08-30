using FracturedState.Game.AI;

namespace FracturedState.Game.Mutators
{
    public class FlamethrowerPyreMutator : IWeaponSwapMutator
    {
        public int Cost => 1;
        
        public bool CanMutate(UnitManager unit)
        {
            return unit.Data.Name == "Flamethrower";
        }

        public void Mutate(UnitManager unit)
        {
            unit.Data.WeaponName = "FlamethrowerWithPyre";
        }
    }
}