using System;
using FracturedState.Game.AI;
using FracturedState.Game.Data;
using FracturedState.Game.Network;
using FracturedState.UI;
using System.Collections.Generic;
using System.Linq;
using FracturedState.Game.Mutators;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FracturedState.Game.Management
{
    public class Team
    {
        public const int TotalMutatorCost = 10;
        
        public bool IsActive { get; private set; }
        public bool IsHuman { get; }
        public bool IsSpectator { get; private set; }
		public string Faction { get; private set; }
        public int FactionIndex { get; private set; }
        public int HouseColorIndex { get; private set; }
        public HouseColor TeamColor { get; private set; }
        public bool IsReady { get; private set; }
        public bool Connected { get; private set; }
        public int LobbySlotIndex { get; set; }
        public int Side { get; private set; }

        public int NetworkedPlayerId { get; }
        public string PlayerName { get; private set; }
        public int AvatarIndex { get; private set; }

        public ReinforcementManager RecruitManager { get; set; }
        public int SquadPopulation { get; set; }
        public int TeamPopulation { get; set; }
        public float SurrenderTime { get; set; }
        public float EliminatedTime { get; private set; }

        private List<IMutator> mutators = new List<IMutator>();
        public List<IMutator> Mutators => mutators;
        
        public Think AIBrain { get; }
        public List<Squad> Squads { get; private set; }

        public DefeatDelegate OnDefeated;

        private HashSet<uint> receivedMessages;

        public Team()
        {
            Init();
            IsHuman = false;
            AIBrain = new Think(this);
        }

        public Team(int connection, string playerName, int avatar)
        {
            Init();
            NetworkedPlayerId = connection;
            PlayerName = playerName;
            AvatarIndex = avatar;
            IsHuman = true;
        }

        private void Init()
        {
            Squads = new List<Squad>();
            IsActive = true;
            PlayerName = "Player";
            AvatarIndex = 0;
            HouseColorIndex = -1;
            LobbySlotIndex = -1;
            IsReady = false;
            Connected = true;
            OnDefeated = delegate(Team team)
            {
                IsActive = false;
                EliminatedTime = CompassUI.Instance.Timers.ElapsedGameTime;
                SkirmishVictoryManager.OnTeamDefeated(this);
                
                // vision toggle for eliminated local teams
                if (this == FracNet.Instance.LocalTeam && IsHuman)
                {
                    var units = Object.FindObjectsOfType<UnitManager>();
                    foreach (var unit in units)
                    {
                        var renders = unit.GetComponentsInChildren<Renderer>(true);
                        foreach (var r in renders)
                        {
                            r.enabled = true;
                        }
                    }

                    var structures = Object.FindObjectsOfType<StructureManager>();
                    foreach (var s in structures)
                    {
                        StructureManager.EliminationToggle(s);
                    }

                    var cam = Object.FindObjectOfType<CommonCameraController>();
                    cam.gameObject.GetComponent<ShroudCamera>().enabled = false;
                }
            };
            
            receivedMessages = new HashSet<uint>();
            OnDefeated += MultiplayerEventBroadcaster.TeamDefeated;
        }

        public Squad GetIdleSquad()
        {
            if (Squads == null || Squads.Count == 0)
                return null;

            for (var i = 0; i < Squads.Count; i++)
            {
                if (Squads[i].IsIdle)
                {
                    return Squads[i];
                }
            }
            return null;
        }

        public void UpdatePlayerName(string name)
        {
            PlayerName = name;
        }

        public void UpdateFaction(int index)
        {
            FactionIndex = index;
            var factionName = index < XmlCacheManager.Factions.Values.Length
                ? XmlCacheManager.Factions.Values[index].Name
                : "faction.spectator";
            Faction = factionName;
            IsSpectator = factionName == "faction.spectator";
            mutators = new List<IMutator>();
            // re-initialize recruit manager for AI teams in order to build correct list
            // of available units
            if (!IsHuman)
            {
                RecruitManager = new ReinforcementManager(this);
            }
            // update music tracks if we're updating our own faction
            else if (FracNet.Instance.LocalTeam == this)
            {
                MusicManager.Instance.SetPlayerFaction(factionName);
            }
        }

        public void UpdateSide(int side)
        {
            Side = side;
        }
        
        public void UpdatePlayerAvatar(int index)
        {
            AvatarIndex = index;
        }

        public void UpdateTeamColor(int index)
        {
            var current = HouseColorIndex;
            HouseColorIndex = index;
            var name = index < XmlCacheManager.HouseColors.Values.Length ? XmlCacheManager.HouseColors.Values[index].Name : null;
            TeamColorSelect.SwapTeamColor(current, index);
            LobbyManager.UpdatePlayerColor(this, index);
            if (name != null)
                TeamColor = XmlCacheManager.HouseColors[name];
        }

        public void AddMutator(string mutator)
        {
            var t = Type.GetType($"FracturedState.Game.Mutators.{mutator}");
            mutators.Add(Activator.CreateInstance(t, null) as IMutator);
        }

        public void RemoveMutator(string mutator)
        {
            mutators.Remove(mutators.First(m => m.GetType().Name == mutator));
        }

        public void Reset()
        {
            IsActive = true;
            TeamPopulation = 0;
            SquadPopulation = 0;
            mutators = new List<IMutator>();
        }

        public bool HasMutator(string name)
        {
            return mutators.SingleOrDefault(m => m.GetType().ToString() == name) != null;
        }

        public void SetReadyState(bool ready)
        {
            IsReady = ready;
            LobbyManager.UpdatePlayerReady(this, ready);
        }

        public void AddReceivedMessage(uint msg)
        {
            receivedMessages.Add(msg);
        }

        public void RemoveReceivedMessage(uint msg)
        {
            receivedMessages.Remove(msg);
        }

        public bool HasReceivedMessage(uint msg)
        {
            return receivedMessages.Contains(msg);
        }

        public void Disconnect()
        {
            Connected = false;
        }
    }
}