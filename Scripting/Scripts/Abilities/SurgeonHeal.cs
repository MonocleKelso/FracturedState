using FracturedState.Game.AI;
using FracturedState.Game.Data;

namespace FracturedState.Scripting
{
    public class SurgeonHeal : TargetAbility, IMonitorAbility
    {
        IState healState;

        public SurgeonHeal(UnitManager caster, UnitManager target, Ability ability) : base(caster, target, ability) { }

        public override void ExecuteAbility()
        {
            if (caster == null || target == null) return;
            
            if ((caster.transform.position - target.transform.position).magnitude <= 3)
            {
                healState = new SurgeonHealState(caster, target);
            }
            else
            {
                healState = new SurgeonHealMoveState(caster, target);
            }
            healState.Enter();
        }

        void IMonitorAbility.Update()
        {
            if (target == null || !target.IsAlive || (target.DamageProcessor != null && target.DamageProcessor.CurrentHealth == target.DamageProcessor.MaxHealth))
            {
                caster.StateMachine.ChangeState(new UnitIdleState(caster));
            }
            else
            {
                healState.Execute();
            }
        }

        public void Finish()
        {
            healState?.Exit();
        }
    }
}