using System.Collections;
using ExitGames.Client.Photon.LoadBalancing;
using FracturedState.Game;
using FracturedState.Game.Network;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace FracturedState.UI
{
    public class MatchEntry : MonoBehaviour
    {
        [SerializeField] private Text gameName;
        [SerializeField] private GameObject lobby;

        private DiscoveredGame game;
        private LoadingPrompt prompt;

        public void SetInfo(DiscoveredGame game)
        {
            this.game = game;
            gameName.text = game.Data;
        }

        public void JoinMatch()
        {
            prompt = MenuContainer.ShowPrompt();
            prompt.StartCoroutine(Join());
        }

        private IEnumerator Join()
        {
            prompt.UpdateMessage("prompt.connecting");
            
            bool returned = false;
            bool connected = false;
            
            FracNet.Instance.Connect(game.MatchData, (success, info, data) =>
            {
                returned = true;
                connected = success;
            });
            while (!returned) yield return null;
            if (!connected)
            {
                prompt.UpdateMessage("prompt.joinfailed");
                Destroy(prompt.gameObject, 5);
            }
            else
            {
                MenuContainer.DisplayMenu(lobby);
                prompt.UpdateMessage("prompt.joining");
            }
        }
    }
}