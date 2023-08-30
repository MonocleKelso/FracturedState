using UnityEngine;
using UnityEngine.SceneManagement;

namespace FracturedState.UI
{
    public class LaunchModTools : MonoBehaviour
    {
        public void Launch()
        {
            MusicManager.Instance.FadeOut();
            SceneManager.LoadScene("modTools");
        }
    }
}