using FracturedState.Game.AI;
using FracturedState.Game.Data;

namespace FracturedState.Scripting
{
    public class WardenShieldDrop : SelfAbility
    {
        public WardenShieldDrop(UnitManager caster, Ability ability) : base(caster, ability) { }

        public override void ExecuteAbility()
        {
            base.ExecuteAbility();
            caster.StateMachine.ChangeState(new UnitIdleState(caster));
        }
    }
}