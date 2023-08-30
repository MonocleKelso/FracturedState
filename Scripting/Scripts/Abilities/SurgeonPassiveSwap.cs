using FracturedState.Game.AI;
using FracturedState.Game.Data;

namespace FracturedState.Scripting
{
    public class SurgeonPassiveSwap : SelfAbility
    {
        public SurgeonPassiveSwap(UnitManager caster, Ability ability) : base(caster, ability) { }

        public override void ExecuteAbility()
        {
            if (caster.HasAbility(SurgeonIdleState.HealAbilityName))
            {
                caster.RemoveAbility(SurgeonIdleState.HealAbilityName);
                caster.AddAbility(SurgeonIdleState.AttackAbilityName);
                caster.UseAbility(SurgeonIdleState.AttackAbilityName);
                if (caster.IsMine)
                {
                    UnitBarkManager.Instance.AbilityBark(caster.GetAbility(SurgeonIdleState.AttackAbilityName));
                }
            }
            else
            {
                caster.RemoveAbility(SurgeonIdleState.AttackAbilityName);
                caster.AddAbility(SurgeonIdleState.HealAbilityName);
                caster.UseAbility(SurgeonIdleState.HealAbilityName);
                if (caster.IsMine)
                {
                    UnitBarkManager.Instance.AbilityBark(caster.GetAbility(SurgeonIdleState.HealAbilityName));
                }
            }
            // hacky method to redraw skill bar
            SelectionManager.Instance.OnSelectionChanged.Invoke();
            
            if (caster.Squad != null && (caster.Squad.LastMovePosition - caster.transform.position).magnitude > 10)
            {
                caster.StateMachine.ChangeState(new UnitMoveState(caster, caster.Squad.LastMovePosition));
            }
            else
            {
                caster.StateMachine.ChangeState(new UnitIdleState(caster));
            }
        }
    }
}