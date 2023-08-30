using FracturedState.Game.Network;
using UnityEngine;

namespace FracturedState.UI
{
    public class StartSinglePlayerMatch : SwapCurrentMenuButton
    {
        [SerializeField] private GameObject lobby;

        public void StartMatch()
        {
            FracNet.Instance.StartSinglePlayer();
            SwapHideBars(lobby);
        }
    }
}