using UnityEngine;

namespace FracturedState.UI
{
    public class PlayMainTheme : MonoBehaviour
    {
        public void Play()
        {
            MusicManager.Instance.PlayMainTheme();
        }
    }
}