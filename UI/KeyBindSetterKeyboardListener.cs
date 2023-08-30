using System;
using System.Linq;
using UnityEngine;

namespace FracturedState.UI
{
    /// <summary>
    /// A UI class used to listen for keyboard input in order to relay that input back to a key binder which saves to the player's settings
    /// </summary>
    public class KeyBindSetterKeyboardListener : MonoBehaviour
    {
        // a list of all the keys we want to listen for that excludes left, right, and middle mouse click
        private static KeyCode[] allKeys = Enum.GetValues(typeof(KeyCode)).Cast<KeyCode>().Except(new KeyCode[] { KeyCode.Mouse0, KeyCode.Mouse1, KeyCode.Mouse2 }).ToArray();

        private KeyBindSetter setter;

        private void Awake()
        {
            setter = GetComponent<KeyBindSetter>();
        }

        private void Update()
        {
            foreach (var key in allKeys)
            {
                if (Input.GetKeyDown(key))
                {
                    setter.BindKey(key);
                    Destroy(this);
                    break;
                }
            }
        }
    }
}