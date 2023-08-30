using FracturedState.Game.Management;

namespace FracturedState.Game.Network
{
    public class UnitApplyBuffMessage : ILockStepMessage
    {
        public uint Id { get; set; }
        UnitManager unit;
        BuffType buffType;
        float amount;
        float duration;
        string fx;

        public UnitApplyBuffMessage(UnitManager unit, BuffType buffType, float amount, float duration, string fx)
        {
            this.unit = unit;
            this.buffType = buffType;
            this.amount = amount;
            this.duration = duration;
            this.fx = fx;
        }

        public void Process()
        {
            if (unit != null)
            {
                Buff buff;
                if (string.IsNullOrEmpty(fx))
                {
                    buff = new Buff(buffType, amount, duration);
                }
                else
                {
                    buff = new Buff(buffType, amount, duration, fx);
                }
                unit.Stats.AddBuff(buff);
            }
        }
    }
}