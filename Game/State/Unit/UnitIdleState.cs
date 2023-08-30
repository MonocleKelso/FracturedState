using FracturedState.Game.Management;
using FracturedState.Game.Network;
using System.Collections.Generic;
using UnityEngine;

namespace FracturedState.Game.AI
{
    public class UnitIdleState : UnitBaseState
    {
        private bool idled;
        
        public UnitIdleState(UnitManager owner) : base(owner) { }

        public override void Enter()
        {
            // swap to custom idle state if one is declared
            if (!string.IsNullOrEmpty(Owner.Data.CustomBehaviours?.IdleClassName) && Owner.StateMachine.CurrentState == this)
            {
                var state = CustomStateFactory<IdleStatePackage>.Create(Owner.Data.CustomBehaviours.IdleClassName, new IdleStatePackage(Owner));
                Owner.StateMachine.ChangeState(state);
                return;
            }
            
            Owner.EffectManager?.PlayIdleSystems();
            if (!FracNet.Instance.IsHost) return;
            
            if (Owner.Squad != null && Owner.Squad.UseFacing && Owner.CurrentCover == null && Owner.Data.CanTakeCover)
            {
                Owner.DoCoverCheck();
                if (Owner.StateMachine.CurrentState == this && Owner.ContextualWeapon != null)
                {
                    var target = Owner.DetermineTarget(null);
                    if (target != null)
                    {
                        Owner.NetMsg.CmdSetTarget(target.NetMsg.NetworkId);
                        Owner.StateMachine.ChangeState(new UnitPendingState(Owner));
                    }
                }
            }
            else  if (Owner.CurrentCover == null && Owner.ContextualWeapon != null)
            {
                var target = Owner.DetermineTarget(null);
                if (target != null)
                {
                    Owner.NetMsg.CmdSetTarget(target.NetMsg.NetworkId);
                    Owner.StateMachine.ChangeState(new UnitPendingState(Owner));
                }
                else if (Owner.WorldState == Nav.State.Exterior)
                {
                    Owner.DoCoverCheck();
                }
            }
        }

        public override void Execute()
        {
            if (!Owner.InCover)
            {
                // do this here so we can wait for initial target eval in Enter() and prevent visual wonkiness
                if (!idled)
                {
                    idled = true;
                    Owner.IsIdle = true;
                    if (Owner.AnimControl != null)
                    {
                        Owner.AnimControl.Stop();
                        Owner.AnimControl.Rewind();
                    }
                }
                
                if (Owner.AnimControl != null && !Owner.AnimControl.isPlaying && Owner.Data.Animations?.Idle != null &&
                    Owner.Data.Animations.Idle.Length > 0)
                {
                    Owner.AnimControl.Play(Owner.Data.Animations.Idle[Random.Range(0, Owner.Data.Animations.Idle.Length)], PlayMode.StopAll);
                }
            }
            
            // search for nearby visible enemies to attack
            if ((Owner.IsMine || Owner.AISimulate) && Owner.Squad != null && Owner.ContextualWeapon != null)
            {
                UnitManager target = null;
                var dist = float.MaxValue;
                var visible = Owner.Squad.GetVisibleForUnit(Owner);
                if (visible == null) return;
                
                for (var i = 0; i < visible.Count; i++)
                {
                    if (visible[i] != null && visible[i].IsAlive && !(Owner.WorldState == Nav.State.Interior && visible[i].Data.IsGarrisonUnit) && VisibilityChecker.Instance.HasSight(Owner, visible[i]))
                    {
                        var toUnit = (visible[i].transform.position - Owner.transform.position);
                        if (!(toUnit.sqrMagnitude < dist)) continue;
                            
                        target = visible[i];
                        dist = toUnit.sqrMagnitude;
                    }
                }
                if (target != null && target.NetMsg != null)
                {
                    Owner.NetMsg.CmdSetTarget(target.NetMsg.NetworkId);
                    Owner.StateMachine.ChangeState(new UnitPendingState(Owner));
                }
            }
        }

        public override void Exit()
        {
            Owner.IsIdle = false;
            Owner.EffectManager?.StopCurrentSystem();
        }
    }
}