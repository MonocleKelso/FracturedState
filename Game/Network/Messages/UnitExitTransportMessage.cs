using FracturedState.Game.AI;

namespace FracturedState.Game.Network
{
    public class UnitExitTransportMessage : ILockStepMessage
    {
        public uint Id { get; set; }
        UnitManager unit;

        public UnitExitTransportMessage(UnitManager unit)
        {
            this.unit = unit;
        }

        public void Process()
        {
            if (unit.Transport != null)
            {
                unit.StateMachine.ChangeState(new UnitExitTransportState(unit));
            }
            else
            {
                unit.PassengerSlot = null;
                unit.StateMachine.ChangeState(new UnitIdleState(unit));
            }
        }
    }
}