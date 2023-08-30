using FracturedState.Game.StatTracker;

namespace FracturedState.Game.Network
{
    public class UnitTakeDamageMessage : ILockStepMessage
    {
        public uint Id { get; set; }
        private readonly UnitManager target;
        private readonly UnitManager attacker;
        private readonly int damageAmount;
        private readonly string weaponName;

        public UnitTakeDamageMessage(UnitManager target, UnitManager attacker, int amount, string weaponName)
        {
            this.target = target;
            this.attacker = attacker;
            damageAmount = amount;
            this.weaponName = weaponName;
        }

        public void Process()
        {
            if (target.DamageProcessor == null) return;
            
            target.DamageProcessor.TakeDamage(damageAmount, attacker, weaponName);
            if (target.IsAlive) return;

            if (target.Data.IsSelectable && attacker != null && target.OwnerTeam != attacker.OwnerTeam)
            {
                MatchStatTracker.KillUnit(attacker.OwnerTeam, target);
            }
        }
    }
}