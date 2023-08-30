namespace FracturedState.Game.AI
{
    public class UnitEnterStructureAttackState : UnitEnterStructureState
    {
        private readonly UnitManager _target;
        
        public UnitEnterStructureAttackState(UnitManager owner, StructureManager currentStructure, UnitManager target) 
            : base(owner, currentStructure)
        {
            _target = target;
        }

        public UnitEnterStructureAttackState(UnitManager owner, StructureManager currentStructure,
            StructureManager newStructure, UnitManager target)
            : base(owner, currentStructure, newStructure)
        {
            _target = target;
        }

        public override void Execute()
        {
            if ((Owner.IsMine || Owner.AISimulate))
            {
                var theStructure = newStructure ?? currentStructure;
                // unit died before we got there so just enter structure
                if (_target == null || !_target.IsAlive)
                {
                    Owner.NetMsg.CmdEnterStructure(theStructure.GetComponent<Identity>().UID);
                    Owner.StateMachine.ChangeState(new UnitPendingState(Owner));
                    return;
                }
                // unit isn't in the structure we're going to anymore
                if (_target.CurrentStructure != theStructure)
                {
                    // just re-issue target command to start eval over again
                    Owner.NetMsg.CmdSetTarget(_target.NetMsg.NetworkId);
                    Owner.StateMachine.ChangeState(new UnitPendingState(Owner));
                    return;
                }
            }
            base.Execute();
        }
    }
}