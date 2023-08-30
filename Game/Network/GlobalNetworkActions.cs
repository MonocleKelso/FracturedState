using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FracturedState.Game.Data;
using FracturedState.Game.Management;
using FracturedState.UI;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

namespace FracturedState.Game.Network
{
    public class GlobalNetworkActions : NetworkBehaviour
    {
        private static readonly Dictionary<Team, GlobalNetworkActions> TeamLookup = new Dictionary<Team, GlobalNetworkActions>();

        public static GlobalNetworkActions GetActions(Team team)
        {
            if (!team.IsHuman)
                return null;

            GlobalNetworkActions a;
            if (TeamLookup.TryGetValue(team, out a))
            {
                return a;
            }
            throw new FracturedStateException("Cannot look up actions for " + team.PlayerName + " make sure they are properly assigned");
        }

        public Team LocalTeam { get; private set; }

        private List<byte> inProcessMapTransfer;

        [Command]
        public void CmdLockStepReceived(uint msgId)
        {
            LockStepProcessor.Instance.ReceiveMessage(this, msgId);
        }

        [ClientRpc]
        public void RpcProcessLockStepMsg(uint msgId)
        {
            if (!isLocalPlayer) return;
            FracNet.Instance.NetworkActions.LocalTeam.RemoveReceivedMessage(msgId);
            LockStepProcessor.Instance.ProcessMessage(msgId);
        }

        #region Player/Team Management
        [Command]
        public void CmdUpdateLobbySlot(int slotIndex, int status)
        {
            RpcUpdateLobbySlot(slotIndex, status);
        }

        [ClientRpc]
        private void RpcUpdateLobbySlot(int slotIndex, int status)
        {
            LobbyManager.Instance.UpdateSlotStatus(slotIndex, status);
        }

        [Command]
        public void CmdAddPlayer(string playerName, int avatar)
        {
            if (!SkirmishVictoryManager.MatchInProgress)
                RpcAddPlayer(playerName, avatar, (int)netId.Value);
        }

        [ClientRpc]
        private void RpcAddPlayer(string playerName, int avatar, int connectionId)
        {
            var team = new Team(connectionId, playerName, avatar);
            LocalTeam = team;
            TeamLookup[team] = this;
            if (isLocalPlayer)
            {
                FracNet.Instance.LocalTeam = team;
                if (MapSelect.NeedsMapTransfer)
                {
                    CmdRequestMapTransfer();
                }
            }
            SkirmishVictoryManager.AddTeam(team);
            SkirmishVictoryManager.ResetTeamReadiness();
        }

        [Command]
        public void CmdRemovePlayer(int connectionId)
        {
            RpcRemovePlayer(connectionId);
            var team = SkirmishVictoryManager.GetTeam(connectionId);
            if (team != null)
            {
                CmdUpdateLobbySlot(team.LobbySlotIndex, (int)LobbySlotStatus.Open);
            }
        }

        [ClientRpc]
        private void RpcRemovePlayer(int connectionId)
        {
            var team = SkirmishVictoryManager.GetTeam(connectionId);
            if (team == null) return;
            SkirmishVictoryManager.RemoveTeam(team);
            SkirmishVictoryManager.ResetTeamReadiness();
        }

        [Command]
        public void CmdAddAIPlayer(string playerName)
        {
            RpcAddAIPlayer(playerName);
        }

        [ClientRpc]
        private void RpcAddAIPlayer(string playerName)
        {
            var team = new Team();
            team.UpdatePlayerName(playerName);
            SkirmishVictoryManager.AddTeam(team);
            SkirmishVictoryManager.ResetTeamReadiness();
            if (FracNet.Instance.IsHost)
            {
                AISimulator.Instance.AddTeam(team);
            }
        }

        [Command]
        public void CmdAddAIPlayerToSlot(int index, string playerName)
        {
            RpcAddAIPLayerToSlot(index, playerName);
        }

        [ClientRpc]
        private void RpcAddAIPLayerToSlot(int index, string playerName)
        {
            var team = new Team();
            team.UpdatePlayerName(playerName);
            team.LobbySlotIndex = index;
            SkirmishVictoryManager.AddTeam(team);
            SkirmishVictoryManager.ResetTeamReadiness();
            if (FracNet.Instance.IsHost)
            {
                AISimulator.Instance.AddTeam(team);
            }
        }

