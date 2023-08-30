using FracturedState.Game;

namespace FracturedState.Scripting
{
    public class FanaticPassive : MutatorAbility
    {
        private const float Amount = 0.25f;
        
        public FanaticPassive(UnitManager owner) : base(owner)
        {
            MutatesWeapon = true;
        }

        public override void ExecuteAbility()
        {
            if (Owner == null) return;

            var weapon = Owner.ContextualWeapon;
            if (weapon == null) return;

            weapon.FireRate -= weapon.FireRate * Amount;
        }

        public override void Remove()
        {
            if (Owner == null) return;

            var weapon = Owner.ContextualWeapon;
            if (weapon == null) return;

            var fireRate = XmlCacheManager.Weapons[weapon.Name].FireRate;
            weapon.FireRate += fireRate * Amount;
            if (weapon.FireRate > fireRate)
            {
                weapon.FireRate = fireRate;
            }
        }
    }
}