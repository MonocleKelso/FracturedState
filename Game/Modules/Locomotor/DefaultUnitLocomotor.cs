using System.Collections.Generic;
using FracturedState.Game.Data;
using UnityEngine;

namespace FracturedState.Game.Modules
{
    public class DefaultUnitLocomotor : Locomotor
    {
        private const float UnitAvoidForce = 0.8f;
        
        protected float Speed;
        
        public DefaultUnitLocomotor(UnitManager owner) : base(owner)
        {
            Speed = Owner.Squad?.MoveSpeed ?? Owner.Data.Physics.MaxSpeed;
        }

        public override int MoveOnPath(List<Vector3> path, int currentIndex)
        {
            var point = path[currentIndex];
            var toPoint = ArriveToPoint(point);
            var unit = GetCollidingUnit();
            if (unit != null)
            {
                var avoid = (toPoint - unit.transform.position).normalized * UnitAvoidForce;
                CurrentVelocity = Vector3.ClampMagnitude(CurrentVelocity + toPoint + avoid, Speed);
                Owner.transform.position += CurrentVelocity * Time.deltaTime;
            }
            else
            {
                Move(toPoint);
            }

            if (Mathf.Approximately(CurrentVelocity.magnitude, 0))
            {
                Owner.AnimControl.Stop();
            }
            else if (!Owner.AnimControl.isPlaying)
            {
                if (Owner.AnimControl != null && Owner.Data.Animations?.Move != null && Owner.Data.Animations.Move.Length > 0)
                {
                    var anim = Owner.Data.Animations.Move[Random.Range(0, Owner.Data.Animations.Move.Length)];
                    var len = Owner.AnimControl[anim].length;
                    Owner.AnimControl[anim].time = Random.Range(0, len);
                    Owner.AnimControl.Play(anim);
                }
            }
            
            if ((Owner.transform.position - point).sqrMagnitude <= ConfigSettings.Instance.Values.CloseEnoughThreshold)
            {
                return currentIndex + 1;
            }
            return currentIndex;
        }

        protected virtual void Move(Vector3 steering)
        {
            CurrentVelocity = Vector3.ClampMagnitude(CurrentVelocity + steering, Speed);
            var lookAt = Owner.transform.position + CurrentVelocity;
            lookAt.y = 0;
            Owner.transform.LookAt(lookAt);
            Owner.transform.position += CurrentVelocity * Time.deltaTime;
        }
        
        protected Collider GetCollidingUnit()
        {
            var distance = CurrentVelocity.magnitude;
            var mask = Owner.WorldState == Nav.State.Exterior ? GameConstants.ExteriorUnitAllMask : GameConstants.InteriorUnitAllMask;
            RaycastHit hit;
            if (Physics.Raycast(new Ray(Owner.transform.position, CurrentVelocity.normalized), out hit, distance, mask))
            {
                if (hit.transform.GetAbsoluteParent() == Owner.transform)
                {
                    return null;
                }
                return hit.collider;
            }
            return null;
        }
        
        protected virtual Vector3 ArriveToPoint(Vector3 point)
        {
            var toTarget = point - Owner.transform.position;
            var normToTarget = toTarget.normalized;
            var desiredVelocity = normToTarget * Speed * (toTarget.magnitude / ConfigSettings.Instance.Values.CloseEnoughThreshold);
            return Vector3.ClampMagnitude(desiredVelocity - CurrentVelocity, Speed);
        }
    }
}