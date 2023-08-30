using System.Collections.Generic;
using System.Runtime.InteropServices;
using FracturedState.Game.Data;
using FracturedState.ModTools;
using UnityEngine;

namespace FracturedState.Game.Modules
{
    public class VehicleLocomotor : DefaultUnitLocomotor
    {
        protected const float CloseEnough = 0.5f;
        
        protected readonly float TurnRate;

        private bool lastPoint;
        private Vector3 steerPosition = Vector3.zero;
        
        public VehicleLocomotor(UnitManager owner) : base(owner)
        {
            TurnRate = Owner.Data.Physics.TurnRate;
        }

        public override int MoveOnPath(List<Vector3> path, int currentIndex)
        {
            var point = path[currentIndex];
            lastPoint = currentIndex == path.Count - 1;
            var returnIndex = currentIndex;

            if (steerPosition == Vector3.zero)
            {
                steerPosition = Owner.transform.position + Owner.transform.forward * TurnRate;
            }
            
            // if this isn't the last point and we're closer to the point than our turn radius use the next point on the path
            if (!lastPoint && (steerPosition - point).magnitude < TurnRate)
            {
                returnIndex = currentIndex + 1;
                point = path[returnIndex];
            }
            
            // calculate steering for ahead position
            var steering = ArriveToPoint(steerPosition, point);
            steerPosition += steering * Time.deltaTime;
            // check that new steering position doesn't cause a collision
            RaycastHit hit;
            if (Physics.CapsuleCast(Owner.transform.position, steerPosition, Owner.Data.Physics.PathRadius,
                (steerPosition - Owner.transform.position).normalized, out hit, TurnRate * Time.deltaTime, GameConstants.ExteriorMask))
            {
                // calculate new steering position based on bounds intersection
                var p = hit.point - hit.collider.transform.position;
                p = p.normalized * Owner.Data.Physics.PathRadius;
                steerPosition = hit.point + p;
            }
            steerPosition.y = 0;
            
            var vSteering = ArriveToPoint(Owner.transform.position, steerPosition);
            Move(vSteering);

            if (returnIndex != currentIndex)
            {
                return returnIndex;
            }
            
            if ((Owner.transform.position - point).sqrMagnitude <= ConfigSettings.Instance.Values.CloseEnoughThreshold)
            {
                return currentIndex + 1;
            }
            return currentIndex;
        }

        protected override void Move(Vector3 steering)
        {
            base.Move(steering);
            steerPosition += CurrentVelocity * Time.deltaTime;
        }

        protected Vector3 ArriveToPoint(Vector3 start, Vector3 point)
        {
            var toTarget = point - start;
            var normToTarget = toTarget.normalized;
            var desiredVelocity = normToTarget * Speed;
            return Vector3.ClampMagnitude(desiredVelocity - CurrentVelocity, Speed);
        }

        public override void ZeroVelocity()
        {
            base.ZeroVelocity();
            if (Owner != null) steerPosition = Vector3.zero;
        }
    }
}