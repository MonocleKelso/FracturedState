using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace FracturedState.UI
{
    /// <summary>
    /// Provides the backing logic for a dropdown that will display a list of available resolutions for the player's current monitor. This
    /// also facilitates switching resolutions when the user picks a new resolution
    /// </summary>
    [RequireComponent(typeof(UnityEngine.UI.Dropdown))]
    public class ScreenResolutionSelect : MonoBehaviour
    {
        [SerializeField] private GameObject refreshMenu;

        private static string[] resPresentation;
        private static Resolution[] availResolutions;
        /// <summary>
        /// Caches a unique list of resolutions available on the currently active display
        /// </summary>
        public static void StoreAvailableResolutions()
        {
            // process only resolutions above 1024x768
            var resList = UnityEngine.Screen.resolutions.Where(r => r.width > 1024 && r.height > 768);
            // for some reason Unity contains duplicate entries for available resolutions so grab only unique ones
            var unique = new Dictionary<string, Resolution>();
            foreach (var res in resList)
            {
                unique[res.ToString()] = res;
            }
            // sort unique keys by comparing width and then height of their representative resolutions
            var sortedKeys = unique.Keys.ToList();
            sortedKeys.Sort((a, b) => {
                int diff = unique[a].width.CompareTo(unique[b].width);
                if (diff != 0) return diff;
                return unique[a].height.CompareTo(unique[b].height);
            });
            availResolutions = new Resolution[sortedKeys.Count];
            resPresentation = new string[sortedKeys.Count];
            for (int i = 0; i < sortedKeys.Count; i++)
            {
                resPresentation[i] = sortedKeys[i];
                availResolutions[i] = unique[sortedKeys[i]];
            }
        }

        private void Awake()
        {
            if (resPresentation == null || availResolutions == null)
            {
                StoreAvailableResolutions();
            }
            int width = 0;
            int height = 0;
            var refreshRate = UnityEngine.Screen.currentResolution.refreshRate;
            if (UnityEngine.Screen.fullScreen)
            {
                var curRes = UnityEngine.Screen.currentResolution;
                width = curRes.width;
                height = curRes.height;
            }
            else
            {
                width = UnityEngine.Screen.width;
                height = UnityEngine.Screen.height;
            }
            var dropdown = GetComponent<UnityEngine.UI.Dropdown>();
            int selectedIndex = 0;
            for (int i = 0; i < availResolutions.Length; i++)
            {
                var res = availResolutions[i];
                if (width == res.width && height == res.height && (res.refreshRate == refreshRate || Mathf.Abs(refreshRate - res.refreshRate) == 1))
                {
                    selectedIndex = i;
                }
            }
            dropdown.AddOptions(new List<string>(resPresentation));
            dropdown.value = selectedIndex;
            dropdown.onValueChanged.AddListener((sel) =>
            {
                var selRes = availResolutions[sel];
                StartCoroutine(Refresh(selRes));
            });
        }

        private IEnumerator Refresh(Resolution selRes)
        {
            PlayerPrefs.SetInt("Screenmanager Resolution Width", selRes.width);
            PlayerPrefs.SetInt("Screenmanager Resolution Height", selRes.height);
            PlayerPrefs.Save();
            yield return new WaitForSeconds(0.25f);
            UnityEngine.Screen.SetResolution(selRes.width, selRes.height, UnityEngine.Screen.fullScreen);
            yield return null;
            MenuContainer.DisplayMenu(refreshMenu);
        }

        private void OnDestroy()
        {
            GetComponent<UnityEngine.UI.Dropdown>().onValueChanged.RemoveAllListeners();
        }
    }
}