using System.Reflection;
using UnityEngine;

namespace FracturedState
{
    [System.Serializable]
    public class KeyBindingConfiguration
    {
        public static KeyBindingConfiguration DefaultConfiguration
        {
            get
            {
                return new KeyBindingConfiguration()
                {
                    ToggleTacticalMode = new KeyBinding(KeyCode.Tab, "keybind.tacticalmode"),
                    ToggleRecruitPanel = new KeyBinding(KeyCode.BackQuote, "keybind.togglerecruitpanel"),
                    ToggleFacingMove = new KeyBinding(KeyCode.LeftShift, "keybind.togglefacingmove"),
                    MoveCamUp = new KeyBinding(KeyCode.W, "keybind.movecamup"),
                    MoveCamDown = new KeyBinding(KeyCode.S, "keybind.movecamdown"),
                    MoveCamLeft = new KeyBinding(KeyCode.A, "keybind.movecamleft"),
                    MoveCamRight = new KeyBinding(KeyCode.D, "keybind.movecamright"),
                    RotateCamClockWise = new KeyBinding(KeyCode.Q, "keybind.camrotclockwise"),
                    RotateCamCounterClockWise = new KeyBinding(KeyCode.E, "keybind.camrotcounterclockwise"),
                    ShowHelperUI = new KeyBinding(KeyCode.LeftAlt, "keybind.showhelperui")
                };
            }
        }

        public static KeyBinding GetKeyBindByName(KeyBindingConfiguration src, string name)
        {
            PropertyInfo prop = typeof(KeyBindingConfiguration).GetProperty(name);
            if (prop != null)
            {
                return prop.GetValue(src, null) as KeyBinding;
            }
            return null;
        }

        public static void SetKeyBinding(KeyBindingConfiguration src, string keyBindName, KeyCode key)
        {
            PropertyInfo prop = typeof(KeyBindingConfiguration).GetProperty(keyBindName);
            if (prop != null)
            {
                PropertyInfo[] props = typeof(KeyBindingConfiguration).GetProperties();
                foreach (var propInfo in props)
                {
                    if (propInfo.Name != keyBindName)
                    {
                        KeyBinding kb = propInfo.GetValue(src, null) as KeyBinding;
                        if (kb != null && kb.Key == key)
                        {
                            return;
                        }
                    }
                }
                KeyBinding keyBind = prop.GetValue(src, null) as KeyBinding;
                keyBind.Key = key;
            }
        }

        public KeyBinding ToggleTacticalMode { get; set; }
        public KeyBinding ToggleRecruitPanel { get; set; }
        public KeyBinding ToggleFacingMove { get; set; }
        public KeyBinding MoveCamUp { get; set; }
        public KeyBinding MoveCamDown { get; set; }
        public KeyBinding MoveCamLeft { get; set; }
        public KeyBinding MoveCamRight { get; set; }
        public KeyBinding RotateCamClockWise { get; set; }
        public KeyBinding RotateCamCounterClockWise { get; set; }
        public KeyBinding ShowHelperUI { get; set; }
    }
}