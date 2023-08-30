using UnityEngine;

namespace FracturedState.Game.AI
{
    public class UnitApplyFacingState : UnitBaseState
    {
        public UnitApplyFacingState(UnitManager owner) : base(owner) { }

        public override void Execute()
        {
            if (Owner.Data.IsInfantry)
            {
                Owner.transform.rotation = Owner.Squad.Facing;
                Owner.Squad.FacingVector = Owner.transform.forward;
                Owner.StateMachine.ChangeState(new UnitIdleState(Owner));
            }
            else
            {
                if (Quaternion.Angle(Owner.transform.rotation, Owner.Squad.Facing) > 0.01f)
                {
                    Owner.transform.rotation = Quaternion.RotateTowards(Owner.transform.rotation, Owner.Squad.Facing, Owner.Data.Physics.TurnRate * Time.deltaTime);
                }
                else
                {
                    Owner.Squad.FacingVector = Owner.transform.forward;
                    Owner.StateMachine.ChangeState(new UnitIdleState(Owner));
                }
            }
        }
    }
}