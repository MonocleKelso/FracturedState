using FracturedState.Game;
using UnityEngine;

namespace FracturedState.UI
{
    public class SetPlayerAvatar : MonoBehaviour
    {
        [SerializeField] private UnityEngine.UI.Button button;

        private int avatarIndex;

        public void SetIndex(int index)
        {
            avatarIndex = index;
        }

        private void Awake()
        {
            button.onClick.AddListener(SetProfileAvatar);
        }

        private void SetProfileAvatar()
        {
            var profile = ProfileManager.GetActiveProfile();
            profile.SaveSettings(profile.PlayerName, avatarIndex);
            ProfileManager.SaveCurrentProfile(profile);
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(SetProfileAvatar);
            }
        }
    }
}