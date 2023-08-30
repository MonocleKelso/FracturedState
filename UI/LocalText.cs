using UnityEngine;
using UnityEngine.UI;

namespace FracturedState.UI
{
    [RequireComponent(typeof(Text))]
    public class LocalText : MonoBehaviour
    {
        [SerializeField] private string key;

        private void Awake()
        {
            if (!string.IsNullOrEmpty(key))
            {
                GetComponent<Text>().text = LocalizedString.GetString(key);
            }
        }
    }
}