namespace FracturedState.Game.Network
{
    public class UnitSetTargetMessage : ILockStepMessage
    {
        public uint Id { get; set; }
        private readonly UnitManager unit;
        private readonly UnitManager target;

        public UnitSetTargetMessage(UnitManager unit, UnitManager target)
        {
            this.unit = unit;
            this.target = target;
        }

        public void Process()
        {
            unit.OnTargetChanged(target);
        }
    }
}