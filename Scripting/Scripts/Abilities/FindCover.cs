using FracturedState.Game;
using FracturedState.Game.AI;
using FracturedState.Game.Data;
using UnityEngine;

namespace FracturedState.Scripting
{
    public class FindCover : SelfAbility
    {
        public FindCover(UnitManager caster, Ability ability) : base(caster, ability) { }

        public override void ExecuteAbility()
        {
            if (caster.IsMine && !caster.InCover && !caster.StateMachine.IsCoverPrepped)
            {
                if (!caster.Squad.IsGettingCover)
                {
                    int layerMask = caster.WorldState == Game.Nav.State.Exterior ? GameConstants.ExteriorMask : GameConstants.InteriorMask;
                    caster.Squad.DetermineCover(Physics.OverlapSphere(caster.transform.position, caster.Data.VisionRange, layerMask), caster, true);
                    if (caster.CurrentCover == null)
                    {
                        caster.StateMachine.ChangeState(new UnitIdleState(caster));
                    }
                }
            }
            else if (caster.InCover)
            {
                caster.StateMachine.ChangeState(new UnitIdleCoverState(caster));
            }
            else if (caster.IsOnFirePoint)
            {
                caster.StateMachine.ChangeState(new UnitFirePointIdleState(caster, caster.CurrentFirePoint));
            }
            else
            {
                caster.StateMachine.ChangeState(new UnitIdleState(caster));
            }
        }
    }
}