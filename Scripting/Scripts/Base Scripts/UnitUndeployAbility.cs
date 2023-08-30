using FracturedState.Game.Data;
using FracturedState.Game.Modules;

namespace FracturedState.Scripting
{
    public class UnitUndeployAbility : SelfAbility
    {
        public UnitUndeployAbility(UnitManager caster, Ability ability) : base(caster, ability) { }

        public override void ExecuteAbility()
        {
            base.ExecuteAbility();
            caster.LocoMotor = LocomotorFactory.Create(caster.Data.LocomotorName, caster);
        }
    }
}