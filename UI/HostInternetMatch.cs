using FracturedState.Game.Network;
using UnityEngine.Networking;

namespace FracturedState.UI
{
    public class HostInternetMatch : HostMatch
    {
        protected override void StartServer()
        {
            FracNet.Instance.StartInternetServer(this);
        }

        protected override bool IsReady()
        {
            return NetworkManager.singleton != null && NetworkManager.singleton.matchMaker != null;
        }
    }
}