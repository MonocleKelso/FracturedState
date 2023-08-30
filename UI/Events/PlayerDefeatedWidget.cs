using UnityEngine;
using UnityEngine.UI;

namespace FracturedState.UI.Events
{
    public class PlayerDefeatedWidget : MonoBehaviour
    {
        [SerializeField] private Text playerName;

        public void SetPlayerName(string player)
        {
            playerName.text = $"{player} HAS BEEN DEFEATED";
        }
    }
}