using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using ExitGames.Client.Photon.LoadBalancing;
using FracturedState.Game.Management;
using FracturedState.UI;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

namespace FracturedState.Game.Network
{
    public sealed class FracNet
    {
        private static FracNet instance;
        public static FracNet Instance => instance ?? (instance = new FracNet());

        private FracNet() { }

        public bool IsHost { get; private set; }

        // this is a local alias for NetworkManager.singleton but is used for
        // single-init checks when performing network operations internal to this class
        private FSNetworkManager networkManager;
        public GlobalNetworkActions NetworkActions { get; set; }
        public Team LocalTeam { get; set; }

        private readonly List<Team> mapTransferPlayers = new List<Team>();

        public bool IsInternetMatch { get; private set; }

        private void InitManager()
        {
            if (networkManager != null) return;
            networkManager = Object.Instantiate(PrefabManager.NetManager, Vector3.zero, Quaternion.identity).GetComponent<FSNetworkManager>();
            NetworkManager.singleton = networkManager;
        }

        public void StartLocalServer()
        {
            Disconnect();
            InitManager();
            InitClientMessages(networkManager.StartHost());
            var disc = networkManager.GetNetworkDiscovery();
            if (disc.running)
            {
                disc.StopBroadcast();
            }
            disc.StartAsServer();
            IsHost = true;
            IsInternetMatch = false;
            SkirmishVictoryManager.IsMultiPlayer = true;
            SkirmishVictoryManager.OnTeamAdded = MultiplayerEventBroadcaster.PlayerJoined;
            SkirmishVictoryManager.OnTeamRemoved = MultiplayerEventBroadcaster.PlayerLeft;
        }

        public void StartInternetServer(MonoBehaviour caller)
        {
            InitManager();
            IsHost = true;
            IsInternetMatch = true;
            SkirmishVictoryManager.IsMultiPlayer = true;
            SkirmishVictoryManager.OnTeamAdded = MultiplayerEventBroadcaster.PlayerJoined;
            SkirmishVictoryManager.OnTeamRemoved = MultiplayerEventBroadcaster.PlayerLeft;
            caller.StartCoroutine(Matchmaker());
        }

        private IEnumerator Matchmaker()
        {
            yield return null;
            networkManager.StartMatchMaker();
        }

        public void StartLocalListener()
        {
            Disconnect();
            InitManager();
            var disc = networkManager.GetNetworkDiscovery();
            if (disc.running)
            {
                disc.StopBroadcast();
            }
            disc.StartAsClient();
        }

        public void StartInternetListener(MonoBehaviour caller)
        {
            Disconnect();
            InitManager();
            IsInternetMatch = true;
            caller.StartCoroutine(ListMatches());
        }

        private IEnumerator ListMatches()
        {
            yield return null;
            networkManager.StartMatchMaker();
            while (networkManager.matchMaker == null) yield return null;
            var mm = networkManager.Matchmaker;
            while (mm.client == null) yield return null;
            while (!mm.client.ready) yield return null;
            mm.client.OnEventAction -= GotServerData;
            mm.client.OnEventAction += GotServerData;
        }

        private void GotServerData(EventData evt)
        {
            if (evt.Code == EventCode.AppStats && !SkirmishVictoryManager.MatchInProgress)
            {
                networkManager.Matchmaker.ListMatches(0, 25, "", true, 0, 0, networkManager.ParseInternetGameList);
            }
        }

        public void Connect(MatchInfoSnapshot match, NetworkMatch.DataResponseDelegate<MatchInfo> callback)
        {
            networkManager.matchMaker.JoinMatch(match.networkId, "", "", "", 0, 0, (success, info, matchInfo) =>
            {
                if (success)
                {
                    InitClientMessages(networkManager.StartClient(matchInfo));
                    IsHost = false;
                    SkirmishVictoryManager.IsMultiPlayer = true;
                }

                callback?.Invoke(success, info, matchInfo);
            });
        }

        public void StopMatchMaker()
        {
            if (IsInternetMatch)
            {
                networkManager.StopMatchMaker();
            }
        }

        private void InitClientMessages(NetworkClient client)
        {
            client.RegisterHandler(GameConstants.MessageType.LobbySetup, OnLobbyPayload);
            client.RegisterHandler(GameConstants.MessageType.SpawnSquad, OnSpawnSquad);
            client.RegisterHandler(GameConstants.MessageType.MapTransfer, OnMapTransfer);
        }

        public Dictionary<string, DiscoveredGame> GetAvailableGames()
        {
            return networkManager.DiscoveredGames;
        }

