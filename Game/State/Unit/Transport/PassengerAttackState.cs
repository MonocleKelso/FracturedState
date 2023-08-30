using FracturedState.Game.Data;
using FracturedState.Game.Management;
using UnityEngine;

namespace FracturedState.Game.AI
{
    public class PassengerAttackState : UnitAttackState
    {
        private readonly Transform passengerSlot;

        public PassengerAttackState(UnitManager owner, Transform passengerSlot, UnitManager target)
            : base(owner, target)
        {
            this.passengerSlot = passengerSlot;
        }

        public override void Enter()
        {
            // switch to custom passenger attack state if necessary
            if (!string.IsNullOrEmpty(Owner.Data.CustomBehaviours?.PassengerAttackClassName) && Owner.StateMachine.CurrentState == this)
            {
                var state = CustomStateFactory<AttackStatePackage>.Create(Owner.Data.CustomBehaviours.PassengerAttackClassName, new AttackStatePackage(Owner, target));
                Owner.StateMachine.ChangeState(state);
            }
            else
            {
                base.Enter();
            }
        }

        public override void Execute()
        {
            if (Owner.Transport != null)
            {
                Owner.transform.position = passengerSlot.position;
            }
            else
            {
                Owner.StateMachine.ChangeState(new PassengerIdleState(Owner, Owner.Transport, passengerSlot));
                return;
            }
            
            if (target != null && target.IsAlive && VisibilityChecker.Instance.HasSight(Owner, target))
            {
                var toTarget = (target.transform.position - Owner.transform.position);
                var inRange = toTarget.magnitude < weapon.Range;
                var canFire = Owner.LastFiredTime + weapon.FireRate < Time.time;
                var inVision = Vector3.Dot(passengerSlot.forward, toTarget.normalized) > ConfigSettings.Instance.Values.FirePointVisionThreshold;

                if (inRange && inVision)
                {
                    Owner.transform.LookAt(target.transform.position);
                    if (canFire)
                        Shoot();
                }
                else
                {
                    Owner.StateMachine.ChangeState(new PassengerIdleState(Owner, Owner.Transport, passengerSlot));
                }
            }
            else
            {
                Owner.StateMachine.ChangeState(new PassengerIdleState(Owner, Owner.Transport, passengerSlot));
            }
        }
    }
}