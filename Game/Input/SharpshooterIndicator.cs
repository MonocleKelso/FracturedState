using FracturedState.Game.AI;
using FracturedState.Scripting;

namespace FracturedState.Game
{
    public class SharpshooterIndicator : PositionClampedIndicatorManager
    {
        protected override void ExecuteAbility()
        {
            Unit.UseAbility("PenetratingShot");
            Unit.SetMicroState(new MicroUseAbilityState(Unit, PenetratingShotExecute.AbilityName, transform.rotation.eulerAngles));
            Unit.PropagateMicroState();
        }
    }
}