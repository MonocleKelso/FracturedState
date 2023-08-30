using FracturedState.Game.AI;
using FracturedState.Game.Data;

namespace FracturedState.Scripting
{
    /// <summary>
    /// An ability that enables transports to order their passengers to leave through an exit point defined in the casting unit's data file
    /// </summary>
    public class TransportDisembark : SelfAbility
    {
        public TransportDisembark(UnitManager caster, Ability ability) : base(caster, ability) { }

        public override void ExecuteAbility()
        {
            base.ExecuteAbility();
            for (int i = 0; i < caster.Passengers.Count; i++)
            {
                caster.Passengers[i].StateMachine.ChangeState(new UnitExitTransportState(caster.Passengers[i]));
            }
            caster.StateMachine.ChangeState(new UnitIdleState(caster));
        }
    }
}