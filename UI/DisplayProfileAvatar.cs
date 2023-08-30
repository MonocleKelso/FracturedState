using FracturedState.Game;
using FracturedState.Game.Data;
using UnityEngine;

namespace FracturedState.UI
{
    public class DisplayProfileAvatar : MonoBehaviour
    {
        [SerializeField] private UnityEngine.UI.Image img;

        private void Awake()
        {
            img.sprite = AvailableAvatars.Instance.Avatars[ProfileManager.GetActiveProfile().BuiltInAvatarIndex];
        }
    }
}