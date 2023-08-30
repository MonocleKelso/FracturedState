using System;
using System.Collections.Generic;
using FracturedState.Game;
using UnityEngine;
using UnityEngine.UI;

namespace FracturedState.UI
{
    [RequireComponent(typeof(Dropdown))]
    public class ReflectionQualitySelect : MonoBehaviour
    {
        private void Awake()
        {
            var options = new List<string>();
            var qVals = Enum.GetValues(typeof(ProfileGameSettings.ReflectionQualityOptions));
            foreach (var opt in Enum.GetNames(typeof(ProfileGameSettings.ReflectionQualityOptions)))
            {
                options.Add(opt);
            }
            var dropdown = GetComponent<Dropdown>();
            dropdown.AddOptions(options);
            var quality = ProfileManager.GetActiveProfile().GameSettings.ReflectionQuality;
            dropdown.value = Array.IndexOf(qVals, quality);
            dropdown.onValueChanged.AddListener(i =>
            {
                var profile = ProfileManager.GetActiveProfile();
                profile.GameSettings.ReflectionQuality = (ProfileGameSettings.ReflectionQualityOptions)qVals.GetValue(i);
                ProfileManager.SaveCurrentProfile(profile);
            });
        }

        private void OnDestroy()
        {
            var dropdown = GetComponent<Dropdown>();
            if (dropdown != null)
            {
                dropdown.onValueChanged.RemoveAllListeners();
            }
        }
    }
}