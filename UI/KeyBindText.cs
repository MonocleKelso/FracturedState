using FracturedState.Game;
using UnityEngine;
using UnityEngine.UI;

namespace FracturedState.UI
{
    /// <summary>
    /// Displays the string description of the action a key binding is associated with
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class KeyBindText : MonoBehaviour
    {
        [SerializeField] private KeyBindSetter.KeyBindTypes bindAction;

        private void Awake()
        {
            var binding = ProfileManager.GetActiveProfile().KeyBindConfig;
            var keyBind = KeyBindingConfiguration.GetKeyBindByName(binding, bindAction.ToString());
            GetComponent<Text>().text = LocalizedString.GetString(keyBind.Description);
        }
    }
}