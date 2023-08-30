using FracturedState.Game;
using FracturedState.Game.Data;
using UnityEngine;

namespace FracturedState.UI
{
    [RequireComponent(typeof(UnityEngine.UI.Image))]
    public class CurrentAvatar : MonoBehaviour
    {
        private void Awake()
        {
            var profile = ProfileManager.GetActiveProfile();
            SetAvatar("", profile.BuiltInAvatarIndex);
            ProfileManager.OnPlayerInfoChanged.AddListener(SetAvatar);
        }

        private void SetAvatar(string name, int avatarIndex)
        {
            GetComponent<UnityEngine.UI.Image>().sprite = AvailableAvatars.Instance.Avatars[avatarIndex];
        }

        private void OnDestroy()
        {
            ProfileManager.OnPlayerInfoChanged.RemoveListener(SetAvatar);
        }
    }
}