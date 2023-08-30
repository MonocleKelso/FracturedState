using FracturedState.Game.AI;
using FracturedState.Game.Data;
using UnityEngine;

namespace FracturedState.Scripting
{
    public class FocusFire : TargetAbility, IMonitorAbility
    {
        private const float duration = 5f;
        private float startTime;
        private IState state;

        public FocusFire(UnitManager caster, UnitManager target, Ability ability) : base(caster, target, ability) { }

        public override void ExecuteAbility()
        {
            base.ExecuteAbility();
            // not allowed to target across world states unless garrisoned
            if (caster.CurrentFirePoint == null && caster.WorldState != target.WorldState)
            {
                caster.StateMachine.ChangeState(new UnitIdleState(caster));
                return;
            }

            startTime = Time.time;
            if (caster.CurrentFirePoint != null)
            {
                state = new UnitFirePointAttackState(caster, caster.CurrentFirePoint, target);
            }
            else
            {
                state = new UnitAttackState(caster, target);
            }
            
            state.Enter();
        }

        public void Update()
        {
            if (Time.time - startTime < duration)
            {
                state.Execute();
            }
            else
            {
                if (caster.CurrentFirePoint != null)
                {
                    caster.StateMachine.ChangeState(new UnitFirePointIdleState(caster, caster.CurrentFirePoint));
                }
                else if (caster.InCover)
                {
                    caster.StateMachine.ChangeState(new UnitIdleCoverState(caster));
                }
                else
                {
                    caster.StateMachine.ChangeState(new UnitIdleState(caster));
                }
            }
        }

        void IMonitorAbility.Finish() { }
    }
}