        [Command]
        public void CmdRemoveAITeam(string playerName)
        {
            RpcRemoveAITeam(playerName);
        }

        [ClientRpc]
        private void RpcRemoveAITeam(string playerName)
        {
            var team = SkirmishVictoryManager.GetTeam(playerName);
            if (team != null)
            {
                SkirmishVictoryManager.RemoveTeam(team);
                SkirmishVictoryManager.ResetTeamReadiness();
                if (FracNet.Instance.IsHost)
                {
                    AISimulator.Instance.RemoveTeam(team);
                }
                // AI players need to be renamed so that removing and adding ones out of order doesn't
                // result in duplicate player names
                RenameAIPlayers();
            }
        }

        private void RenameAIPlayers()
        {
            var count = 1;
            for (var i = 0; i < SkirmishVictoryManager.SkirmishTeams.Count; i++)
            {
                var team = SkirmishVictoryManager.SkirmishTeams[i];
                if (!team.IsHuman)
                {
                    var name = "AI Opponent " + count;
                    team.UpdatePlayerName(name);
                    LobbyManager.UpdatePlayerName(team, name);
                    count++;
                }
            }
        }

        [Command]
        public void CmdUpdateFaction(int faction)
        {
            RpcUpdateFaction(faction);
        }

        [ClientRpc]
        private void RpcUpdateFaction(int faction)
        {
            SkirmishVictoryManager.ResetTeamReadiness();
            LocalTeam.UpdateFaction(faction);
            LobbyManager.UpdatePlayerFaction(LocalTeam, faction);
        }

        [Command]
        public void CmdUpdateAIFaction(string playerName, int faction)
        {
            RpcUpdateAIFaction(playerName, faction);
        }

        [ClientRpc]
        private void RpcUpdateAIFaction(string playerName, int faction)
        {
            SkirmishVictoryManager.ResetTeamReadiness();
            var team = SkirmishVictoryManager.GetTeam(playerName);
            if (team == null) return;
            
            team.UpdateFaction(faction);
            LobbyManager.UpdatePlayerFaction(team, faction);
        }

        [Command]
        public void CmdUpdateSide(int side)
        {
            RpcUpdateSide(side);
        }

        [ClientRpc]
        private void RpcUpdateSide(int side)
        {
            SkirmishVictoryManager.ResetTeamReadiness();
            LocalTeam.UpdateSide(side);
            LobbyManager.UpdatePlayerSide(LocalTeam, side);
        }

        [Command]
        public void CmdUpdateAISide(string playerName, int side)
        {
            RpcUpdateAISide(playerName, side);
        }

        [ClientRpc]
        private void RpcUpdateAISide(string playerName, int side)
        {
            SkirmishVictoryManager.ResetTeamReadiness();
            var team = SkirmishVictoryManager.GetTeam(playerName);
            if (team == null) return;
            
            team.UpdateSide(side);
            LobbyManager.UpdatePlayerSide(team, side);
        }
        
        [Command]
        public void CmdUpdateColor(int color)
        {
            RpcUpdateColor(color);
        }

        [ClientRpc]
        private void RpcUpdateColor(int color)
        {
            SkirmishVictoryManager.ResetTeamReadiness();
            LocalTeam.UpdateTeamColor(color);
        }

        [Command]
        public void CmdUpdateAIColor(string playerName, int color)
        {
            RpcUpdateAIColor(playerName, color);
        }

        [ClientRpc]
        private void RpcUpdateAIColor(string playerName, int color)
        {
            SkirmishVictoryManager.ResetTeamReadiness();
            var team = SkirmishVictoryManager.GetTeam(playerName);
            team?.UpdateTeamColor(color);
        }

        [Command]
        public void CmdMakeReady()
        {
            RpcMakeReady();
        }

        [ClientRpc]
        private void RpcMakeReady()
        {
            LocalTeam.SetReadyState(true);
            LobbyManager.UpdatePlayerReady(LocalTeam, true);
        }

        [Command]
        public void CmdAddMutator(string mutator)
        {
            RpcAddMutator(mutator);
        }

        [ClientRpc]
        private void RpcAddMutator(string mutator)
        {
            LocalTeam.AddMutator(mutator);
            SkirmishVictoryManager.ResetTeamReadiness();
        }

        [Command]
        public void CmdRemoveMutator(string mutator)
        {
            RpcRemoveMutator(mutator);
        }

