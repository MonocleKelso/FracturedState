using FracturedState.Game.AI;
using FracturedState.Game.Data;

namespace FracturedState.Scripting
{
    public class ShrineRecall : SelfAbility
    {
        public ShrineRecall(UnitManager caster, Ability ability) : base(caster, ability) { }

        public override void ExecuteAbility()
        {
            caster.StateMachine.ChangeState(new UnitIdleState(caster));
        }
    }
}