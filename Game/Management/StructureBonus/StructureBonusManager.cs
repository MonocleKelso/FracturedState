using System.Collections.Generic;
using System.Linq;
using FracturedState.Game.Data;
using FracturedState.Game.Network;
using UnityEngine;

namespace FracturedState.Game.Management.StructureBonus
{
    public static class StructureBonusManager
    {
        private static readonly Dictionary<Team, List<IStructureBonus>> Bonuses = new Dictionary<Team, List<IStructureBonus>>();

        public static void AddBonus(Team team, string structure)
        {
            var bonus = StructureBonusLookup.GetStructureBonus(structure);
            if (bonus == null) return;

            List<IStructureBonus> bonuses;
            if (!Bonuses.TryGetValue(team, out bonuses))
            {
                bonuses = new List<IStructureBonus>();
                Bonuses[team] = bonuses;
            }
            
            bonuses.Add(bonus);

            // apply to existing units
            foreach (var squad in team.Squads)
            {
                foreach (var unit in squad.Members)
                {
                    if (unit == null || !unit.IsAlive) continue;
                    
                    bonus.ApplyOnUnit(unit);
                    if (unit.Data.WeaponData != null)
                    {
                        bonus.ApplyOnWeapon(unit.Data.WeaponData);
                    }
                }
            }
            
            // do event for local team
            if (team == FracNet.Instance.LocalTeam)
            {
                MultiplayerEventBroadcaster.Text($"{bonus.StructureName} Bonus {bonus.BonusText}");
            }
        }

        public static void RemoveBonus(Team team, string structure)
        {
            List<IStructureBonus> bonuses;
            if (!Bonuses.TryGetValue(team, out bonuses)) return;

            var toRemove = bonuses.SingleOrDefault(b => b.StructureName == structure);
            if (toRemove != null)
            {
                bonuses.Remove(toRemove);

                // remove from existing units
                foreach (var squad in team.Squads)
                {
                    foreach (var unit in squad.Members)
                    {
                        if (unit == null || !unit.IsAlive) continue;
                        
                        toRemove.RemoveFromUnit(unit);
                        if (unit.Data.WeaponData != null)
                        {
                            toRemove.RemoveFromWeapon(unit.Data.WeaponData);
                        }
                    }
                }
                
                // do event for local team
                if (team == FracNet.Instance.LocalTeam)
                {
                    MultiplayerEventBroadcaster.Text($"{toRemove.StructureName} Bonus Lost");
                }
            }
        }

        public static void ApplyUnitBonuses(Team team, UnitManager unit)
        {
            List<IStructureBonus> bonus;
            if (!Bonuses.TryGetValue(team, out bonus)) return;

            foreach (var b in bonus)
            {
                b.ApplyOnUnit(unit);
            }
        }
        
        public static void ApplyWeaponBonuses(Team team, Weapon weapon)
        {
            List<IStructureBonus> bonus;
            if (!Bonuses.TryGetValue(team, out bonus)) return;

            foreach (var b in bonus)
            {
                b.ApplyOnWeapon(weapon);
            }
        }
        
        public static void Reset()
        {
            Bonuses.Clear();
        }
    }
}