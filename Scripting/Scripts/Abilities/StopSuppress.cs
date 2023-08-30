using FracturedState.Game.AI;
using FracturedState.Game.Data;

namespace FracturedState.Scripting
{
    public class StopSuppress : SelfAbility
    {
        public const string AbilityName = "StopSuppress";
        
        public StopSuppress(UnitManager caster, Ability ability) : base(caster, ability) { }

        public override void ExecuteAbility()
        {
            if (caster == null || !caster.HasAbility(AbilityName) || !caster.Data.CanSuppress) return;

            caster.RemoveAbility(AbilityName);
            caster.AddAbility(Suppress.AbilityName);
            caster.AcceptInput = true;

            var state = caster.CurrentCover != null ? new UnitIdleCoverState(caster) : new UnitIdleState(caster);
            caster.StateMachine.ChangeState(state);
            
            if (SelectionManager.Instance.SelectedUnits.Contains(caster))
            {
                SelectionManager.Instance.OnSelectionChanged.Invoke();
            }
        }
    }
}