using FracturedState.Game.AI;
using FracturedState.Game.Data;
using UnityEngine;

namespace FracturedState.Scripting
{
    public class FindFirePoint : SelfAbility
    {
        public FindFirePoint(UnitManager caster, Ability ability) : base(caster, ability) { }

        public override void ExecuteAbility()
        {
            base.ExecuteAbility();
            if (caster.IsMine && caster.IsAlive && caster.Data.CanTakeFirePoint && !caster.IsOnFirePoint)
            {
                Transform firePoint = caster.CurrentStructure.TakeClosestFirePoint(caster.transform.position);
                if (firePoint != null)
                {
                    caster.NetMsg.CmdTakeFirePoint(firePoint.name);
                }
                else
                {
                    caster.StateMachine.ChangeState(new UnitIdleState(caster));
                }
            }

            if (caster.IsOnFirePoint)
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