        public void UpdateHostData(NetworkMatch.DataResponseDelegate<MatchInfo> callback)
        {
            if (!SkirmishVictoryManager.IsMultiPlayer) return;
            
            if (networkManager.DiscoveryActive)
            {
                callback?.Invoke(true, "", null);
            }
            else if (networkManager.CurrentMatch == null)
            {
                CreateMatchRequest(SkirmishVictoryManager.GameName, callback);
            }
        }

        private void CreateMatchRequest(string data, NetworkMatch.DataResponseDelegate<MatchInfo> callback)
        {
            networkManager.matchMaker.CreateMatch(data, 8, true, "", "", "", 0, 0, (succcess, info, matchInfo) =>
            {
                if (succcess)
                {
                    networkManager.CurrentMatch = matchInfo;
                    NetworkServer.Listen(matchInfo, 9000);
                    InitClientMessages(networkManager.StartHost(matchInfo));
                }

                callback?.Invoke(succcess, info, matchInfo);
            });
        }

        public void StartSinglePlayer()
        {
            Disconnect();
            SkirmishVictoryManager.GameName = "Single Player Skirmish";
            InitManager();
            InitClientMessages(networkManager.StartHost());
            IsHost = true;
        }

        public void AddAiTeam()
        {
            var count = SkirmishVictoryManager.SkirmishTeams != null ? SkirmishVictoryManager.SkirmishTeams.Count(t => !t.IsHuman) : 0;
            NetworkActions.CmdAddAIPlayer("AI Opponent " + (count + 1));
        }

        public void AddAiTeam(int slotIndex)
        {
            var count = SkirmishVictoryManager.SkirmishTeams != null ? SkirmishVictoryManager.SkirmishTeams.Count(t => !t.IsHuman) : 0;
            NetworkActions.CmdAddAIPlayerToSlot(slotIndex, "AI Opponent " + (count + 1));
        }

        public bool RemoveAiTeam(string name)
        {
            #if UNITY_EDITOR
            NetworkActions.CmdRemoveAITeam(name);
            return true;
            #else
            if (!SkirmishVictoryManager.IsMultiPlayer && SkirmishVictoryManager.SkirmishTeams.Count(t => !t.IsHuman) == 1)
                return false;
            NetworkActions.CmdRemoveAITeam(name);
            return true;
            #endif
        }

        public void Disconnect()
        {
            if (networkManager != null)
            {
                if (IsHost)
                {
                    networkManager.StopHost();
                }
                else
                {
                    networkManager.StopClient();
                }
                StopLan();
            }
            if (IsInternetMatch && networkManager!= null)
            {
                networkManager.StopMatchMaker();
                networkManager.ResetInternetGamesList();
                IsInternetMatch = false;
            }
            else if (networkManager != null)
            {
                networkManager.StopDiscovery();
            }
            if (SkirmishVictoryManager.SkirmishTeams != null)
            {
                SkirmishVictoryManager.SkirmishTeams.Clear();
                AISimulator.Instance.ClearTeams();
            }
            TeamColorSelect.Reset();
            SkirmishVictoryManager.IsMultiPlayer = false;
            SkirmishVictoryManager.MatchInProgress = false;
            SkirmishVictoryManager.OnTeamAdded = null;
            SkirmishVictoryManager.OnTeamRemoved = null;
            SkirmishVictoryManager.GameName = string.Empty;
        }

        public void StopLan()
        {
            var disc = networkManager.GetNetworkDiscovery();
            if (disc.running)
            {
                disc.StopBroadcast();
            }
        }

        public void MakeReady()
        {
            NetworkActions.CmdMakeReady();
        }

        public void ClearMapTransferRequests()
        {
            mapTransferPlayers.Clear();
        }

        public void AddTeamToMapTransfer(Team team)
        {
            if (!mapTransferPlayers.Contains(team))
            {
                mapTransferPlayers.Add(team);
            }
        }

        public void LoadMap(string mapName)
        {
            NetworkActions.StartCoroutine(NetworkActions.HostLoadMap(mapTransferPlayers));
        }

        public static void SetTeamElimination(Team team)
        {
            if (team.IsHuman)
            {
                GlobalNetworkActions.GetActions(team).RpcStartElimination();
            }
            else
            {
                instance.NetworkActions.RpcStartEliminationAi(team.PlayerName);
            }
        }

        public static void StopLocalWarning()
        {
            Loader.Instance.InterruptLocalWarning();
        }

        public void Surrender()
        {
            NetworkActions.CmdSurrender(CompassUI.Instance.Timers.ElapsedGameTime);
        }

