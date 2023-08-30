using FracturedState.Game;
using UnityEngine;
using UnityEngine.UI;

namespace FracturedState.UI
{
    public class VersionText : MonoBehaviour
    {
        [SerializeField] private Text text;

        private void Awake()
        {
            text.text = GameConstants.Version;
        }
    }
}