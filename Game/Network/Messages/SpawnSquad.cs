using UnityEngine.Networking;
using FracturedState.Game.Management;
using FracturedState.Game.StatTracker;
using FracturedState.UI;
using UnityEngine;

namespace FracturedState.Game.Network
{
    public class SpawnSquad : MessageBase
    {
        public int TeamId;
        public string AITeamName;
        public bool IsHuman;
        public NetworkIdentity[] UnitIds;
        public Vector3 RallyPoint;

        public void BindSquad()
        {
            var team = IsHuman ? SkirmishVictoryManager.GetTeam(TeamId) : SkirmishVictoryManager.GetTeam(AITeamName);
            var units = new UnitManager[UnitIds.Length];
            for (var i = 0; i < UnitIds.Length; i++)
            {
                var unit = UnitIds[i].GetComponent<UnitManager>();
                units[i] = unit;

            }
            var squad = new Squad(units);
            squad.SetOwner(team);
            squad.SquadMove(RallyPoint, false);
            if (team == FracNet.Instance.NetworkActions.LocalTeam)
            {
                CompassUI.Instance.AddSquad();
                ScreenEdgeNotificationManager.Instance.RequestRecruitNotification(squad);
            }
            MatchStatTracker.MakeUnits(team, UnitIds.Length);
        }
    }
}