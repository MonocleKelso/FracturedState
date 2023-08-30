using System;
using FracturedState.Game;
using UnityEngine;
using UnityEngine.UI;

namespace FracturedState.UI
{
    [RequireComponent(typeof(Slider))]
    public class VolumeSlider : MonoBehaviour
    {
        private enum VolumeType { Music, Effect, UI }

        [SerializeField] private VolumeType volumeType;

        private void Awake()
        {
            var profile = ProfileManager.GetActiveProfile();
            var slide = GetComponent<Slider>();
            switch (volumeType)
            {
                case VolumeType.Effect:
                    slide.value = profile.GameSettings.EffectsVolume;
                    slide.onValueChanged.AddListener(vol =>
                    {
                        profile.GameSettings.EffectsVolume = vol;
                    });
                    break;
                case VolumeType.Music:
                    slide.value = profile.GameSettings.MusicVolume;
                    slide.onValueChanged.AddListener(vol =>
                    {
                        profile.GameSettings.MusicVolume = vol;
                        MusicManager.Instance.SetVolume(vol);
                    });
                    break;
                case VolumeType.UI:
                    slide.value = profile.GameSettings.UIVolume;
                    slide.onValueChanged.AddListener(vol =>
                    {
                        profile.GameSettings.UIVolume = vol;
                        InterfaceSoundPlayer.UpdateVolume(vol);
                        UnitBarkManager.Instance.UpdateVolume(vol);
                    });
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnDestroy()
        {
            var slide = GetComponent<Slider>();
            slide.onValueChanged.RemoveAllListeners();
        }
    }
}