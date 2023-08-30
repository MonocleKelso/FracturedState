using UnityEngine;
using UnityEngine.UI;

namespace FracturedState.UI
{
    [RequireComponent(typeof(Toggle))]
    public class FullScreenToggle : MonoBehaviour
    {
        private void Awake()
        {
            var toggle = GetComponent<Toggle>();
            toggle.isOn = UnityEngine.Screen.fullScreen;
            toggle.onValueChanged.AddListener(on =>
            {
                UnityEngine.Screen.fullScreen = on;
                PlayerPrefs.SetInt("Screenmanager Is Fullscreen mode", System.Convert.ToInt32(on));
                PlayerPrefs.Save();
            });
        }

        private void OnDestroy()
        {
            var toggle = GetComponent<Toggle>();
            toggle.onValueChanged.RemoveAllListeners();
        }
    }
}