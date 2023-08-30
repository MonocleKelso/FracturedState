using FracturedState.Game.Network;
using UnityEngine;

namespace FracturedState.UI
{
    public class StartLocalListener : MonoBehaviour
    {
        private void Awake()
        {
            FracNet.Instance.StartLocalListener();
        }
    }
}