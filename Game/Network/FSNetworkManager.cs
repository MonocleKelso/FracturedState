using FracturedState.Game.Management;
using FracturedState.UI;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

namespace FracturedState.Game.Network
{
    public class FSNetworkManager : PhotonNetworkManager
    {
        FracDiscovery networkDiscovery;

        public PhotonMatchmaker Matchmaker
        {
            get
            {
                return matchMaker as PhotonMatchmaker;
            }
        }
        
        public bool DiscoveryActive
        {
            get
            {
                return networkDiscovery != null && networkDiscovery.running;
            }
        }

        private Dictionary<string, DiscoveredGame> internetGames;
        public Dictionary<string, DiscoveredGame> DiscoveredGames
        {
            get
            {
                return DiscoveryActive ? networkDiscovery.DiscoveredGames : internetGames;
            }
        }

        public MatchInfo CurrentMatch { get; set; }
        
        public override void OnServerConnect(NetworkConnection conn)
        {
            if (SkirmishVictoryManager.MatchInProgress)
            {
                conn.Disconnect();
                return;
            }

            base.OnServerConnect(conn);
            conn.SetChannelOption(0, ChannelOption.MaxPendingBuffers, 32);
            if (SkirmishVictoryManager.SkirmishTeams != null && SkirmishVictoryManager.SkirmishTeams.Count > 0)
            {
                var lobby = new LobbySetup()
                {
                    GameName = SkirmishVictoryManager.GameName,
                    Players = new LobbySetup.PlayerInfo[SkirmishVictoryManager.SkirmishTeams.Count]
                };
                for (int i = 0; i < SkirmishVictoryManager.SkirmishTeams.Count; i++)
                {
                    var team = SkirmishVictoryManager.SkirmishTeams[i];
                    lobby.Players[i] = new LobbySetup.PlayerInfo(team.PlayerName, team.AvatarIndex, team.FactionIndex,
                        team.HouseColorIndex, team.LobbySlotIndex, team.NetworkedPlayerId, team.IsReady);
                }
                lobby.MapName = MapSelect.CurrentMapName;
                lobby.MapHash = MapSelect.CurrentMapHash;
                lobby.Slots = LobbyManager.Instance.GetCurrentSlotStatus();
                conn.Send(GameConstants.MessageType.LobbySetup, lobby);
            }
        }

        public override void OnClientConnect(NetworkConnection conn)
        {
            base.OnClientConnect(conn);
            conn.SetChannelOption(0, ChannelOption.MaxPendingBuffers, 32);
        }

        public override void OnClientDisconnect(NetworkConnection conn)
        {
            base.OnClientDisconnect(conn);
            if (SkirmishVictoryManager.MatchInProgress)
            {
                CompassUI.Instance.gameObject.SetActive(false);
                MusicManager.Instance.PlayMainTheme();
                SkirmishVictoryManager.PostGameWorldCleanUp();
            }
            FracNet.Instance.Disconnect();
            MenuContainer.FlushMenuCache();
            MenuContainer.SetCurrentMenu(null);
            Instantiate(MenuContainer.Container);
            MenuContainer.Showbars();
        }

        public override void OnClientError(NetworkConnection conn, int errorCode)
        {
            base.OnClientError(conn, errorCode);
            if (!SkirmishVictoryManager.MatchInProgress)
            {
                FracNet.Instance.Disconnect();
                MenuContainer.FlushMenuCache();
                MenuContainer.SetCurrentMenu(null);
                Instantiate(MenuContainer.Container);
                MenuContainer.Showbars();
            }
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            SkirmishVictoryManager.OnMatchComplete += delegate (Team winner)
            {
                Loader.Instance.StartCoroutine(Loader.Instance.InformMatchEnd(winner));
            };
        }

        public FracDiscovery GetNetworkDiscovery()
        {
            if (networkDiscovery != null) return networkDiscovery;
            networkDiscovery = GetComponent<FracDiscovery>();
            if (networkDiscovery == null)
            {
                networkDiscovery = gameObject.AddComponent<FracDiscovery>();
                networkDiscovery.Initialize();
            }
            else if (!networkDiscovery.enabled)
            {
                networkDiscovery.enabled = true;
            }
            return networkDiscovery;
        }

        public void StopDiscovery()
        {
            if (networkDiscovery == null) return;
            if (networkDiscovery.running)
                networkDiscovery.StopBroadcast();

            Destroy(networkDiscovery);
        }

        public void SetDiscoveryData(string data)
        {
            if (networkDiscovery != null)
            {
                networkDiscovery.SetData(data);
            }
        }

        public void ParseInternetGameList(bool success, string extendedInfo, List<MatchInfoSnapshot> games)
        {
            if (!success) return;
            internetGames = new Dictionary<string, DiscoveredGame>();
            if (games == null) return;
            foreach (var t in games)
            {
                internetGames[t.name] = new DiscoveredGame(t);
            }
            if (MatchBrowser.OnGotGames != null)
                MatchBrowser.OnGotGames.Invoke(internetGames);
        }

        public void ResetInternetGamesList()
        {
            internetGames?.Clear();
            internetGames = null;
            if (MatchBrowser.OnGotGames != null)
                MatchBrowser.OnGotGames.Invoke(internetGames);
        }
    }
}