        [ClientRpc]
        private void RpcRemoveMutator(string mutator)
        {
            LocalTeam.RemoveMutator(mutator);
            SkirmishVictoryManager.ResetTeamReadiness();
        }

        

        [ClientRpc]
        public void RpcEndMatch()
        {
            var compass = GameObject.Find("Compass Camera");
            if (compass != null)
            {
                compass.SetActive(false);
            }
            SkirmishVictoryManager.SetWinningTeam(LocalTeam);
            MenuContainer.ShowEndGame();
            PlayWinLossMusic();
            SkirmishVictoryManager.PostGameWorldCleanUp();
        }

        [ClientRpc]
        public void RpcEndMatchAI(string winner)
        {
            GameObject.Find("Compass Camera").SetActive(false);
            SkirmishVictoryManager.SetWinningTeam(SkirmishVictoryManager.GetTeam(winner));
            MenuContainer.ShowEndGame();
            PlayWinLossMusic();
            SkirmishVictoryManager.PostGameWorldCleanUp();
        }

        [ClientRpc]
        public void RpcStartElimination()
        {
            Loader.Instance.StartCoroutine(Loader.Instance.EvalTeamElimination(LocalTeam));
        }

        [ClientRpc]
        public void RpcStartEliminationAi(string player)
        {
            Loader.Instance.StartCoroutine(Loader.Instance.EvalTeamElimination(SkirmishVictoryManager.GetTeam(player)));
        }
        
        [Command]
        public void CmdSurrender(float time)
        {
            RpcSurrender(time);
        }

        [ClientRpc]
        private void RpcSurrender(float time)
        {
            LocalTeam.SurrenderTime = time;
            LocalTeam.OnDefeated(LocalTeam);
        }
        
        private static void PlayWinLossMusic()
        {
            if (SkirmishVictoryManager.WinningTeam == FracNet.Instance.LocalTeam)
            {
                MusicManager.Instance.PlayWinTrack();
            }
            else
            {
                MusicManager.Instance.PlayLoseTrack();
            }
        }

        #endregion

        #region Spawning

        [Command]
        public void CmdSpawnSquad(string[] units, string territory)
        {
            var unitList = new List<UnitObject>();
            foreach (var u in units)
            {
                unitList.Add(XmlCacheManager.Units[u]);
            }

            var req = new SquadRequest(LocalTeam, unitList) {Territory = territory};
            FracNet.Instance.CallReinforcements(req);
        }

        [Command]
        public void CmdCombineSquads(NetworkInstanceId[] unitIds)
        {
            var msgId = LockStepProcessor.Instance.GetNextMessageId();
            RpcCombineSquads(msgId, unitIds);
        }

        [ClientRpc]
        private void RpcCombineSquads(uint msgId, NetworkInstanceId[] unitIds)
        {
            var units = new List<UnitManager>();
            foreach (var id in unitIds)
            {
                var unit = ClientScene.FindLocalObject(id);
                if (unit == null) continue;

                var um = unit.GetComponent<UnitManager>();
                if (um == null) continue;
                
                units.Add(um);
            }
            
            var msg = new CombineSquadMessage(units.ToArray()) {Id = msgId};
            LockStepProcessor.Instance.AddMessage(msg);
            CmdLockStepReceived(msgId);
        }

        #endregion

        #region Map Management

        [Command]
        public void CmdChangeMap(string mapName, int mapHash)
        {
            RpcChangeMap(mapName, mapHash);
        }

        [ClientRpc]
        private void RpcChangeMap(string mapName, int mapHash)
        {
            SkirmishVictoryManager.ResetTeamReadiness();
            var hasMap = MapSelect.SetCurrentMap(mapName, mapHash);
            Chat.AddMessage($"The map has been changed to {mapName}");
            if (!hasMap)
            {
                MultiplayerEventBroadcaster.NeedMap();
                FracNet.Instance.NetworkActions.CmdRequestMapTransfer();
            }
        }

        [Command]
        public void CmdRequestMapTransfer()
        {
            FracNet.Instance.AddTeamToMapTransfer(LocalTeam);
            MultiplayerEventBroadcaster.PlayersNeedMapTransfer(LocalTeam.PlayerName);
        }

