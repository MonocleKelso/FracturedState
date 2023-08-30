using System.Collections;
using UnityEngine;

namespace FracturedState.Game.Network
{
    public class UnitSyncLocationMessage : ILockStepMessage
    {
        public uint Id { get; }
        private readonly UnitManager unit;
        private readonly Vector3 destination;

        public UnitSyncLocationMessage(uint id, UnitManager unit, Vector3 destination)
        {
            Id = id;
            this.unit = unit;
            this.destination = destination;
        }
        
        public void Process()
        {
            unit.StartCoroutine(Smooth());
        }

        private IEnumerator Smooth()
        {
            var origPos = unit.transform.position;
            
            var p = 0f;
            while (p < 1 && unit != null)
            {
                unit.transform.position = Vector3.Lerp(origPos, destination, p);
                p += Time.deltaTime * 10;
                yield return null;
            }
        }
    }
}