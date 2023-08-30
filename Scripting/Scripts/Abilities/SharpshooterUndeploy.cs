using FracturedState.Game.AI;
using FracturedState.Game.Data;

namespace FracturedState.Scripting
{
    public class SharpshooterUndeploy : UnitUndeployAbility, IMonitorAbility
    {
        public const string AbilityName = "SharpshooterUndeploy";
        private const string WeaponName = "SharpshooterGun_undeployed";
        
        public SharpshooterUndeploy(UnitManager caster, Ability ability) : base(caster, ability) { }

        public override void ExecuteAbility()
        {
            base.ExecuteAbility();
            caster.AnimControl.Play("UnDeploy");
            // swap back to undeployed gun
            caster.SetWeaponData(WeaponName);
            // toggle deploy/undeploy buttons
            caster.RemoveAbility(AbilityName);
            caster.AddAbility(SharpshooterDeploy.AbilityName);
            caster.UseAbility(SharpshooterDeploy.AbilityName);
            // remove Penetrating Shot
            caster.RemoveAbility("PenetratingShot");
            // change firing animation
            caster.Data.Animations.CrouchFire[0] = "HipFire";
            caster.Data.Animations.StandFire[0] = "HipFire";
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