using FracturedState.Game.Data;

namespace FracturedState.Game.Management.StructureBonus
{
    public class SanctuaryBonus : IStructureBonus
    {
        public string StructureName => "Sanctuary";
        public string BonusText => "-10% skill cooldown";
        public string HelperText => "bonus.sanctuary";
        
        public void ApplyOnUnit(UnitManager unit)
        {
            Apply(unit.GetAbilities());
            Apply(unit.GetPassiveAbilities());
            Apply(unit.GetSquadAbilities());
        }

        public void RemoveFromUnit(UnitManager unit)
        {
            Remove(unit.GetAbilities());
            Remove(unit.GetPassiveAbilities());
            Remove(unit.GetSquadAbilities());
        }

        public void ApplyOnWeapon(Weapon weapon)
        {
            // empty does not modify weapon
        }

        public void RemoveFromWeapon(Weapon weapon)
        {
            // empty does not modify weapon
        }

        private void Apply(Ability[] ab)
        {
            if (ab == null) return;
            foreach (var a in ab)
            {
                a.CooldownTime -= a.CooldownTime * 0.1f;
            }
        }

        private void Remove(Ability[] ab)
        {
            if (ab == null) return;
            foreach (var a in ab)
            {
                a.CooldownTime += a.CooldownTime * 0.1f;
                var orig = XmlCacheManager.Abilities[a.Name];
                if (a.CooldownTime > orig.CooldownTime)
                {
                    a.CooldownTime = orig.CooldownTime;
                }
            }
        }
    }
}