using FracturedState.Game.Network;
using UnityEngine;

namespace FracturedState.UI
{
    public class DisconnectOnAwake : MonoBehaviour
    {
        private void Awake()
        {
            FracNet.Instance.Disconnect();
        }
    }
}