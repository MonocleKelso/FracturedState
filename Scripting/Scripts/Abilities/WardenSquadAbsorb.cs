using FracturedState.Game.AI;

namespace FracturedState.Scripting
{
    public class WardenSquadAbsorb : PassiveAttackInterrupt
    {
        public WardenSquadAbsorb(UnitManager caster, UnitManager attacker) : base(caster, attacker) { }

        public override void ExecuteAbility()
        {
            caster.StateMachine.ChangeState(new UnitIdleState(caster));
        }
    }
}