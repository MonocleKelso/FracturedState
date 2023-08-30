using System.Collections.Generic;
using FracturedState.Game.Data;
using FracturedState.Game.Management;
using FracturedState.Game.Network;
using UnityEngine;

namespace FracturedState.Game.AI
{
    public class PyreAttack : MonoBehaviour
    {
        private Team owner;
        public Team Owner
        {
            get { return owner; }
            set
            {
                if (owner == null) owner = value;
            } 
        }
        private Weapon weapon;
        private int mask;
        private readonly Dictionary<UnitManager, float> hitTimes = new Dictionary<UnitManager, float>();
        
        private void Start()
        {
            weapon = XmlCacheManager.Weapons["PyreFire"];
            mask = gameObject.layer == GameConstants.ExteriorLayer ? GameConstants.ExteriorUnitAllMask : GameConstants.InteriorUnitAllMask;
        }

        private void Update()
        {
            if (!FracNet.Instance.IsHost) return;
            
            var nearby = Physics.OverlapSphere(transform.position, weapon.Range, mask);
            foreach (var near in nearby)
            {
                var unit = near.GetComponent<UnitManager>();
                if (unit == null || unit.Transport != null || unit.OwnerTeam == Owner) continue;

                float hit;
                if (!hitTimes.TryGetValue(unit, out hit))
                {
                    hitTimes[unit] = weapon.FireRate;
                    var damage = unit.MitigateDamage(weapon, weapon.Damage, transform.position);
                    unit.NetMsg.CmdTakeDamage(damage, null, weapon.Name);
                    continue;
                }

                hit -= Time.deltaTime;
                if (hit <= 0)
                {
                    hitTimes[unit] = weapon.FireRate;
                    var damage = unit.MitigateDamage(weapon, weapon.Damage, transform.position);
                    unit.NetMsg.CmdTakeDamage(damage, null, weapon.Name);
                }
            }
        }

        private void OnDestroy()
        {
            hitTimes.Clear();
        }
    }
}