using UnityEngine;

namespace FracturedState.Game.AI
{
    public class UnitExitStructureAttackState : UnitExitStructureState
    {
        private UnitManager _target;
        
        public UnitExitStructureAttackState(UnitManager owner, UnitManager target)
            : base(owner, target.transform.position)
        {
            _target = target;
        }

        protected override void OnArrival()
        {
            base.OnArrival();
            if (_target != null && _target.IsAlive)
            {
                Owner.StateMachine.ChangeState(new UnitAttackState(Owner, _target));
            }
        }
    }
}