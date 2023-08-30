using FracturedState.Game.Data;
using UnityEngine;

namespace FracturedState.Game.AI
{
    public class SurgeonHealMoveState : UnitMoveState
    {
        static string[] barks = new string[1] { "Laperia/Ability/SurgeonHeal01" };

        protected UnitManager target;

        public SurgeonHealMoveState(UnitManager owner, UnitManager target) : base(owner)
        {
            this.target = target;
        }

        public override void Enter()
        {
            Destination = target.transform.position;
            base.Enter();
            if (Owner.IsMine)
                UnitBarkManager.Instance.Random2DBark(barks);
        }

        public override void Execute()
        {
            // we monitor here in case the units run past each other so the surgeon doesn't path to an old location
            if (Owner != null && target != null && (Owner.transform.position - target.transform.position).sqrMagnitude <= 1.5f)
            {
                Owner.StateMachine.ChangeState(new SurgeonHealState(Owner, target));
                return;
            }
            base.Execute();
        }
    }
}