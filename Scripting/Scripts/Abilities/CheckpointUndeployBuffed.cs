using FracturedState.Game.AI;
using FracturedState.Game.Data;

namespace FracturedState.Scripting
{
    public class CheckpointUndeployBuffed : UnitUndeployAbility, IMonitorAbility
    {
        public const string AbilityName = "CheckpointUndeployBuffed";
        
        public CheckpointUndeployBuffed(UnitManager caster, Ability ability) : base(caster, ability)
        {
        }

        public override void ExecuteAbility()
        {
            base.ExecuteAbility();
            caster.AnimControl.Play("Undeploy");
            caster.SetWeaponData(null);
            caster.RemoveAbility(AbilityName);
            caster.RemoveAbility("CheckpointRake");
            caster.AddAbility(CheckpointDeployBuffed.AbilityName);
            // if the caster is mine then trigger selection event to rebuild skill bar
            if (caster.IsMine && SelectionManager.Instance.SelectedUnits.Contains(caster))
            {
                SelectionManager.Instance.OnSelectionChanged.Invoke();
            }
        }

        public void Update()
        {
            if (caster == null) return;
            
            if (!caster.AnimControl.isPlaying)
            {
                caster.StateMachine.ChangeState(new UnitIdleState(caster));
            }
        }

        public void Finish() { }
    }
}