using UnityEngine;
using UnityEngine.Networking.Match;

namespace FracturedState.Game.Network
{
    public struct DiscoveredGame
    {
        public string Data { get; private set; }

        public string Address;
        public string RawData;
        public float PingTime;
        public MatchInfoSnapshot MatchData;

        public DiscoveredGame(string address, string data) : this()
        {
            Address = address;
            RawData = data;
            PingTime = Time.time;
            MatchData = null;
            Data = data;
        }

        public DiscoveredGame(MatchInfoSnapshot matchData) : this()
        {
            Address = "";
            PingTime = Time.time;
            RawData = matchData.name;
            MatchData = matchData;
            Data = matchData.name;
        }
    }
}