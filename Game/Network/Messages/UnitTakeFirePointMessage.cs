using UnityEngine;

namespace FracturedState.Game.Network
{
    public class UnitTakeFirePointMessage : ILockStepMessage
    {
        public uint Id { get; set; }
        UnitManager unit;
        Transform point;

        public UnitTakeFirePointMessage(UnitManager unit, Transform point)
        {
            this.unit = unit;
            this.point = point;
        }

        public void Process()
        {
            unit.OnTakeFirePointIssued(point);
        }
    }
}