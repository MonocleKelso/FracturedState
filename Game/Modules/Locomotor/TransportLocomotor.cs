using System.Collections.Generic;
using System.Timers;
using FracturedState.Game.Data;
using UnityEngine;

namespace FracturedState.Game.Modules
{
    public class TransportLocomotor : DefaultUnitLocomotor
    {
        private const float HalfLength = 8f;

        private bool reverse;

        public TransportLocomotor(UnitManager owner) : base(owner) { }
        
        public override int MoveOnPath(List<Vector3> path, int currentIndex)
        {
            var point = path[currentIndex];
            var speed = Owner.Data.Physics.MaxSpeed; 

            var facing = Vector3.Dot(Owner.transform.forward, (point - Owner.transform.position).normalized);

            if (facing < 0)
            {
                var r = Vector3.RotateTowards(Owner.transform.forward, (point - Owner.transform.position).normalized,
                    Owner.Data.Physics.TurnRate * Time.deltaTime, 0);
                
                var revRot = Vector3.RotateTowards(Owner.transform.forward, (Owner.transform.position - point).normalized,
                    Owner.Data.Physics.TurnRate * Time.deltaTime, 0);
                
                // if rotating towards our path point means we clip into a static object
                if (Physics.CapsuleCast(Owner.transform.position, Owner.transform.position + r,
                    Owner.Data.Physics.TurnRate, r, HalfLength, GameConstants.ExteriorMask) ||
                    Physics.CapsuleCast(Owner.transform.position, Owner.transform.position + revRot,
                        Owner.Data.Physics.TurnRate, revRot, HalfLength, GameConstants.ExteriorMask))
                {
                    reverse = true;
                }
                
                if (reverse)
                {
                    Owner.transform.rotation = Quaternion.LookRotation(revRot);
                    Owner.transform.position = 
                        Vector3.MoveTowards(Owner.transform.position, point, (speed * (facing * -1)) * Time.deltaTime);
                    
                    if ((Owner.transform.position - point).sqrMagnitude <= ConfigSettings.Instance.Values.CloseEnoughThreshold)
                    {
                        reverse = false;
                        return currentIndex + 1;
                    }
                    
                    return currentIndex;
                }

                Owner.transform.rotation = Quaternion.LookRotation(r);
                return currentIndex;
            }
            
            // if we're not totally facing the destination then rotate towards it
            if (facing < 1)
            {
                var r = Vector3.RotateTowards(Owner.transform.forward, (point - Owner.transform.position).normalized,
                    Owner.Data.Physics.TurnRate * Time.deltaTime, 0);

                var rot = Quaternion.LookRotation(r);
                Owner.transform.rotation = rot;
            }

            // if we're less than perpendicular to the destination then move towards it
            if (facing > 0)
            {
                Owner.transform.position = 
                    Vector3.MoveTowards(Owner.transform.position, point, speed * facing * Time.deltaTime);
            }

            return (Owner.transform.position - point).sqrMagnitude <= ConfigSettings.Instance.Values.CloseEnoughThreshold ? currentIndex + 1 : currentIndex;
        }
    }
}