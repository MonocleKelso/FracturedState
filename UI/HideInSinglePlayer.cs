using FracturedState.Game.Management;
using UnityEngine;

namespace FracturedState.UI
{
    public class HideInSinglePlayer : MonoBehaviour
    {
        private void Start()
        {
            if (!SkirmishVictoryManager.IsMultiPlayer)
            {
                gameObject.SetActive(false);
            }
        }
    }
}