        public IEnumerator HostLoadMap(List<Team> mapTransferPlayers)
        {
            MultiplayerEventBroadcaster.HostStarted();
            SkirmishVictoryManager.ResetTeamReadiness();
            yield return null;
            /*
            if (mapTransferPlayers != null && mapTransferPlayers.Count > 0)
            {
                MultiplayerEventBroadcaster.TransferringMaps();
                yield return null;
                var mapTransfer = new MapTransfer();
                mapTransfer.WriteMap(SkirmishVictoryManager.CurrentMap);
                // if this map is small enough to transfer in a single call
                if (mapTransfer.MapData.Length <= 64000)
                {
                    foreach (var team in mapTransferPlayers)
                    {
                        NetworkConnection conn = GetActions(team).connectionToClient;
                        conn.SendByChannel(GameConstants.MessageType.MapTransfer, mapTransfer, 2);
                    }
                }
                // otherwise split into multiple calls
                else
                {
                    foreach (var team in mapTransferPlayers)
                    {
                        GetActions(team).RpcStartBigMapTransfer();
                    }
                    yield return null;
                    int start = 0;
                    int length = 0;
                    while (start < mapTransfer.MapData.Length)
                    {
                        if (start + 1300 > mapTransfer.MapData.Length)
                        {
                            length = mapTransfer.MapData.Length - start;
                        }
                        else
                        {
                            length = 1300;
                        }
                        byte[] sendData = new byte[length];
                        System.Array.Copy(mapTransfer.MapData, start, sendData, 0, length);
                        start += length;
                        foreach (var team in mapTransferPlayers)
                        {
                            GetActions(team).RpcBigMapTransfer(sendData);
                        }
                        yield return null;
                    }
                    foreach (var team in mapTransferPlayers)
                    {
                        GetActions(team).RpcFinalizeBigMapTransfer();
                    }
                    yield return null;

                }
                bool ready = false;
                while (!ready)
                {
                    ready = true;
                    for (int i = 0; i < mapTransferPlayers.Count; i++)
                    {
                        if (!mapTransferPlayers[i].IsReady)
                        {
                            ready = false;
                        }
                    }
                    yield return null;
                }
                SkirmishVictoryManager.ResetTeamReadiness();
            }
*/
            
            DoLoadMap(SkirmishVictoryManager.CurrentMap.MapName);
        }

        [ClientRpc]
        private void RpcStartBigMapTransfer()
        {
            inProcessMapTransfer = new List<byte>();
        }

        [ClientRpc]
        private void RpcBigMapTransfer(byte[] data)
        {
            inProcessMapTransfer.AddRange(data);
        }

        [ClientRpc]
        private void RpcFinalizeBigMapTransfer()
        {
            var transfer = new MapTransfer();
            transfer.MapData = inProcessMapTransfer.ToArray();
            SkirmishVictoryManager.CustomMap = transfer.ReadMap();
            inProcessMapTransfer.Clear();
            inProcessMapTransfer = null;
            CmdMakeReady();
        }

        private void DoLoadMap(string mapName)
        {
            RpcLoadmap(mapName);
        }

        [ClientRpc]
        private void RpcLoadmap(string mapName)
        {
            SkirmishVictoryManager.MatchInProgress = true;
            MenuContainer.ShowLoadingScreen();
            Chat.ClearMessages();
            var load = new GameObject("MapLoader");
            var mapLoad = load.AddComponent<MapLoader>();
            mapLoad.MapName = mapName;
        }

