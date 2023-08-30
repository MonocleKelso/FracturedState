using FracturedState.Game.AI;

namespace FracturedState.Game.Network
{
    public class UnitEnterTransportMessage : ILockStepMessage
    {
        public uint Id { get; set; }
        UnitManager unit;
        UnitManager transport;

        public UnitEnterTransportMessage(UnitManager unit, UnitManager transport)
        {
            this.unit = unit;
            this.transport = transport;
        }

        public void Process()
        {
            if (unit.IsMine)
            {
                UnitBarkManager.Instance.EnterTransportBark(unit.Data);
            }
            unit.StateMachine.ChangeState(new UnitEnterTransportState(unit, transport));
        }
    }
}