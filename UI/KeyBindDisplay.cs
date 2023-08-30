using FracturedState.Game;
using UnityEngine;
using UnityEngine.UI;

namespace FracturedState.UI
{
    /// <summary>
    /// A UI class used in conjunction with a Text component and a parent KeyBindSetter component that displays a string
    /// representation of a KeyCode used in a key binding
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class KeyBindDisplay : MonoBehaviour
    {
        private Text text;

        private void Awake()
        {
            text = GetComponent<Text>();
            var binder = GetComponentInParent<KeyBindSetter>();
            // set display for initial value
            UpdateDisplay(binder.BoundKey);
            // subscribe for updates
            binder.OnKeyBindChange += UpdateDisplay;
        }

        private void UpdateDisplay(KeyCode key)
        {
            text.text = key.ToString().PrettyPascal();
        }

        private void OnDestroy()
        {
            var binder = GetComponentInParent<KeyBindSetter>();
            if (binder != null)
            {
                binder.OnKeyBindChange -= UpdateDisplay;
            }
        }
    }
}