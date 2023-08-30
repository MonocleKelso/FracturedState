namespace FracturedState.Game.AI
{
    public class UnitGarrisonIdleState : UnitIdleState
    {
        public UnitGarrisonIdleState(UnitManager owner) : base(owner) { }

        public override void Enter()
        {
            Owner.IsIdle = true;
            if (Owner.AnimControl != null)
            {
                Owner.AnimControl.Stop();
                Owner.AnimControl.Rewind();
            }

            Owner.EffectManager?.PlayIdleSystems();
            
            if (Owner.IsMine || Owner.AISimulate)
            {
                if (Owner.ContextualWeapon != null)
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
}