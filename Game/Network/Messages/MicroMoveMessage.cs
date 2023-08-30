using FracturedState.Game.AI;
using UnityEngine;

namespace FracturedState.Game.Network
{
    public class MicroMoveMessage : ILockStepMessage
    {
        public uint Id { get; set; }
        UnitManager unit;
        Vector3 destination;

        public MicroMoveMessage(UnitManager unit, Vector3 destination)
        {
            this.unit = unit;
            this.destination = destination;
        }

        public void Process()
        {
            unit.SetMicroState(new MicroMoveState(unit, destination));
            unit.ExecuteMicroState();
        }
    }
}
