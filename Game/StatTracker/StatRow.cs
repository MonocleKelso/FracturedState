using UnityEngine;
using UnityEngine.UI;

namespace FracturedState.Game.StatTracker
{
    public class StatRow : MonoBehaviour
    {
        [SerializeField] private Text playerName;
        [SerializeField] private Text created;
        [SerializeField] private Text killed;
        [SerializeField] private Text lost;
        [SerializeField] private Text maxBuildings;
        [SerializeField] private Text maxTerritories;
        [SerializeField] private Text eliminationTime;

        public void Init(TeamStats stats)
        {
            playerName.text = stats.OwnerTeam.PlayerName;
            playerName.color = XmlCacheManager.HouseColors.Values[stats.OwnerTeam.HouseColorIndex].UnityColor;
            created.text = stats.UnitMadeCount.ToString();
            killed.text = stats.KillCount.ToString();
            lost.text = stats.UnitLostCount.ToString();
            maxBuildings.text = stats.MaxStructuresOwned.ToString();
            maxTerritories.text = stats.MaxTerritoriesOwned.ToString();
            if (stats.OwnerTeam.EliminatedTime > 0)
            {
                var seconds = Mathf.RoundToInt(stats.OwnerTeam.EliminatedTime);
                eliminationTime.text = $"{seconds / 3600:00}:{(seconds / 60) % 60:00}:{seconds % 60:00}";
            }
        }
    }
}