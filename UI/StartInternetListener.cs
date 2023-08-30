using FracturedState.Game.Network;
using UnityEngine;

namespace FracturedState.UI
{
    public class StartInternetListener : MonoBehaviour
    {
        private void Awake()
        {
            FracNet.Instance.StartInternetListener(this);
        }
    }
}