using System.Collections.Generic;
using System.Linq;
using FracturedState.Game.Data;
using FracturedState.Game.Management.StructureBonus;
using FracturedState.Game.Network;
using FracturedState.UI;
using ThreeEyedGames;
using UnityEngine;

namespace FracturedState.Game.Management
{
    public delegate void TeamAddedDelegate(string playerName);
    public delegate void TeamRemovedDelegate(string playerName);
	public delegate void MatchOverDelegate(Team winner);

    public static class SkirmishVictoryManager
    {
        public static readonly int[] WarningStateTimes = { 60, 45, 30, 15, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 };

        public static List<Team> SkirmishTeams { get; private set; }
        private static readonly List<Team> EliminatedTeams = new List<Team>();

		public static RawMapData CurrentMap { get; set; }
        public static RawMapData CustomMap { get; set; }
        public static bool IsMultiPlayer { get; set; }
        public static bool MatchInProgress { get; set; }
        public static string GameName { get; set; }

        public static TeamAddedDelegate OnTeamAdded;
        public static TeamRemovedDelegate OnTeamRemoved;
		public static MatchOverDelegate OnMatchComplete;

        public static Team WinningTeam { get; private set; }

        public static string GameTimeSnapshot { get; set; }

        public static bool IsSpectating => FracNet.Instance.NetworkActions.LocalTeam.IsSpectator;

        public static void SetWinningTeam(Team team)
        {
            WinningTeam = team;
        }

        public static void AddTeam(Team team)
        {
            if (SkirmishTeams == null)
            {
                SkirmishTeams = new List<Team>();
            }
            var lobbySlot = team.LobbySlotIndex == -1 ? LobbyManager.Instance.GetNextSlot() : LobbyManager.Instance.GetSlotAtIndex(team.LobbySlotIndex);
            if (lobbySlot != null)
            {
                team.UpdateFaction(team.FactionIndex); // default new players to first faction
                if (team.HouseColorIndex == -1)
                {
                    team.UpdateTeamColor(TeamColorSelect.GetNextAvailableColor()); // default to next available color
                }
                else
                {
                    team.UpdateTeamColor(team.HouseColorIndex);
                }
                
                team.LobbySlotIndex = LobbyManager.Instance.IndexOfSlot(lobbySlot);
                SkirmishTeams.Add(team);
                lobbySlot.SetTeam(team);

                OnTeamAdded?.Invoke(team.PlayerName);
            }
        }

        public static void RemoveTeam(Team team)
        {
            SkirmishTeams.Remove(team);
            // release this team's color so it can be selected by someone else
            TeamColorSelect.SwapTeamColor(team.HouseColorIndex, -1);

            OnTeamRemoved?.Invoke(team.PlayerName);
        }
		
        public static Team GetTeam(int connectionId)
        {
            if (SkirmishTeams == null || SkirmishTeams.Count == 0)
                return null;

            for (var i = 0; i < SkirmishTeams.Count; i++)
            {
                if (SkirmishTeams[i].IsHuman && SkirmishTeams[i].NetworkedPlayerId == connectionId)
                    return SkirmishTeams[i];
            }
            return null;
        }
		
        public static Team GetTeam(string playerName)
        {
            if (SkirmishTeams == null || SkirmishTeams.Count == 0)
                return null;

            for (var i = 0; i < SkirmishTeams.Count; i++)
            {
                if (SkirmishTeams[i].PlayerName == playerName)
                    return SkirmishTeams[i];
            }
            return null;
        }

        public static void ResetTeamReadiness()
        {
            foreach (var team in SkirmishTeams)
            {
                team.SetReadyState(false);
            }
        }

        public static bool IsTeamEliminated(Team team)
        {
            return EliminatedTeams.Contains(team);
        }

        public static void UneliminateTeam(Team team)
        {
            EliminatedTeams.Remove(team);
        }

