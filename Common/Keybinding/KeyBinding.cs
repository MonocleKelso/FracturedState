using UnityEngine;

namespace FracturedState
{
    [System.Serializable]
    public class KeyBinding
    {
        public KeyCode Key { get; set; }
        public string Description { get; set; }

        public KeyBinding(KeyCode key, string description)
        {
            Key = key;
            Description = description;
        }
    }
}