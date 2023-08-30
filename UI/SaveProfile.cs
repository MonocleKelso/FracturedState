using FracturedState.Game;
using UnityEngine;

namespace FracturedState.UI
{
    public class SaveProfile : MonoBehaviour
    {
        public void Save()
        {
            ProfileManager.SaveCurrentProfile(ProfileManager.GetActiveProfile());
        }

        public void Reset()
        {
            ProfileManager.ResetProfile();
        }
    }
}