        public void CallReinforcements(SquadRequest request)
        {
            if (IsHost)
            {
                var rallyPoint = !string.IsNullOrEmpty(request.Territory) ? TerritoryManager.Instance.GetTerritoryRallyPoint(request.Territory) : request.RallyPoint;
                var spawnPoint = TerritoryManager.GetClosestMapEdge(rallyPoint);
                var ss = new SpawnSquad
                {
                    TeamId = request.Owner.NetworkedPlayerId,
                    RallyPoint = rallyPoint,
                    UnitIds = new NetworkIdentity[request.Units.Count],
                    IsHuman = request.Owner.IsHuman
                };
                GameObject playerObj;
                if (request.Owner.IsHuman)
                {
                    playerObj = GlobalNetworkActions.GetActions(request.Owner).gameObject;
                }
                else
                {
                    playerObj = NetworkActions.gameObject;
                    ss.AITeamName = request.Owner.PlayerName;
                }
                
                for (var i = 0; i < request.Units.Count; i++)
                {
                    var pos = spawnPoint + (Vector3) Random.insideUnitCircle;
                    var newUnit = Object.Instantiate(PrefabManager.NetworkUnitContainer, pos, Quaternion.LookRotation(rallyPoint - pos));
                    NetworkServer.SpawnWithClientAuthority(newUnit, playerObj);
                    if (request.Owner.IsHuman)
                    {
                        newUnit.GetComponent<UnitMessages>().CmdCreateUnit(request.Units[i].Name, request.Owner.NetworkedPlayerId);
                    }
                    else
                    {
                        newUnit.GetComponent<UnitMessages>().CmdCreateAIUnit(request.Units[i].Name, request.Owner.PlayerName);
                    }
                    ss.UnitIds[i] = newUnit.GetComponent<NetworkIdentity>();
                }
                NetworkServer.SendToAll(GameConstants.MessageType.SpawnSquad, ss);
            }
            else
            {
                var units = new string[request.Units.Count];
                for (var i = 0; i < request.Units.Count; i++)
                {
                    units[i] = request.Units[i].Name;
                }
                NetworkActions.CmdSpawnSquad(units, request.Territory);
            }
        }

        public void SpawnGarrison(StructureManager structure, Team team, string unitName, string pointName)
        {
            if (team.IsHuman)
            {
                var actions = GlobalNetworkActions.GetActions(team);
                actions.CmdSpawnGarrisonUnit(unitName, structure.GetComponent<Identity>().UID, pointName);
            }
            else
            {
                NetworkActions.CmdSpawnAIGarrisonUnit(unitName, structure.GetComponent<Identity>().UID, pointName, team.PlayerName);
            }
        }

        public void ReleaseStructure(int structId, Team team)
        {
            var action = GlobalNetworkActions.GetActions(team);
            action.CmdReleaseStructure(structId);
        }

        /// <summary>
        /// Handler method for LobbySetup network messages. Updates new players with the current state of the game lobby
        /// </summary>
        /// <param name="msg"></param>
        void OnLobbyPayload(NetworkMessage msg)
        {
            var lobby = msg.ReadMessage<LobbySetup>();
            SkirmishVictoryManager.GameName = lobby.GameName;
            var status = new LobbySlotStatus[lobby.Slots.Length];
            for (var i = 0; i < status.Length; i++)
            {
                status[i] = (LobbySlotStatus)lobby.Slots[i];
            }
            LobbyManager.ApplyPregameStatus(status);
            foreach (var player in lobby.Players)
            {
                var team = player.IsHuman ? new Team(player.ConnectionId, player.Name, player.Avatar) : new Team();
                if (!player.IsHuman)
                {
                    team.UpdatePlayerName(player.Name);
                    team.UpdatePlayerAvatar(player.Avatar);
                }
                team.UpdatePlayerName(player.Name);
                team.UpdateFaction(player.Faction);
                team.UpdateTeamColor(player.Color);
                team.LobbySlotIndex = player.Slot;
                SkirmishVictoryManager.AddTeam(team);
            }
            var hasMap = MapSelect.SetCurrentMap(lobby.MapName, lobby.MapHash);
            if (!hasMap)
            {
                MultiplayerEventBroadcaster.NeedMap();
            }
            MenuContainer.HidePrompt();
        }

        /// <summary>
        /// Handler method for SpawnSquad network messages. Tells players to bind the given units into a squad
        /// </summary>
        /// <param name="msg"></param>
        private static void OnSpawnSquad(NetworkMessage msg)
        {
            var ss = msg.ReadMessage<SpawnSquad>();
            ss.BindSquad();
        }

        /// <summary>
        /// Handler method for a map transfer between host and player
        /// </summary>
        /// <param name="msg"></param>
        private void OnMapTransfer(NetworkMessage msg)
        {
            var transfer = msg.ReadMessage<MapTransfer>();
            SkirmishVictoryManager.CustomMap = transfer.ReadMap();
            NetworkActions.CmdMakeReady();
        }
    }
}