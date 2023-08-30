using System;
using System.Collections.Generic;
using System.Web;
using ExitGames.Client.Photon.LoadBalancing;
using FracturedState.Game.Network;
using UnityEngine;
using UnityEngine.Events;

namespace FracturedState.UI
{
    public class MatchBrowser : MonoBehaviour
    {
        public class GotGamesEvent : UnityEvent<Dictionary<string, DiscoveredGame>> { }

        public static GotGamesEvent OnGotGames = new GotGamesEvent();

        [SerializeField] private MatchEntry matchPrefab;

        private void Awake()
        {
            OnGotGames.AddListener(ProcessGames);
        }

        private void ProcessGames(Dictionary<string, DiscoveredGame> games)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (child.GetComponent<MatchEntry>() != null)
                {
                    Destroy(child.gameObject);
                }
            }
            
            if (games == null) return;

            var keys = games.Keys.GetEnumerator();
            while (keys.MoveNext())
            {
                if (keys.Current != null)
                {
                    var gameInfo = games[keys.Current];
                    var entry = Instantiate(matchPrefab, transform);
                    entry.SetInfo(gameInfo);
                }
            }
            keys.Dispose();
        }

        private void OnDestroy()
        {
            OnGotGames.RemoveListener(ProcessGames);
        }
    }
}