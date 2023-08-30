using System.Collections.Generic;
using FracturedState.Game.Management;
using FracturedState.UI;
using UnityEngine.Networking;
using UnityEngine;

namespace FracturedState.Game.Network
{
    public class FracDiscovery : NetworkDiscovery
    {
        const float PingWaitTime = 5;

        public Dictionary<string, DiscoveredGame> DiscoveredGames { get; private set; }

        string tmpData;
        bool dataChangeProc;

        Coroutine listener;

        void Awake()
        {
            useNetworkManager = false;
            showGUI = false;
            broadcastData = SkirmishVictoryManager.GameName + "|||";
            DiscoveredGames = new Dictionary<string, DiscoveredGame>();
        }

        void OnEnable()
        {
            listener = StartCoroutine(Listen());
        }

        void OnDisable()
        {
            if (listener != null)
            {
                StopCoroutine(listener);
                listener = null;
            }
        }

        System.Collections.IEnumerator Listen()
        {
            List<string> toRemove = new List<string>();
            while (true)
            {
                if (running && isClient)
                {
                    var keys = DiscoveredGames.Keys.GetEnumerator();
                    while (keys.MoveNext())
                    {
                        var data = DiscoveredGames[keys.Current];
                        if (Time.time - data.PingTime > PingWaitTime)
                        {
                            toRemove.Add(keys.Current);
                        }
                    }
                    for (int i = 0; i < toRemove.Count; i++)
                    {
                        DiscoveredGames.Remove(toRemove[i]);
                    }
                    toRemove.Clear();
                }
                yield return null;
            }
        }

        public void SetData(string data)
        {
            tmpData = data;
            if (!dataChangeProc)
            {
                StartCoroutine(UpdateData());
            }
        }

        public override void OnReceivedBroadcast(string fromAddress, string data)
        {
            base.OnReceivedBroadcast(fromAddress, data);
            DiscoveredGames[fromAddress] = new DiscoveredGame(fromAddress, data);
//            if (MatchBrowser.OnGotGames != null)
//                MatchBrowser.OnGotGames.Invoke(DiscoveredGames);
        }

        System.Collections.IEnumerator UpdateData()
        {
            dataChangeProc = true;
            if (running)
            {
                StopBroadcast();
            }
            while (NetworkTransport.IsBroadcastDiscoveryRunning())
            {
                yield return null;
            }
            while (tmpData.Length < broadcastData.Length)
            {
                tmpData += " ";
            }
            broadcastData = tmpData;
            StartAsServer();
            dataChangeProc = false;
        }
    }
}