using UnityEngine.Networking;

namespace FracturedState.Game.Network
{
    public class LobbySetup : MessageBase
    {
        public struct PlayerInfo
        {
            public string Name;
            public int Avatar;
            public int Faction;
            public int Color;
            public int Slot;
            public int ConnectionId;
            public bool IsHuman;
            public bool IsReady;

            public PlayerInfo(string name, int avatar, int faction, int color, int slot, int connectionId, bool isReady)
            {
                Name = name;
                Avatar = avatar;
                Faction = faction;
                Color = color;
                Slot = slot;
                ConnectionId = connectionId;
                IsReady = isReady;
                IsHuman = true;
            }

            public PlayerInfo(string name, int avatar, int faction, int color, int slot)
            {
                Name = name;
                Avatar = avatar;
                Faction = faction;
                Color = color;
                Slot = slot;
                ConnectionId = -1;
                IsReady = true;
                IsHuman = false;
            }
        }

        public string GameName;
        public PlayerInfo[] Players;
        public int[] Slots;
        public string MapName;
        public int MapHash;
    }
}