using FracturedState.Game.AI;
using FracturedState.Scripting;

namespace FracturedState.Game
{
    public class CheckpointIndicator : PositionClampedIndicatorManager
    {
        protected override void ExecuteAbility()
        {
            Unit.UseAbility(CheckpointDeploy.AbilityName);
            UnitBarkManager.Instance.AbilityBark(XmlCacheManager.Abilities[CheckpointDeploy.AbilityName]);
            Unit.SetMicroState(new MicroUseAbilityState(Unit, "CheckpointDeploy_execute", transform.rotation.eulerAngles));
            Unit.PropagateMicroState();
            SkillManager.ResetSkill();
        }
    }
}