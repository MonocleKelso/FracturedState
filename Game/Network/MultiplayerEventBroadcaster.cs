using FracturedState.Game.Data;
using UnityEngine;
using FracturedState.Game.Management;
using FracturedState.UI;
using FracturedState.UI.Events;

namespace FracturedState.Game.Network
{
    public static class MultiplayerEventBroadcaster
    {
        private static EventMessenger eventMessenger;

        private static void NetworkChatMessage(string msg)
        {
            FracNet.Instance.NetworkActions.CmdAddChatEvent(msg);
        }

        private static void LocalChatMessage(string msg)
        {
            Chat.AddMessage(msg);
        }

        public static void EventMessage(string msg)
        {
            if (eventMessenger == null)
            {
                eventMessenger = Object.FindObjectOfType<EventMessenger>();
            }
            eventMessenger.AddMessage(msg);
        }
        
        public static void PlayerJoined(string playerName)
        {
            NetworkChatMessage($"{playerName} has joined the game");
        }

        public static void PlayerLeft(string playerName)
        {
            NetworkChatMessage($"{playerName} has left the game");
        }

        public static void HostWantsStart()
        {
            NetworkChatMessage("The host wishes to proceed but not all players are ready");
        }

        public static void HostStarted()
        {
            NetworkChatMessage("The host has started the match");
        }

        public static void TooManyPlayers()
        {
            LocalChatMessage("There are more players in the lobby than this map supports");
        }

        public static void NeedMap()
        {
            LocalChatMessage("You do not have the map that the host has selected. It will be transferred to you automatically");
        }

        public static void PlayersNeedMapTransfer(string playerName)
        {
            LocalChatMessage($"{playerName} does not have the currently selected map. It will be transferred to them automatically");
        }

        public static void TeamDefeated(Team team)
        {
            EventCanvas.PlayerDefeated(team.PlayerName);
        }

        public static void CaptureBuilding(string territoryName)
        {
            EventCanvas.TerritoryBolster(territoryName);
        }

        public static void LoseBuilding(string buildingName)
        {
            EventCanvas.LoseStructure(buildingName);
        }

        public static void UnlockUnit(UnitObject unit)
        {
            EventCanvas.UnlockUnit(unit);
        }

        public static void LockUnit(UnitObject unit)
        {
            EventCanvas.LockUnit(unit);
        }

        public static void LoseTerritory(string territoryName)
        {
            EventCanvas.TerritoryLost(territoryName);
        }

        public static void GainTerritory(string territoryName)
        {
            EventCanvas.CaptureTerritory(territoryName);
        }

        public static void GainTerritory(string playerName, string territoryName)
        {
            EventCanvas.CaptureTerritory(playerName, territoryName);
        }

        public static void Text(string text)
        {
            EventCanvas.TextEvent(text);
        }
    }
}