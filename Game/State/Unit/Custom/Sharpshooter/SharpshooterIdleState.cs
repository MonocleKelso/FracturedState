using FracturedState.Game.Management;
using UnityEngine;

namespace FracturedState.Game.AI
{
    public class SharpshooterIdleState : CustomState
    {
        private readonly bool _isDeployed;
        
        public SharpshooterIdleState(IdleStatePackage initPackage) : base(initPackage)
        {
            _isDeployed = owner.LocoMotor == null;
        }

        public override void Enter()
        {
            owner.IsIdle = true;
            owner.AnimControl.Stop();
            owner.AnimControl.Rewind();
            owner.EffectManager?.PlayIdleSystems();
        }

        public override void Execute()
        {
            if (!_isDeployed)
            {
                owner.AnimControl.Play(owner.Data.Animations.Idle[Random.Range(0, owner.Data.Animations.Idle.Length)], PlayMode.StopAll);
            }
            
            if ((owner.IsMine || owner.AISimulate) && owner.Squad != null && owner.ContextualWeapon != null)
            {
                UnitManager target = null;
                var dist = float.MaxValue;
                var visible = owner.Squad.GetVisibleForUnit(owner);
                if (visible == null) return;
                
                for (var i = 0; i < visible.Count; i++)
                {
                    if (visible[i] != null && visible[i].IsAlive && !(owner.WorldState == Nav.State.Interior && visible[i].Data.IsGarrisonUnit) && VisibilityChecker.Instance.HasSight(owner, visible[i]))
                    {
                        var toUnit = (visible[i].transform.position - owner.transform.position);
                        if (!(toUnit.sqrMagnitude < dist)) continue;
                            
                        target = visible[i];
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