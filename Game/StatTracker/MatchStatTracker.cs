using System.Collections.Generic;
using System.Linq;
using FracturedState.Game.Management;

namespace FracturedState.Game.StatTracker
{
    public static class MatchStatTracker
    {
        private static readonly List<TeamStats> Stats = new List<TeamStats>();

        public static void KillUnit(Team team, UnitManager unit)
        {
            var stat = GetStats(team);

            stat.KillCount++;
            
            if (unit != null && unit.OwnerTeam != null)
            {
                var other = Stats.SingleOrDefault(s => s.OwnerTeam == unit.OwnerTeam);
                if (other == null)
                {
                    other = new TeamStats(unit.OwnerTeam);
                    Stats.Add(other);
                }

                other.UnitLostCount++;
            }
        }

        public static void MakeUnits(Team team, int count)
        {
            var stat = GetStats(team);

            stat.UnitMadeCount += count;
        }

        public static void CheckMaxStructures(Team team, int count)
        {
            var stat = GetStats(team);

            if (stat.MaxStructuresOwned < count)
            {
                stat.MaxStructuresOwned = count;
            }
        }

        public static void CheckMaxTerritories(Team team, int count)
        {
            var stat = GetStats(team);

            if (stat.MaxTerritoriesOwned < count)
            {
                stat.MaxTerritoriesOwned = count;
            }
        }

        public static List<TeamStats> GetStats()
        {
            return Stats;
        }
        
        public static void Reset()
        {
            Stats.Clear();
        }

        private static TeamStats GetStats(Team team)
        {
            var stat = Stats.SingleOrDefault(s => s.OwnerTeam == team);
            if (stat == null)
            {
                stat = new TeamStats(team);
                Stats.Add(stat);
            }

            return stat;
        }
    }
}