        public static void UpdateMatchStatus()
        {
            if (CompassUI.Instance.Timers.ElapsedGameTime > GameConstants.EliminationWaitTime && TerritoryManager.Instance.GetMaxOwnedTerritories() > 0)
            {
                // set elimnation status for host
                if (FracNet.Instance.IsHost)
                {
                    for (var i = 0; i < SkirmishTeams.Count; i++)
                    {
                        if (!SkirmishTeams[i].IsSpectator)
                        {
                            var tCount = TerritoryManager.Instance.GetOwnedTerritoryCount(SkirmishTeams[i]);
                            if (tCount == 0)
                            {
                                if (!EliminatedTeams.Contains(SkirmishTeams[i]))
                                {
                                    EliminatedTeams.Add(SkirmishTeams[i]);
                                    FracNet.SetTeamElimination(SkirmishTeams[i]);
                                }
                            }
                        }
                    }
                }
            }
        }

		public static void OnTeamDefeated(Team team)
		{
		    if (!FracNet.Instance.IsHost) return;
		    
		    var units = Object.FindObjectsOfType<UnitManager>();
		    foreach (var unit in units)
		    {
		        if (unit.OwnerTeam == team)
		        {
		            unit.NetMsg.CmdTakeDamage(int.MaxValue, null, Weapon.DummyName);
		        }
		    }
		    var activeTeams = SkirmishTeams.Where(t => t.IsActive && !t.IsSpectator).ToArray();
		    if (activeTeams.Select(t => t.Side).Distinct().Count() == 1)
		    {
		        OnMatchComplete(activeTeams.First());
		    }
		}

        private static void DeleteMapChildren(string parentName)
        {
            var parent = GameObject.Find(parentName).transform;
            var children = parent.GetComponentsInChildren<Transform>();
            foreach (var t in children)
            {
                if (t != parent)
                {
                    Object.Destroy(t.gameObject);
                }
            }
        }
        
        public static void PostGameWorldCleanUp()
        {
            Loader.Instance.InterruptLocalWarning();
            Loader.Instance.StopAllCoroutines();
            ScreenEdgeNotificationManager.Instance.RemoveAllIcons();
            SelectionManager.Instance.ClearSelection();
            foreach (var team in SkirmishTeams)
            {
                team.RecruitManager?.ClearAllPendingRequests();
                team.Reset();
            }
            
            StructureBonusManager.Reset();
            
            var units = UnitManager.WorldUnitParent.GetComponentsInChildren<Transform>();
            foreach (var t in units)
            {
                if (t == UnitManager.WorldUnitParent) continue;
                
                // allow states to clean themselves up correctly
                var unit = t.GetComponent<UnitManager>();
                if (unit != null)
                {
                    unit.StateMachine.ChangeState(null);
                }
                Object.Destroy(t.gameObject);
            }
            
            var structures = StructureManager.WorldTransform.GetComponentsInChildren<Transform>();
            foreach (var t in structures)
            {
                if (t != StructureManager.WorldTransform)
                {
                    Object.Destroy(t.gameObject);
                }
            }

            var shrouds = GameObject.FindGameObjectsWithTag(GameConstants.BuildingShroudTag);
            foreach (var g in shrouds)
            {
                Object.Destroy(g);
            }

            var decals = Object.FindObjectsOfType<Decal>();
            if (decals != null)
            {
                foreach (var decal in decals)
                {
                    Object.Destroy(decal.gameObject);
                }
            }

            var buttons = Object.FindObjectsOfType<WorldRecruitButton>();
            if (buttons != null)
            {
                foreach (var button in buttons)
                {
                    Object.Destroy(button.gameObject);
                }
            }

            SelectionManager.Instance.SelectedUnits.Clear();
            VisibilityChecker.Instance.StopChecking();
            var cm = Object.FindObjectOfType<CommandManager>();
            cm.ResetTerritoryHelper();
            cm.enabled = false;
            Object.FindObjectOfType<MicroManager>().enabled = false;
            SelectionManager.Instance.enabled = false;
            Object.FindObjectOfType<UnitHotKeyManager>().enabled = false;

            Camera.main.gameObject.GetComponent<CommonCameraController>().enabled = false;
            Camera.main.gameObject.GetComponent<WorldBackgroundRenderer>().enabled = false;

            DeleteMapChildren("TerrainParent");
            DeleteMapChildren("PropParent");
            
            ResetTeamReadiness();
            StructureManager.ResetOwnedStructures();
            TerritoryManager.Reset();
            SelectionManager.Instance.Reset();
            MatchInProgress = false;
            Resources.UnloadUnusedAssets();
        }
    }
}