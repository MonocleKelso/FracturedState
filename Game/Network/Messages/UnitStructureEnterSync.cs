using FracturedState.Game.AI;
using UnityEngine;

namespace FracturedState.Game.Network
{
    public class UnitStructureEnterSync : ILockStepMessage
    {
        public uint Id { get; set; }
        private readonly UnitManager unit;
        private readonly Vector3 position;
        private readonly int structureId;

        public UnitStructureEnterSync(UnitManager unit, int structureId, Vector3 position)
        {
            this.unit = unit;
            this.structureId = structureId;
            this.position = position;
        }

        public void Process()
        {
            unit.WorldState = Nav.State.Interior;
            var sm = ObjectUIDLookUp.Instance.GetStructureManager(structureId);
            unit.CurrentStructure = sm;
            unit.transform.position = position;
            
            if (unit.IsMine)
            {
                unit.gameObject.SetLayerRecursively(GameConstants.InteriorUnitLayer, "Vision");
                unit.UpdateSelectionLayer();

                unit.Squad?.UnregisterAllVisibleUnits(unit);
            }
            else
            {
                unit.gameObject.SetLayerRecursively(GameConstants.InteriorEnemyLayer, "Vision");
                
                // force move path to null which will trigger OnArrival and stop the unit from
                // steering back out of the structure to continue their state execution
                var state = unit.StateMachine.CurrentState as UnitEnterStructureState;
                state?.SetPath(null);
            }
            
            unit.CurrentStructure.Occupy(unit);
        }
    }
}