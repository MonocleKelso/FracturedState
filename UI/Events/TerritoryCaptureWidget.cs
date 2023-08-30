using UnityEngine;
using UnityEngine.UI;

namespace FracturedState.UI.Events
{
    public class TerritoryCaptureWidget : MonoBehaviour
    {
        [SerializeField] private Text title;
        [SerializeField] private Text territoryName;

        public void SetTerritoryName(string tName)
        {
            territoryName.text = tName;
        }

        public void SetPlayerName(string playerName)
        {
            title.text = $"{playerName} HAS CAPTURED A TERRITORY";
        }
    }
}