        private IEnumerator LoadMap(string mapName)
        {
            CompassUI.Instance.Init();
            yield return null;
            // if we have a custom map due to a map transfer then use it and ignore the map name passed in
            if (SkirmishVictoryManager.CustomMap != null)
            {
                SkirmishVictoryManager.CurrentMap = SkirmishVictoryManager.CustomMap;
                SkirmishVictoryManager.CustomMap = null;
            }
            else
            {
                SkirmishVictoryManager.CurrentMap = DataUtil.DeserializeXml<RawMapData>(DataLocationConstants.GameRootPath + DataLocationConstants.MapDirectory + "/" + mapName + "/" + "map.xml");
            }
            yield return StartCoroutine(DataUtil.LoadMap(SkirmishVictoryManager.CurrentMap));
            
            /*while (!Loader.Instance.ProbeRenderComplete)
            {
                yield return null;
            }*/
            FracNet.Instance.NetworkActions.CmdUpdateMapProgress(1);
            yield return null;

            if (FracNet.Instance.IsHost)
            {
                var ready = false;
                // ensure everyone has loaded the map before proceeding
                while (!ready)
                {
                    ready = true;
                    for (var i = 0; i < SkirmishVictoryManager.SkirmishTeams.Count; i++)
                    {
                        var team = SkirmishVictoryManager.SkirmishTeams[i];
                        if (team.IsHuman)
                        {
                            var bar = LoadingBarManager.GetBar(team);
                            if (bar == null || bar.Progress != 1)
                            {
                                ready = false;
                            }
                        }
                    }
                    yield return null;
                }
                // reset readines again to check before starting match
                SkirmishVictoryManager.ResetTeamReadiness();
                var startPoints = new List<int>();
                var s = 0;
                while (s < SkirmishVictoryManager.CurrentMap.StartingPoints.Length)
                {
                    startPoints.Add(s++);
                }
                for (var i = 0; i < SkirmishVictoryManager.SkirmishTeams.Count; i++)
                {
                    var team = SkirmishVictoryManager.SkirmishTeams[i];
                    var actions = GetActions(team);
                    if (!team.IsSpectator)
                    {

                        var faction = XmlCacheManager.Factions[team.Faction];
                        // set starting camera position for human players
                        Vector3 startPos;
                        var randPoint = Random.Range(0, startPoints.Count);
                        SkirmishVictoryManager.CurrentMap.StartingPoints[startPoints[randPoint]].TryVector3(out startPos);
                        startPoints.RemoveAt(randPoint);
                        if (team.IsHuman)
                        {
                            actions.RpcSetStartingLocation(startPos);
                        }
                        // spawn starting units
                        var startingUnits = new List<UnitObject>();
                        foreach (var unit in faction.StartingUnits)
                        {
                            for (var u = 0; u < unit.Count; u++)
                            {
                                startingUnits.Add(XmlCacheManager.Units[unit.Name]);
                            }
                        }
                        var sr = new SquadRequest(team, startingUnits);
                        sr.RallyPoint = startPos;
                        FracNet.Instance.CallReinforcements(sr);
                    }
                    else
                    {
                        // spectators just get their camera positions set to the first starting location
                        Vector3 startPos;
                        SkirmishVictoryManager.CurrentMap.StartingPoints[0].TryVector3(out startPos);
                        actions.RpcSetStartingLocation(startPos);
                    }
                }
                // loop all teams again and ping readiness which will just send a reply back
                for (var i = 0; i < SkirmishVictoryManager.SkirmishTeams.Count; i++)
                {
                    if (SkirmishVictoryManager.SkirmishTeams[i].IsHuman)
                        GetActions(SkirmishVictoryManager.SkirmishTeams[i]).RpcPingReady();
                }
                // wait to make sure all clients have received all messages
                var humanCount = SkirmishVictoryManager.SkirmishTeams.Count(t => t.IsHuman);
                while (SkirmishVictoryManager.SkirmishTeams.Count(t => t.IsHuman && t.IsReady) != humanCount)
                {
                    yield return null;
                }
                FracNet.Instance.NetworkActions.RpcBeginMatch();
            }
        }

        [Command]
        public void CmdUpdateMapProgress(float progress)
        {
            RpcUpdateMapProgress(progress);
        }

        [ClientRpc]
        private void RpcUpdateMapProgress(float progress)
        {
            var loadingBar = LoadingBarManager.GetBar(LocalTeam);
            if (loadingBar != null)
            {
                loadingBar.SetProgress(progress);
            }
        }

        [ClientRpc]
        public void RpcPingReady()
        {
            // this is basically just the host asking each client 'are you ready?'
            CmdMakeReady();
        }

        [ClientRpc]
        public void RpcSetStartingLocation(Vector3 startPos)
        {
            if (isLocalPlayer)
            {
                var camTran = GameObject.Find("MainCamParent").transform;
                var camDownAngle = camTran.Find("Main Camera").transform.rotation.eulerAngles.x;
                var rad = (90f - camDownAngle) * Mathf.Deg2Rad;
                var z = camTran.position.y * Mathf.Tan(rad);
                camTran.position = new Vector3(startPos.x, ConfigSettings.Instance.Values.CameraDefaultHeight, startPos.z - z);
            }
        }

