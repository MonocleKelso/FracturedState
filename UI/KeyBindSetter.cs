using System.Collections.Generic;
using FracturedState.Game;
using UnityEngine;
using Btn = UnityEngine.UI.Button;

namespace FracturedState.UI
{
    public delegate void KeyBindChangeDelegate(KeyCode key);

    /// <summary>
    /// A UI class used in conjunction with a Button component that handles binding keys to ingame actions
    /// </summary>
    [RequireComponent(typeof(Btn))]
    public class KeyBindSetter : MonoBehaviour
    {
        public enum KeyBindTypes
        {
            ToggleTacticalMode,
            ToggleRecruitPanel,
            ToggleFacingMove,
            MoveCamUp,
            MoveCamDown,
            MoveCamLeft,
            MoveCamRight,
            RotateCamClockWise,
            RotateCamCounterClockWise,
            ShowHelperUI
        }

        // static list to track binding button instances so we can toggle on/off when binding
        private static List<Btn> keyBindButtons = new List<Btn>();

        public KeyCode BoundKey { get; private set; }
        public KeyBindChangeDelegate OnKeyBindChange;

        [SerializeField] private KeyBindTypes bindAction;

        private KeyBindingConfiguration bindConfig;

        private void Awake()
        {
            bindConfig = ProfileManager.GetActiveProfile().KeyBindConfig;
            var binding = KeyBindingConfiguration.GetKeyBindByName(bindConfig, bindAction.ToString());
            BoundKey = binding.Key;
            // fire this here in case something in the scene is already somehow listening
            if (OnKeyBindChange != null)
            {
                OnKeyBindChange(BoundKey);
            }
            keyBindButtons.Add(GetComponent<Btn>());
        }

        /// <summary>
        /// Sets the currently active binding to this instance by adding a listener component to
        /// this component's GameObject
        /// </summary>
        public void SetCurrentBindingAction()
        {
            gameObject.AddComponent<KeyBindSetterKeyboardListener>();
            SetBindButtonEnabledState(false);
        }

        /// <summary>
        /// Binds the given key to the action represented by this instance
        /// </summary>
        public void BindKey(KeyCode key)
        {
            KeyBindingConfiguration.SetKeyBinding(bindConfig, bindAction.ToString(), key);
            if (OnKeyBindChange != null)
            {
                OnKeyBindChange(key);
            }
            SetBindButtonEnabledState(true);
        }

        private void SetBindButtonEnabledState(bool enabled)
        {
            for (int i = 0; i < keyBindButtons.Count; i++)
            {
                keyBindButtons[i].enabled = enabled;
            }
        }

        private void OnDestroy()
        {
            // remove any leftover delegates that are still listening to this event
            if (OnKeyBindChange != null)
            {
                var dels = OnKeyBindChange.GetInvocationList();
                foreach (var del in dels)
                {
                    OnKeyBindChange -= (KeyBindChangeDelegate)del;
                }
            }
            keyBindButtons.Remove(GetComponent<Btn>());
        }
    }
}