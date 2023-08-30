using UnityEngine;
using FracturedState.Game.Data;
using FracturedState.Game.Management;

namespace FracturedState.Game.AI
{
    public class UnitFirePointAttackState : UnitAttackState
    {
        private readonly Transform firePoint;

        public UnitFirePointAttackState(UnitManager owner, Transform firePoint, UnitManager target)
            : base(owner, target)
        {
            this.firePoint = firePoint;
        }

        public override void Execute()
        {
            if (target != null && target.IsAlive && VisibilityChecker.Instance.HasSight(Owner, target))
            {
                var toTarget = (target.transform.position - Owner.transform.position);
                var inRange = toTarget.magnitude < weapon.Range;
                var canFire = (Owner.LastFiredTime + weapon.FireRate) - weapon.FireRate * ConfigSettings.Instance.Values.GarrisonROFBonus < Time.time;
                var inVision = Vector3.Dot(firePoint.forward, toTarget.normalized) > ConfigSettings.Instance.Values.FirePointVisionThreshold;

                if (inRange && (inVision || (target.WorldState == Nav.State.Interior && target.CurrentStructure == Owner.CurrentStructure)))
                {
                    Owner.transform.LookAt(target.transform.position);
                    if (canFire)
                    {
                        Shoot();
                    }
                }
                else
                {
                    Owner.StateMachine.ChangeState(new UnitFirePointIdleState(Owner, firePoint));
                }
            }
            else
            {
                Owner.StateMachine.ChangeState(new UnitFirePointIdleState(Owner, firePoint));
            }
        }

        protected override void Shoot()
        {
            base.Shoot();
            if (Owner.FirePointMuzzleFlash != null)
            {
                Owner.FirePointMuzzleFlash.transform.LookAt(target.transform);
                var rot = Owner.FirePointMuzzleFlash.transform.rotation.eulerAngles;
                rot.x = 0;
                Owner.FirePointMuzzleFlash.transform.rotation = Quaternion.Euler(rot);
                Owner.FirePointMuzzleFlash.Play();
            }
        }
    }
}