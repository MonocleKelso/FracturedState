using UnityEngine;
using UnityEngine.UI;

namespace FracturedState.UI
{
    public class CurrentMapDisplay : MonoBehaviour
    {
        [SerializeField] private Text mapName;

        [SerializeField] private Text mapPop;

        private void Awake()
        {
            SetMapInfo(MapSelect.CurrentMapName, MapSelect.CurrentMapPop);
            MapSelect.OnMapChanged.AddListener(SetMapInfo);
        }

        private void SetMapInfo(string selectedMapName, int pop)
        {
            mapName.text = selectedMapName;
            mapPop.text = pop.ToString();
        }

        private void OnDestroy()
        {
            MapSelect.OnMapChanged.RemoveListener(SetMapInfo);
        }
    }
}