using UnityEngine;
using UnityEngine.UI;

namespace FracturedState.UI
{
    public class LoadingPrompt : MonoBehaviour
    {
        [SerializeField] private Text message;

        public void UpdateMessage(string msg)
        {
            message.text = LocalizedString.GetString(msg);
        }
    }
}