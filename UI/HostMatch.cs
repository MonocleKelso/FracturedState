using System;
using System.Collections;
using FracturedState.Game.Management;
using FracturedState.Game.Network;
using UnityEngine;

namespace FracturedState.UI
{
    public abstract class HostMatch : MonoBehaviour
    {
        [SerializeField]protected GameObject hostScreen;
        
        public void Host()
        {
            if (string.IsNullOrEmpty(SkirmishVictoryManager.GameName)) return;

            var prompt = MenuContainer.ShowPrompt();
            prompt.StartCoroutine(CreateMatch(prompt));
        }

        private IEnumerator CreateMatch(LoadingPrompt prompt)
        {
            prompt.UpdateMessage("prompt.connecting");
            StartServer();
            if (FracNet.Instance.IsInternetMatch)
            {
                yield return null;
                while (!IsReady())
                    yield return null;
            
                yield return new WaitForSeconds(5);
                prompt.UpdateMessage("prompt.creatematch");
            }
            
            MapSelect.Init();
            FracNet.Instance.UpdateHostData((success, info, matchInfo) =>
            {
                if (!success)
                {
                
                    prompt.UpdateMessage("prompt.createfailed");
                    Destroy(prompt.gameObject, 5);
                }
                else
                {
                    Destroy(prompt.gameObject);
                    MenuContainer.DisplayMenu(hostScreen);
                }
            });
        }

        protected virtual void StartServer()
        {
            throw new NotImplementedException();
        }

        protected virtual bool IsReady()
        {
            return false;
        }
    }
}