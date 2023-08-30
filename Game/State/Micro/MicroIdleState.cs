using FracturedState.Game.Management;
using System.Collections.Generic;
using UnityEngine;

namespace FracturedState.Game.AI
{
    public class MicroIdleState : MicroBaseState
    {
        public MicroIdleState(UnitManager owner) : base(owner) { }

        public override void Enter()
        {
            // swap to custom idle state if one is declared
            if (!string.IsNullOrEmpty(owner.Data.CustomBehaviours?.IdleClassName) && owner.StateMachine.CurrentState == this)
            {
                var state = CustomStateFactory<IdleStatePackage>.Create(owner.Data.CustomBehaviours.IdleClassName, new IdleStatePackage(owner));
                owner.StateMachine.ChangeState(state);
                return;
            }
            
            owner.IsIdle = true;
            if (owner.AnimControl != null)
            {
                owner.AnimControl.Stop();
                owner.AnimControl.Rewind();
            }

            owner.EffectManager?.PlayIdleSystems();
            
            if (owner.IsMine && owner.CurrentCover == null)
            {
                var target = owner.DetermineTarget(null);
                if (target != null)
                {
                    owner.NetMsg.CmdSetTarget(target.NetMsg.NetworkId);
                }
            }
        }

        public override void Execute()
        {
            if (owner.AnimControl != null && !owner.AnimControl.isPlaying && owner.Data.Animations != null && owner.Data.Animations.Idle != null && owner.Data.Animations.Idle.Length > 0)
            {
                owner.AnimControl.Play(owner.Data.Animations.Idle[Random.Range(0, owner.Data.Animations.Idle.Length)], PlayMode.StopAll);
            }
            // search for nearby visible enemies to attack
            if ((owner.IsMine || owner.AISimulate) && owner.Squad != null && owner.ContextualWeapon != null)
            {
                UnitManager target = null;
                var dist = float.MaxValue;
                var visible = owner.Squad.GetVisibleForUnit(owner);
                if (visible != null)
                {
                    for (var i = 0; i < visible.Count; i++)
                    {
                        if (visible[i] != null && visible[i].IsAlive && !(owner.WorldState == Nav.State.Interior && visible[i].Data.IsGarrisonUnit) && VisibilityChecker.Instance.HasSight(owner, visible[i]))
                        {
                            var toUnit = (visible[i].transform.position - owner.transform.position);
                            if (toUnit.sqrMagnitude < dist)
                            {
                                target = visible[i];
                                dist = toUnit.sqrMagnitude;
                            }
                        }
                    }
                    if (target != null && target.NetMsg != null)
                    {
                        owner.NetMsg.CmdSetTarget(target.NetMsg.NetworkId);
                        owner.StateMachine.ChangeState(new UnitPendingState(owner));
                    }
                }
            }
        }

        public override void Exit()
        {
            base.Exit();
            owner.IsIdle = false;
            owner.EffectManager?.StopCurrentSystem();
        }
    }
}