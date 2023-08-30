using FracturedState.Game.Network;
using UnityEngine;
using UnityEngine.UI;

namespace FracturedState.UI.Events
{
    public class LocalEliminationWarning : MonoBehaviour
    {
        [SerializeField] private Text countdown;

        private static LocalEliminationWarning instance;
        private float time;

        public static void Begin()
        {
            instance.time = 60;
            instance.gameObject.SetActive(true);
        }

        public static void Stop()
        {
            instance.gameObject.SetActive(false);
        }
        
        private void Awake()
        {
            instance = this;
            gameObject.SetActive(false);
        }
        
        private void Update()
        {
            time -= Time.deltaTime;
            if (time <= 0)
            {
                Loader.Instance.ShowDefeatIndicator();
                gameObject.SetActive(false);
                return;
            }
            countdown.text = time.ToString("00.0");
        }

        public void Surrender()
        {
            FracNet.Instance.Surrender();
            Loader.Instance.ShowDefeatIndicator();
            MusicManager.Instance.PlayWarningHit();
            gameObject.SetActive(false);
        }
    }
}