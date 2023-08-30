namespace FracturedState.Game.Network
{
    class UnitEnterStructureMessage : ILockStepMessage
    {
        public uint Id { get; set; }
        UnitManager unit;
        int structureId;

        public UnitEnterStructureMessage(UnitManager unit, int structureId)
        {
            this.unit = unit;
            this.structureId = structureId;
        }

        public void Process()
        {
            StructureManager sm = ObjectUIDLookUp.Instance.GetStructureManager(structureId);
            unit.OnEnterIssued(sm);
        }
    }
}
