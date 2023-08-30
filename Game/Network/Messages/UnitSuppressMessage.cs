using FracturedState.Game.AI;

namespace FracturedState.Game.Network
{
    public class UnitSuppressMessage : ILockStepMessage
    {
        public uint Id { get; set; }
        
        private readonly UnitManager unit;
        private readonly int structureId;
        private readonly string pointName;
        
        public UnitSuppressMessage(UnitManager unit, int structureId, string pointName)
        {
            this.unit = unit;
            this.structureId = structureId;
            this.pointName = pointName;
        }
        
        public void Process()
        {
            var structure = ObjectUIDLookUp.Instance.GetStructureManager(structureId);
            unit.StateMachine.ChangeState(new UnitSuppressState(unit, structure, pointName));
        }
    }
}