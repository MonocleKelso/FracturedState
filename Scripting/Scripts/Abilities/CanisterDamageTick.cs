using FracturedState.Game;
using FracturedState.Game.Data;
using System.Collections.Generic;
using FracturedState.Game.Management;
using UnityEngine;

namespace FracturedState.Scripting
{
    public class CanisterDamageTick : MonoBehaviour
    {
        const float tickTime = 1;
        const string weaponName = "FlamethrowerCanister";

        static Weapon weapon;

        public UnitManager Caster { get; set; }
        Dictionary<UnitManager, float> damageTimes = new Dictionary<UnitManager, float>();
        List<UnitManager> units = new List<UnitManager>();

        void Start()
        {
            if (weapon == null)
            {
                weapon = XmlCacheManager.Weapons[weaponName];
            }
        }

        void Update()
        {
            for (int i = 0; i < units.Count; i++)
            {
                var unit = units[i];
                float dTime;
                if (damageTimes.TryGetValue(unit, out dTime) && Time.time - dTime >= tickTime)
                {
                    damageTimes[unit] = Time.time;
                    if (unit.NetMsg != null)
                    {
                        unit.NetMsg.CmdTakeDamage(weapon.Damage, null, weaponName);
                        unit.NetMsg.CmdApplyBuff((int)BuffType.Accuracy, -20, 1, string.Empty);
                    }
                }
            }
        }

        void OnTriggerEnter(Collider other)
        {
            var unit = other.GetComponent<UnitManager>();
            if (unit != null && unit.OwnerTeam != Caster.OwnerTeam)
            {
                damageTimes[unit] = Time.time - tickTime;
                if (!units.Contains(unit))
                {
                    units.Add(unit);
                }
            }
        }

        void OnTriggerExit(Collider other)
        {
            var unit = other.GetComponent<UnitManager>();
            if (unit != null)
            {
                damageTimes.Remove(unit);
            }
        }

        void OnDestroy()
        {
            units.Clear();
            damageTimes.Clear();
            Caster = null;
        }
    }
}