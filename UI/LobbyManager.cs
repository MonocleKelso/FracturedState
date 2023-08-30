using UnityEngine;
using System.Collections.Generic;
using FracturedState.Game;
using FracturedState.Game.Management;

namespace FracturedState.UI
{
    public class LobbyManager : MonoBehaviour
    {
        public static LobbyManager Instance { get; private set; }
        private static LobbySlotStatus[] pregameStatus;

        [SerializeField] private PlayerLobbyStatus playerSlot;

        private List<PlayerLobbyStatus> lobbySlots;

        private void Awake()
        {
            Instance = this;
            lobbySlots = new List<PlayerLobbyStatus>();
            for (int i = 0; i < 8; i++)
            {
                lobbySlots.Add(Instantiate(playerSlot, transform));
            }
            MapSelect.Init();
            if (pregameStatus != null)
            {
                for (int i = 0; i < 8; i ++)
                {
                    lobbySlots[i].SetStatus(pregameStatus[i]);
                }
                pregameStatus = null;
            }
        }

        public static void ApplyPregameStatus(LobbySlotStatus[] status)
        {
            pregameStatus = status;
            if (Instance == null) return;
            for (int i = 0; i < 8; i++)
            {
                Instance.lobbySlots[i].SetStatus(pregameStatus[i]);
            }
            pregameStatus = null;
        }

        public static void UpdatePlayerFaction(Team team, int faction)
        {
            if (Instance == null) return;
            foreach (var slot in Instance.lobbySlots)
            {
                if (slot.OwnerTeam == team)
                {
                    slot.SetFaction(faction);
                }
            }

            if (faction >= XmlCacheManager.Factions.Values.Length)
            {
                Chat.AddMessage($"{team.PlayerName} became a spectator");
            }
            else
            {
                Chat.AddMessage($"{team.PlayerName} updated their faction to {LocalizedString.GetString(XmlCacheManager.Factions.Values[faction].Name)}");
            }
        }

        public static void UpdatePlayerName(Team team, string playerName)
        {
            if (Instance == null) return;
            foreach (var slot in Instance.lobbySlots)
            {
                if (slot.OwnerTeam == team)
                {
                    slot.SetName(playerName);
                }
            }
        }

        public static void UpdatePlayerReady(Team team, bool ready)
        {
            if (Instance == null) return;
            foreach (var slot in Instance.lobbySlots)
            {
                if (slot.OwnerTeam == team)
                {
                    slot.SetReady(ready);
                }
            }
        }

        public static void UpdatePlayerColor(Team team, int color)
        {
            if (Instance == null) return;
            foreach (var slot in Instance.lobbySlots)
            {
                if (slot.OwnerTeam == team)
                {
                    slot.SetColor(color);
                }
            }
            Chat.AddMessage($"{team.PlayerName} updated their color to {XmlCacheManager.HouseColors.Values[color].Name}");
        }

        public static void UpdatePlayerSide(Team team, int side)
        {
            if (Instance == null) return;
            foreach (var slot in Instance.lobbySlots)
            {
                if (slot.OwnerTeam == team)
                {
                    slot.UpdateSide(side);
                }
            }
            Chat.AddMessage($"{team.PlayerName} updated their team to {side + 1}");
        }

        public PlayerLobbyStatus GetSlotAtIndex(int index)
        {
            return lobbySlots[index];
        }

        public PlayerLobbyStatus GetNextSlot()
        {
            foreach (var slot  in lobbySlots)
            {
                if (slot.Status == LobbySlotStatus.Open)
                {
                    return slot;
                }
            }
            return null;
        }

        public int[] GetCurrentSlotStatus()
        {
            int[] status = new int[lobbySlots.Count];
            for (int i = 0; i < lobbySlots.Count; i++)
            {
                status[i] = (int)lobbySlots[i].Status;
            }
            return status;
        }

        public int IndexOfSlot(PlayerLobbyStatus slot)
        {
            return lobbySlots.IndexOf(slot);
        }

        public void UpdateSlotStatus(int index, int status)
        {
            lobbySlots[index].SetStatus((LobbySlotStatus)status);
        }

        private void OnDestroy()
        {
            Instance = null;
        }
    }
}