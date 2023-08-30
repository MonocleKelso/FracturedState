using UnityEngine;
using UnityEngine.UI;

namespace FracturedState.UI
{
    public class PregameSlotStatus : MonoBehaviour
    {
        private HostGameSetup host;

        private void Awake()
        {
            host = GetComponentInParent<HostGameSetup>();
            var dropdown = GetComponent<Dropdown>();
            if (dropdown != null)
            {
                dropdown.onValueChanged.AddListener(UpdateStatus);
            }
        }

        private void UpdateStatus(int index)
        {
            LobbySlotStatus newStatus = index == 0 ? LobbySlotStatus.Open : LobbySlotStatus.Closed;
            host.UpdateStatus(transform.GetSiblingIndex(), newStatus);
        }

        private void OnDestroy()
        {
            var dropdown = GetComponent<Dropdown>();
            if (dropdown != null)
            {
                dropdown.onValueChanged.RemoveListener(UpdateStatus);
            }
        }
    }
}