        [ClientRpc]
        public void RpcBeginMatch()
        {
            MenuContainer.HideMenus();
            CursorSettings.Instance.StartCursorCheck();
            Camera.main.gameObject.GetComponent<CommonCameraController>().enabled = true;
            Camera.main.gameObject.GetComponent<WorldBackgroundRenderer>().enabled = true;
            var input = GameObject.Find("Input");
            input.GetComponent<SelectionManager>().enabled = true;
            input.GetComponent<CommandManager>().enabled = true;
            input.GetComponent<MicroManager>().enabled = true;
            input.GetComponent<UnitHotKeyManager>().enabled = true;
            if (FracNet.Instance.IsHost)
            {
                AISimulator.Instance.StartSimulation();
            }
            MusicManager.Instance.PlayAmbientLoop();
            SkillBarManager.Activate();
        }

        #endregion

        #region Territory Management

        [Command]
        private void CmdClearTerritories(int teamId)
        {
            RpcClearTerritories(teamId);
        }

        [ClientRpc]
        private void RpcClearTerritories(int teamId)
        {
            var team = SkirmishVictoryManager.GetTeam(teamId);
            var territories = TerritoryManager.Instance.GetOwnedTerritories(team);
            if (territories != null)
            {
                for (var i = territories.Count - 1; i >= 0; i--)
                {
                    TerritoryManager.Instance.RemoveTeamTerritory(team, territories[i]);
                }
                SkirmishVictoryManager.UpdateMatchStatus();
            }
        }

        #endregion

        #region Structure/Garrison Management

        [Command]
        public void CmdReleaseStructure(int structureId)
        {
            var msgId = LockStepProcessor.Instance.GetNextMessageId();
            RpcReleaseStructure(msgId, structureId);
        }

        [Command]
        public void CmdForceReleaseStructure(int teamId, int structureId)
        {
            var msgId = LockStepProcessor.Instance.GetNextMessageId();
            RpcForceReleaseStructure(msgId, teamId, structureId);
        }

        [ClientRpc]
        private void RpcForceReleaseStructure(uint msgId, int teamId, int structureId)
        {
            StructureRelease(msgId, SkirmishVictoryManager.GetTeam(teamId), ObjectUIDLookUp.Instance.GetStructureManager(structureId));
        }

        [ClientRpc]
        private void RpcReleaseStructure(uint msgId, int structureId)
        {
            StructureRelease(msgId, LocalTeam, ObjectUIDLookUp.Instance.GetStructureManager(structureId));
        }

        private void StructureRelease(uint id, Team team, StructureManager structure)
        {
            var msg = new StructureReleaseMessage(team, structure);
            msg.Id = id;
            LockStepProcessor.Instance.AddMessage(msg);
            FracNet.Instance.NetworkActions.CmdLockStepReceived(msg.Id);
        }

        [Command]
        public void CmdSpawnGarrisonUnit(string unitName, int structureId, string pointName)
        {
            var sm = ObjectUIDLookUp.Instance.GetStructureManager(structureId);
            var point = sm.GetExteriorChild(pointName);
            var newUnit = (GameObject)Instantiate(PrefabManager.NetworkUnitContainer, point.position, point.rotation);
            NetworkServer.SpawnWithClientAuthority(newUnit, gameObject);
            RpcSetGarrisonUnit(newUnit.GetComponent<NetworkIdentity>(), unitName, structureId, pointName);
        }

        [Command]
        public void CmdSpawnAIGarrisonUnit(string unitName, int structureId, string pointName, string playerName)
        {
            var sm = ObjectUIDLookUp.Instance.GetStructureManager(structureId);
            var point = sm.GetExteriorChild(pointName);
            var newUnit = (GameObject)Instantiate(PrefabManager.NetworkUnitContainer, point.position, point.rotation);
            NetworkServer.SpawnWithClientAuthority(newUnit, gameObject);
            RpcSetAIGarrisonUnit(newUnit.GetComponent<NetworkIdentity>(), unitName, structureId, pointName, playerName);
        }

        [ClientRpc]
        private void RpcSetGarrisonUnit(NetworkIdentity unit, string unitName, int structureId, string pointName)
        {
            unit.GetComponent<UnitMessages>().LocalCreate(unitName, LocalTeam.NetworkedPlayerId);
            var u = unit.GetComponent<UnitManager>();
            var sm = ObjectUIDLookUp.Instance.GetStructureManager(structureId);
            var g = sm.GetGarrisonPoint(pointName);
            g.SetCurrentUnit(u);
        }

