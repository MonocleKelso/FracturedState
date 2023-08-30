using FracturedState.Game.Network;
using UnityEngine;
using UnityEngine.UI;

namespace FracturedState.UI
{
    [RequireComponent(typeof(Button))]
    public class MapChoice : MonoBehaviour
    {
        [SerializeField] private Text mapName;

        [SerializeField] private Text playerCount;

        private void Awake()
        {
            GetComponent<Button>().interactable = FracNet.Instance.IsHost;
        }

        public void SetMapName(string selectedMapName)
        {
            this.mapName.text = selectedMapName;
        }

        public void SetPlayerCount(string count)
        {
            playerCount.text = count;
        }

        public void SelectMap()
        {
            MapSelect.UpdateMapChoice(transform.GetSiblingIndex());
        }
    }
}