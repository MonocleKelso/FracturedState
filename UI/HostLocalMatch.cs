using FracturedState.Game.Network;
using UnityEngine.Networking;

namespace FracturedState.UI
{
    public class HostLocalMatch : HostMatch
    {
        protected override void StartServer()
        {
            FracNet.Instance.StartLocalServer();
        }

        protected override bool IsReady()
        {
            return NetworkManager.singleton != null && ((FSNetworkManager)NetworkManager.singleton).DiscoveryActive;
        }
    }
}