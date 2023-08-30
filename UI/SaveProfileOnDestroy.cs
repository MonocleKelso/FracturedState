using FracturedState.Game;
using UnityEngine;

namespace FracturedState.UI
{
    public class SaveProfileOnDestroy : MonoBehaviour
    {
        private void OnDestroy()
        {
            ProfileManager.SaveCurrentProfile(ProfileManager.GetActiveProfile());
        }
    }
}