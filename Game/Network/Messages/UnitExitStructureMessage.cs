using UnityEngine;
using UnityEngine.Networking;

namespace FracturedState.Game.Network
{
    class UnitExitStructureMessage : ILockStepMessage
    {
        public uint Id { get; set; }
        public UnitManager unit;
        public Vector3 destination;

        public UnitExitStructureMessage(UnitManager unit, Vector3 destination)
        {
            this.unit = unit;
            this.destination = destination;
        }

        public void Process()
        {
            unit.OnExitIssued(destination);
        }
    }
}
