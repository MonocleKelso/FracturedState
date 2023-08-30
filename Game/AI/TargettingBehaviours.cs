using FracturedState.Game.Data;
using System.Collections.Generic;
using UnityEngine;

namespace FracturedState.Game.AI
{
    /// <summary>
    /// A common interface to implement custom targetting behavior
    /// </summary>
    public interface ITargettingBehaviour
    {
        /// <summary>
        /// Find a target based on the logic of this implementation.
        /// </summary>
        /// <param name="owner"></param>
        /// <returns></returns>
        UnitManager FindTarget(UnitManager owner);
    }

    public interface ITargettingBehaviour<T> : ITargettingBehaviour
    {
        UnitManager FindTarget(UnitManager owner, T arg);
    }

    /// <summary>
    /// A targetting behavior that never picks a target.
    /// </summary>
    public class NoTarget : ITargettingBehaviour
    {
        public UnitManager FindTarget(UnitManager owner)
        {
            return null;
        }
    }

    /// <summary>
    /// A targetting behavior that picks the target farthest away from the calling unit that also has the most enemy units in between it and
    /// the target's location.
    /// </summary>
    public class FarthestMostBetween : ITargettingBehaviour
    {
        public UnitManager FindTarget(UnitManager owner)
        {
            if (owner.Squad == null) return null;
            
            var weapon = owner.ContextualWeapon;
            if (weapon == null) return null;
            
            var visible = owner.Squad.GetVisibleForUnit(owner);
            if (visible == null) return null;
            
            var maxBetween = 0;
            UnitManager target = null;
            for (var i = 0; i < visible.Count; i++)
            {
                if (visible[i] == null) continue;
                
                var toTarget = visible[i].transform.position - owner.transform.position;
                var hits = Physics.CapsuleCastAll(owner.transform.position, visible[i].transform.position, weapon.DamageRadius, toTarget.normalized, toTarget.magnitude);
                var unitCount = 0;
                for (var h = 0; h < hits.Length; h++)
                {
                    var unit = hits[h].collider.GetComponent<UnitManager>();
                    if (unit != null && unit.OwnerTeam != owner.OwnerTeam)
                    {
                        unitCount++;
                    }
                }
                if (unitCount > maxBetween)
                {
                    maxBetween = unitCount;
                    target = visible[i];
                }
                else if (target != null && unitCount == maxBetween)
                {
                    if (toTarget.sqrMagnitude > (target.transform.position - owner.transform.position).sqrMagnitude)
                    {
                        target = visible[i];
                    }
                }
            }
            return target;
        }
    }

    /// <summary>
    /// A targetting behavior that picks the most damage squad mate of the given unit. Returns null if the entire squad is at full health
    /// </summary>
    public class MostDamagedSquadMate : ITargettingBehaviour
    {
        public UnitManager FindTarget(UnitManager owner)
        {
            if (owner.Squad != null)
            {
                UnitManager target = null;
                int damage = 0;
                for (int i = 0; i < owner.Squad.Members.Count; i++)
                {
                    var m = owner.Squad.Members[i];
                    if (m.DamageProcessor != null)
                    {
                        int dmg = m.Data.Health - m.DamageProcessor.CurrentHealth;
                        if (dmg > damage)
                        {
                            target = m;
                            damage = dmg;
                        }
                    }
                }
                return target;
            }
            return null;
        }
    }

    public class MostDamagedFriendlyNearby : ITargettingBehaviour<float>
    {
        public UnitManager FindTarget(UnitManager owner, float arg)
        {
            var nearby = Physics.OverlapSphere(owner.transform.position, arg, GameConstants.FriendlyUnitMask);
            if (nearby == null || nearby.Length == 0) return null;
            UnitManager target = null;
            float healthPercent = 1;
            foreach (var nb in nearby)
            {
                var unit = nb.GetComponent<UnitManager>();
                if (unit != null && unit.IsAlive && unit.WorldState == owner.WorldState && unit.DamageProcessor != null)
                {
                    if (owner.WorldState == Nav.State.Interior && owner.CurrentStructure != unit.CurrentStructure) continue;
                    
                    var percent = (float)unit.DamageProcessor.CurrentHealth / unit.Data.Health;
                    if (!(percent < healthPercent)) continue;
                    
                    healthPercent = percent;
                    target = unit;
                }
            }
            return target;
        }

        public UnitManager FindTarget(UnitManager owner)
        {
            return FindTarget(owner, Mathf.Infinity);
        }
    }
}