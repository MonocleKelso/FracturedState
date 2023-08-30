
using FracturedState.Game.Data;

namespace FracturedState.Scripting
{
    public class TestAbility : SelfAbility
    {
        public TestAbility(UnitManager caster, Ability ability) : base(caster, ability) { }

        public override void ExecuteAbility()
        {
            caster.DamageProcessor.TakeDamage(9999, null, Game.Data.Weapon.DummyName);
        }
    }
}