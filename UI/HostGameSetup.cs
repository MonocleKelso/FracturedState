using UnityEngine;

namespace FracturedState.UI
{
    public class HostGameSetup : MonoBehaviour
    {
        private string gameName;
        private string gamePassword;
        private LobbySlotStatus[] slotStatus;

        private void Awake()
        {
            slotStatus = new LobbySlotStatus[8];
            for (int i = 0; i < 8; i++)
            {
                slotStatus[i] = LobbySlotStatus.Open;
            }
        }

        public void UpdateStatus(int slot, LobbySlotStatus status)
        {
            slotStatus[slot] = status;
        }

        public void ApplySetup()
        {
            LobbyManager.ApplyPregameStatus(slotStatus);
        }
    }
}