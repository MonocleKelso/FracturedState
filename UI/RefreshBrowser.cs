using FracturedState.Game.Network;
using UnityEngine;

namespace FracturedState.UI
{
    public class RefreshBrowser : MonoBehaviour
    {
        public void Refresh()
        {
            if (FracNet.Instance.IsInternetMatch)
            {
                FracNet.Instance.StartInternetListener(this);
            }
            else
            {
                FracNet.Instance.StartLocalListener();
            }
        }
    }
}