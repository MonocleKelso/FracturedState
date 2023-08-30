using FracturedState.Game.Management;
using FracturedState.Game.Network;
using UnityEngine;
using UnityEngine.UI;

namespace FracturedState.UI.Events
{
    public class TerritoryBolsterWidget : MonoBehaviour
    {
        [SerializeField] private Text text;
        [SerializeField] private Text score;
        [SerializeField] private Text territoryTotal;

        public void SetTerritoryName(string tName)
        {
            text.text = tName;
            var territory = TerritoryManager.Instance.GetTerritoryByName(tName);
            if (territory == null) return;
            score.text = TerritoryManager.Instance.GetTeamScoreForTerritory(territory, FracNet.Instance.LocalTeam).ToString();
            territoryTotal.text = TerritoryManager.Instance.GetStructuresInTerritory(territory).Count.ToString();
        }
    }
}