        [ClientRpc]
        private void RpcSetAIGarrisonUnit(NetworkIdentity unit, string unitName, int structureId, string pointName, string playerName)
        {
            unit.GetComponent<UnitMessages>().LocalAICreate(unitName, playerName);
            var u = unit.GetComponent<UnitManager>();
            var sm = ObjectUIDLookUp.Instance.GetStructureManager(structureId);
            var g = sm.GetGarrisonPoint(pointName);
            g.SetCurrentUnit(u);
        }

        #endregion

        #region Chat and Text-based Events

        [Command]
        public void CmdAddChatMessage(string message)
        {
            RpcAddChatMessage(message);
        }

        [ClientRpc]
        private void RpcAddChatMessage(string message)
        {
            Chat.AddMessage(LocalTeam.PlayerName + ": " + message);
        }

        [Command]
        public void CmdAddChatEvent(string message)
        {
            RpcAddChatEvent(message);
        }

        [ClientRpc]
        private void RpcAddChatEvent(string message)
        {
            Chat.AddMessage(message);
        }

        [Command]
        public void CmdAddAllyChatEvent(string message)
        {
            var msg = $"{LocalTeam.PlayerName}: {message}";
            foreach (var team in TeamLookup.Keys)
            {
                if (team.Side == LocalTeam.Side)
                {
                    TeamLookup[team].RpcAddAllyChatEvent(msg);
                }
            }
        }

        [ClientRpc]
        public void RpcAddAllyChatEvent(string message)
        {
            IngameChatManager.Instance.AddEntry(message);
        }

        [Command]
        public void CmdAddTextEvent(string evt)
        {
            RpcAddTextEvent(evt);
        }

        [ClientRpc]
        private void RpcAddTextEvent(string evt)
        {
            MultiplayerEventBroadcaster.EventMessage(evt);
        }

        #endregion

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            if (isLocalPlayer)
            {
                FracNet.Instance.NetworkActions = this;
                var profile = ProfileManager.GetActiveProfile();
                CmdAddPlayer(profile.PlayerName, profile.BuiltInAvatarIndex);
                if (FracNet.Instance.IsHost && !SkirmishVictoryManager.IsMultiPlayer)
                {
                    FracNet.Instance.AddAiTeam();
                }
            }
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (!isLocalPlayer)
            {
                LocalTeam = SkirmishVictoryManager.GetTeam((int)netId.Value);
                if (LocalTeam != null)
                {
                    TeamLookup[LocalTeam] = this;
                }
            }
        }

        public override void OnNetworkDestroy()
        {
            base.OnNetworkDestroy();
            if (FracNet.Instance.IsHost)
            {
                if (SkirmishVictoryManager.MatchInProgress)
                {
                    var units = FindObjectsOfType<UnitManager>();
                    foreach (var unit in units)
                    {
                        if (unit.OwnerTeam == LocalTeam)
                        {
                            // send network message to deal max damage to all units
                            // this will kill them and trigger their appropriate world object releases
                            unit.NetMsg.CmdTakeDamage(int.MaxValue, null, Weapon.DummyName);
                        }
                    }
                    // force release tech buildings
                    // the combination of these two actions should release all of the player's territories and
                    // eliminate them from the match
                    var structures = FindObjectsOfType<StructureManager>();
                    foreach (var structure in structures)
                    {
                        if (structure.OwnerTeam == LocalTeam)
                        {
                            FracNet.Instance.NetworkActions.CmdForceReleaseStructure((int)netId.Value, structure.GetComponent<Identity>().UID);
                        }
                    }
                    // last pass to remove any system-assigned territories that wouldn't have cleared above
                    FracNet.Instance.NetworkActions.CmdClearTerritories((int)netId.Value);
                    LocalTeam.Disconnect();
                    var allUnits = FindObjectsOfType<UnitManager>();
                    foreach (var unit in allUnits)
                    {
                        if (unit.OwnerTeam == LocalTeam)
                        {
                            unit.NetMsg.CmdTakeDamage(int.MaxValue, null, Weapon.DummyName);
                        }
                    }
                    LocalTeam.OnDefeated(LocalTeam);
                }
                else
                {
                    FracNet.Instance.NetworkActions.CmdRemovePlayer((int)netId.Value);
                }
            }
        }
    }
}