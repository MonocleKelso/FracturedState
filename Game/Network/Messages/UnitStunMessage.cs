using FracturedState.Game.AI;

namespace FracturedState.Game.Network
{
    public class UnitStunMessage : ILockStepMessage
    {
        public uint Id { get; set; }
        UnitManager unit;
        float duration;

        public UnitStunMessage(UnitManager unit, float duration)
        {
            this.unit = unit;
            this.duration = duration;
        }

        public void Process()
        {
            if (unit != null && unit.IsAlive)
            {
                unit.StateMachine.ChangeState(new UnitStunState(unit, duration));
            }
        }
    }
}