using UnityEngine;
using FracturedState.Game.Management;
using FracturedState.Game.Data;

namespace FracturedState.Game.AI
{
    public class UnitFirePointIdleState : UnitBaseState
    {
        private readonly Transform firePoint;
        private readonly Weapon weapon;

        public UnitFirePointIdleState(UnitManager owner, Transform firePoint)
            : base(owner)
        {
            this.firePoint = firePoint;
            Owner.transform.rotation = this.firePoint.rotation;
            weapon = Owner.ContextualWeapon;
        }

        public override void Enter()
        {
            if (Owner.AnimControl != null && Owner.Data.Animations.CrouchAim != null && Owner.Data.Animations.CrouchAim.Length > 0)
            {
                Owner.AnimControl.Play(Owner.Data.Animations.CrouchAim[Random.Range(0, Owner.Data.Animations.CrouchAim.Length)], PlayMode.StopAll);
            }
            Owner.IsIdle = true;
        }

        public override void Execute()
        {
            if (!Owner.IsMine && !Owner.AISimulate) return;
            
            var visible = Owner.Squad.GetVisibleForUnit(Owner);
            if (visible == null) return;
            
            var dist = float.MaxValue;
            UnitManager target = null;
            for (var i = 0; i < visible.Count; i++)
            {
                if (visible[i].IsFriendly) continue;
                        
                if (visible[i] != null && VisibilityChecker.Instance.HasSight(Owner, visible[i]))
                {
                    var toUnit = (visible[i].transform.position - Owner.transform.position);
                    var inRange = toUnit.magnitude < weapon.Range;
                    var inVision = Vector3.Dot(firePoint.forward, toUnit.normalized) > ConfigSettings.Instance.Values.FirePointVisionThreshold;

                    if (inRange && (inVision || (visible[i].WorldState == Nav.State.Interior && visible[i].CurrentStructure == Owner.CurrentStructure)) && toUnit.sqrMagnitude < dist)
                    {
                        target = visible[i];
                        dist = toUnit.sqrMagnitude;
                    }
                }
            }
            if (target != null)
            {
                Owner.NetMsg.CmdSetTarget(target.NetMsg.NetworkId);
                Owner.StateMachine.ChangeState(new UnitPendingState(Owner));
            }
        }
    }
}