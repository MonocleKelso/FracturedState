using FracturedState.Game.Management;

namespace FracturedState.Game.StatTracker
{
    public class TeamStats
    {
        public Team OwnerTeam { get; }
        public int KillCount { get; set; }
        public int UnitLostCount { get; set; }
        public int UnitMadeCount { get; set; }
        public int MaxStructuresOwned { get; set; }
        public int MaxTerritoriesOwned { get; set; }

        public TeamStats(Team team)
        {
            OwnerTeam = team;
        }
    }
}