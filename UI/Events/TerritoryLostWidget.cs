using UnityEngine;
using UnityEngine.UI;

namespace FracturedState.UI.Events
{
    public class TerritoryLostWidget : MonoBehaviour
    {
        [SerializeField] private Text territoryName;

        public void SetTerritoryName(string tName)
        {
            territoryName.text = tName;
        }
    }
}