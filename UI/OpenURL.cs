using UnityEngine;

namespace FracturedState.UI
{
    /// <summary>
    /// A UI class that is used in conjunction with a button to open a specific URL in the player's browser
    /// </summary>
    public class OpenURL : MonoBehaviour
    {
        [SerializeField] private string url;

        public void OpenPage()
        {
            Application.OpenURL(url);
        }

        public void SetURL(string url)
        {
            this.url = url;
        }
    }
}