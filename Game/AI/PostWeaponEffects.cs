using FracturedState.Game.Data;
using FracturedState.Game.Management;
using FracturedState.Game.Network;
using UnityEngine;
using UnityEngine.Networking;

namespace FracturedState.Game.AI
{
    /// <summary>
    /// A common interface to implement effects that occur after a weapon hits a target
    /// </summary>
    public interface IWeaponPostEffect
    {
        void DoEffect(UnitManager attacker, UnitManager target, Weapon weapon);
    }

    public class SurgeonHealDebuff : IWeaponPostEffect
    {
        public void DoEffect(UnitManager attacker, UnitManager target, Weapon weapon)
        {
            target.Stats.AddBuff(new Buff(BuffType.Heal, -10, 15, "Surgeon/surgeonAttackDebuff"));
        }
    }

    public class SurgeonBleed : IWeaponPostEffect
    {
        public void DoEffect(UnitManager attacker, UnitManager target, Weapon weapon)
        {
            if (!FracNet.Instance.IsHost) return;
            
            target.Stats.AddBuff(new Buff(BuffType.Bleed, 1, 3));
        }
    }

    public class CheckpointDebuff : IWeaponPostEffect
    {
        public void DoEffect(UnitManager attacker, UnitManager target, Weapon weapon)
        {
            target.Stats.AddBuff(new Buff(BuffType.Defense, -35, 3));
        }
    }

    public class FlamethrowerPyre : IWeaponPostEffect
    {
        public void DoEffect(UnitManager attacker, UnitManager target, Weapon weapon)
        {
            if (!FracNet.Instance.IsHost || target.IsAlive) return;
            
            var playerObj = GlobalNetworkActions.GetActions(attacker.OwnerTeam).gameObject;
            var pyre = DataUtil.LoadBuiltInModel("Projectiles/FlamethrowerPyre");
            pyre.transform.position = target.transform.position;
            pyre.layer = attacker.WorldState == Nav.State.Exterior ? GameConstants.ExteriorLayer : GameConstants.InteriorLayer;
            var attack = pyre.GetComponent<PyreAttack>();
            attack.Owner = attacker.OwnerTeam;
            NetworkServer.SpawnWithClientAuthority(pyre, playerObj);
        }
    }
}