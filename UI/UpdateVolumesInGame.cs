using FracturedState.Game;
using UnityEngine;

namespace FracturedState.UI
{
    public class UpdateVolumesInGame : MonoBehaviour
    {
        private const float DefaultVolume = 0.4f;
        
        public void DoUpdate()
        {
            var volume = ProfileManager.GetEffectsVolumeFromProfile();
            var units = FindObjectsOfType<UnitManager>();
            foreach (var unit in units)
            {
                var unitAudio = unit.gameObject.GetComponent<AudioSource>();
                if (unitAudio != null)
                {
                    unitAudio.volume = DefaultVolume * volume;
                }
            }
        }
    }
}