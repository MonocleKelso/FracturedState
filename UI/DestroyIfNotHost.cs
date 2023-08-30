using FracturedState.Game.Network;
using UnityEngine;

namespace FracturedState.UI
{
    public class DestroyIfNotHost : MonoBehaviour
    {
        private void Start()
        {
            if (!FracNet.Instance.IsHost)
            {
                Destroy(gameObject);
            }
        }
    }
}