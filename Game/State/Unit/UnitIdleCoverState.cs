using UnityEngine;

namespace FracturedState.Game.AI
{
    public class UnitIdleCoverState : UnitIdleState
    {

        public UnitIdleCoverState(UnitManager owner)
            : base(owner){ }

        public override void Enter()
        {
            if (Owner.AnimControl != null)
            {
                Owner.AnimControl.Stop();
                Owner.AnimControl.Rewind();
                
                if (Owner.CurrentCoverPoint.Stance == Data.CoverPointStance.Stand)
                {
                    if (Owner.Data.Animations.StandAim != null && Owner.Data.Animations.StandAim.Length > 0)
                    {
                        Owner.AnimControl.Play(Owner.Data.Animations.StandAim[Random.Range(0, Owner.Data.Animations.StandAim.Length)], PlayMode.StopAll);
                    }
                }
                else
                {
                    if (Owner.Data.Animations.CrouchAim != null && Owner.Data.Animations.CrouchAim.Length > 0)
                    {
                        Owner.AnimControl.Play(Owner.Data.Animations.CrouchAim[Random.Range(0, Owner.Data.Animations.CrouchAim.Length)], PlayMode.StopAll);
                    }
                }
            }

            Owner.IsIdle = true;
            if (Owner.IsMine || Owner.AISimulate)
            {
                var target = Owner.DetermineTarget(null);
                if (target != null)
                {
                    Owner.NetMsg.CmdSetTarget(target.NetMsg.NetworkId);
                    Owner.StateMachine.ChangeState(new UnitPendingState(Owner));
                }
            }
        }
    }
}