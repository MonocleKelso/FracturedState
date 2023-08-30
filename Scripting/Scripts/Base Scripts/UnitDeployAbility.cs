using FracturedState.Game.Data;

namespace FracturedState.Scripting
{
    public class UnitDeployAbility : SelfAbility
    {
        public UnitDeployAbility(UnitManager caster, Ability ability) : base(caster, ability) { }

        public override void ExecuteAbility()
        {
            base.ExecuteAbility();
            caster.LocoMotor = null;
        }
    }
}