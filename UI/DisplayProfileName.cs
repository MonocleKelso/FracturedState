using FracturedState.Game;
using UnityEngine;
using UnityEngine.UI;

namespace FracturedState.UI
{
    public class DisplayProfileName : MonoBehaviour
    {
        [SerializeField] private Text text;

        private void Awake()
        {
            text.text = ProfileManager.GetActiveProfile().PlayerName;
        }
    }
}