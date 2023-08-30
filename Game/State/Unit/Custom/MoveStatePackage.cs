using UnityEngine;

namespace FracturedState.Game.AI
{
    public class MoveStatePackage : ICustomStatePackage
    {
        public UnitManager Owner { get; }
        public Vector3 Destination { get; }

        public MoveStatePackage(UnitManager owner, Vector3 destination)
        {
            Owner = owner;
            Destination = destination;
        }
    }
}