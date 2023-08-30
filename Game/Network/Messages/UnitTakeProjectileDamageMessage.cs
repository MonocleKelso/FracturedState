using FracturedState.Game.StatTracker;
using UnityEngine;

namespace FracturedState.Game.Network
{
    public class UnitTakeProjectileDamageMessage : ILockStepMessage
    {
        public uint Id { get; set; }
        private readonly UnitManager unit;
        private readonly UnitManager attacker;
        private readonly Vector3 position;
        private readonly int damage;
        private string weapon;

        public UnitTakeProjectileDamageMessage(UnitManager unit, UnitManager attacker, Vector3 position, int damage, string weapon)
        {
            this.unit = unit;
            this.attacker = attacker;
            this.position = position;
            this.damage = damage;
            this.weapon = weapon;
        }

        public void Process()
        {
            if (string.IsNullOrEmpty(weapon)) weapon = "Dummy";
            if (unit.DamageProcessor == null) return;
            
            unit.DamageProcessor.TakeProjectileDamage(damage, attacker, position, weapon);
            if (unit.IsAlive) return;

            if (unit.Data.IsSelectable && attacker != null && unit.OwnerTeam != attacker.OwnerTeam)
            {
                MatchStatTracker.KillUnit(attacker.OwnerTeam, unit);
            }
        }
    }
}