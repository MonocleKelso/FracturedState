using UnityEngine;

namespace FracturedState.Game.Network
{
     public class UnitTakeCoverMessage : ILockStepMessage
    {
        public uint Id { get; set; }
        UnitManager unit;
        CoverManager cover;
        Transform point;

        public UnitTakeCoverMessage(UnitManager unit, CoverManager cover, Transform point)
        {
            this.unit = unit;
            this.cover = cover;
            this.point = point;
        }

        public void Process()
        {
            unit.OnTakeCoverPointIssued(cover, point);
        }
    }
}
