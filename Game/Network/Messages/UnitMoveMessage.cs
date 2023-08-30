using FracturedState.Game.AI;
using UnityEngine;

namespace FracturedState.Game.Network
{
    public class UnitMoveMessage : ILockStepMessage
    {
        public uint Id { get; set; }
        UnitManager unit;
        Vector3 destination;

        public UnitMoveMessage(UnitManager unit, Vector3 destination)
        {
            this.unit = unit;
            this.destination = destination;
        }

        public void Process()
        {
            if (unit != null)
            {
                if (unit.WorldState == Nav.State.Exterior)
                {
                    unit.StateMachine.ChangeState(new UnitMoveState(unit, destination));
                }
                else
                {
                    RaycastHit hit = RaycastUtil.RaycastExterior(new Vector3(destination.x, 100, destination.z));
                    if (hit.transform != null)
                    {
                        StructureManager structure = hit.transform.GetAbsoluteParent().GetComponent<StructureManager>();
                        if (structure == unit.CurrentStructure)
                        {
                            unit.ReturnFirePoint();
                            unit.StateMachine.ChangeState(new UnitMoveState(unit, destination));
                        }
                        else
                        {
                            unit.OnExitIssued(destination);
                        }
                    }
                    else
                    {
                        unit.OnExitIssued(destination);
                    }
                }
            }
        }
    }
}