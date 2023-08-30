using FracturedState.Game.AI;

namespace FracturedState.Game.Network
{
    public class UnitUnStunMessage : ILockStepMessage
    {
        public uint Id { get; set; }
        UnitManager unit;

        public UnitUnStunMessage(UnitManager unit)
        {
            this.unit = unit;
        }

        public void Process()
        {
            if (unit != null && unit.IsAlive)
            {
                UnitStunState state = unit.StateMachine.CurrentState as UnitStunState;
                if (state != null)
                {
                    state.UnStun();
                }
            }
        }
    }
}