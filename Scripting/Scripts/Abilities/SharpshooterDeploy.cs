using FracturedState.Game;
using FracturedState.Game.AI;
using FracturedState.Game.Data;

namespace FracturedState.Scripting
{
    public class SharpshooterDeploy : UnitDeployAbility, IMonitorAbility
    {
        public const string AbilityName = "SharpshooterDeploy";
        private const string WeaponName = "SharpshooterGun_deployed";
        
        public SharpshooterDeploy(UnitManager caster, Ability ability) : base(caster, ability) { }

        public override void ExecuteAbility()
        {
            base.ExecuteAbility();
            caster.AnimControl.Play("Deploy");
            // swap to more accurate gun
            caster.SetWeaponData(WeaponName);
            // toggle deploy/undeploy buttons
            caster.RemoveAbility(AbilityName);
            caster.AddAbility(SharpshooterUndeploy.AbilityName);
            caster.UseAbility(SharpshooterUndeploy.AbilityName);
            // give Penetrating Shot ability
            caster.AddAbility("PenetratingShot");
            // change firing animation
            caster.Data.Animations.CrouchFire[0] = "Fire";
            caster.Data.Animations.StandFire[0] = "Fire";
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