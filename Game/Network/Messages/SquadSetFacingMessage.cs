using System;
using FracturedState.Game.AI;
using UnityEngine;

namespace FracturedState.Game.Network
{
    public class SquadSetFacingMessage : ILockStepMessage
    {
        public uint Id { get; set; }
        UnitManager unit;
        Quaternion facing;

        public SquadSetFacingMessage(UnitManager unit, Quaternion facing)
        {
            this.unit = unit;
            this.facing = facing;
        }

        public void Process()
        {
            if (unit != null && unit.Squad != null)
            {
                unit.Squad.SetFacing(facing);
            }
        }
    }
}