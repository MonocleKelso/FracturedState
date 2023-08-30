using FracturedState.Game.AI;
using FracturedState.Game.Data;
using UnityEngine;

namespace FracturedState.Scripting
{
    public class CheckpointDeploy : LocationAbility, IMonitorAbility
    {
        public const string AbilityName = "CheckpointDeploy";
        
        public CheckpointDeploy(UnitManager caster, Vector3 location, Ability ability) : base(caster, location, ability) { }

        public override void ExecuteAbility()
        {
            base.ExecuteAbility();
            caster.LocoMotor = null;
            var rotation = Quaternion.Euler(location);
            caster.transform.rotation = rotation;
            caster.AnimControl.Play("Deploy");
            caster.SetWeaponData("CheckpointGun");
            caster.RemoveAbility(AbilityName);
            caster.AddAbility(CheckpointUndeploy.AbilityName);
            caster.AddAbility("CheckpointRake");
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