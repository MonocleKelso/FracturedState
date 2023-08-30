using FracturedState.Game;
using UnityEngine;
using UnityEngine.UI;

namespace FracturedState.UI
{
    public class PlayerProfileName : MonoBehaviour
    {
        [SerializeField] private InputField input;

        private void Awake()
        {
            var profile = ProfileManager.GetActiveProfile();
            input.text = profile.PlayerName;
        }

        private void OnDestroy()
        {
            var profile = ProfileManager.GetActiveProfile();
            profile.SaveSettings(input.text, profile.BuiltInAvatarIndex);
            ProfileManager.SaveCurrentProfile(profile);
        }
    }
}