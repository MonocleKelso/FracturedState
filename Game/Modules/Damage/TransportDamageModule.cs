using UnityEngine;

namespace FracturedState.Game.Modules
{
    public class TransportDamageModule : UnitDamageModule
    {
        public override void TakeDamage(int damage, UnitManager attacker, string weapon)
        {
            if (!IsAlive) return;
            base.TakeDamage(damage, attacker, weapon);
            if (!IsAlive)
            {
                for (int i = 0; i < unitManager.Passengers.Count; i++)
                {
                    unitManager.Passengers[i].RequestTransportExit();
                }
            }
        }

        public override void TakeProjectileDamage(int damage, UnitManager attacker, Vector3 projectilePosition, string weapon)
        {
            if (!IsAlive) return;
            base.TakeProjectileDamage(damage, attacker, projectilePosition, weapon);
            if (!IsAlive)
            {
                for (int i = 0; i < unitManager.Passengers.Count; i++)
                {
                    unitManager.Passengers[i].RequestTransportExit();
                }
            }
        }
    }
}