namespace FracturedState.Game.Network
{
    public class UnitHealMessage : ILockStepMessage
    {
        public uint Id { get; set; }
        UnitManager unit;
        int amount;

        public UnitHealMessage(UnitManager unit, int amount)
        {
            this.unit = unit;
            this.amount = amount;
        }

        public void Process()
        {
            if (unit != null && unit.DamageProcessor != null)
            {
                unit.DamageProcessor.Heal(amount);
            }
        }
    }
}