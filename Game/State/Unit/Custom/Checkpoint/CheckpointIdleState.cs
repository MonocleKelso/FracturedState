using FracturedState.Game.Management;
using UnityEngine;

namespace FracturedState.Game.AI
{
    public class CheckpointIdleState : CustomState
    {
        private readonly bool _isDeployed;
        
        public CheckpointIdleState(IdleStatePackage initPackage) : base(initPackage)
        {
            _isDeployed = owner.LocoMotor == null;
        }

        public override void Enter()
        {
            owner.IsIdle = true;
            if (owner.AnimControl != null)
            {
                owner.AnimControl.Stop();
                owner.AnimControl.Rewind();
            }
            owner.EffectManager?.PlayIdleSystems();
            owner.transform.GetChildByName("hotBarrels").GetComponent<ParticleSystem>().Stop();
        }
        
        public override void Execute()
        {
            if (!_isDeployed)
            {
                if (owner.AnimControl != null && !owner.AnimControl.isPlaying && owner.Data.Animations?.Idle != null && owner.Data.Animations.Idle.Length > 0)
                {
                    owner.AnimControl.Play(owner.Data.Animations.Idle[Random.Range(0, owner.Data.Animations.Idle.Length)], PlayMode.StopAll);
                }

                return;
            }
            
            if ((owner.IsMine || owner.AISimulate) && owner.Squad != null && owner.ContextualWeapon != null)
            {
                UnitManager target = null;
                var dist = float.MaxValue;
                var visible = owner.Squad.GetVisibleForUnit(owner);
                if (visible == null) return;
                
                for (var i = 0; i < visible.Count; i++)
                {
                    var v = visible[i];
                    if (v != null && v.IsAlive && !(owner.WorldState == Nav.State.Interior && v.Data.IsGarrisonUnit) && VisibilityChecker.Instance.HasSight(owner, v))
                    {
                        var toUnit = (v.transform.position - owner.transform.position);
                        var mag = toUnit.magnitude;
                    
                        if (mag > owner.ContextualWeapon.Range) continue;
                        if (mag > dist) continue;
                        
                        // also check orientation because checkpoints have limited rotation to face target
                        if (Vector3.Dot(owner.transform.forward, toUnit.normalized) < 0.25f) continue;
                            
                        target = v;
                        dist = toUnit.sqrMagnitude;
                    }
                }
                if (target != null && target.NetMsg != null)
                {
                    owner.NetMsg.CmdSetTarget(target.NetMsg.NetworkId);
                    owner.StateMachine.ChangeState(new UnitPendingState(owner));
                }
            }
        }
        
        public override void Exit()
        {
            owner.IsIdle = false;
            owner.EffectManager?.StopCurrentSystem();
        }
    }
}