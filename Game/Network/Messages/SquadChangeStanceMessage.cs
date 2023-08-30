namespace FracturedState.Game.Network
{
    public class SquadChangeStanceMessage : ILockStepMessage
    {
        public uint Id { get; }
        private readonly UnitManager unit;
        private readonly SquadStance stance;

        public SquadChangeStanceMessage(uint id, UnitManager unit, SquadStance stance)
        {
            Id = id;
            this.unit = unit;
            this.stance = stance;
        }
        
        public void Process()
        {
            if (unit != null && unit.Squad != null)
            {
                unit.Squad.Stance = stance;
            }
        }
    }
}