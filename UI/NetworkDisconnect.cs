using FracturedState.Game.Network;
using UnityEngine;

namespace FracturedState.UI
{
    public class NetworkDisconnect : MonoBehaviour
    {
        public void Disconnect()
        {
            FracNet.Instance.Disconnect();
        }
    }
}