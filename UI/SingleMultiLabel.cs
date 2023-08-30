using FracturedState.Game.Management;
using UnityEngine;
using UnityEngine.UI;

namespace FracturedState.UI
{
    public class SingleMultiLabel : MonoBehaviour
    {
        [SerializeField] private Text label;
        [SerializeField] private string singleLabel;
        [SerializeField] private string multiLabel;

        private void Awake()
        {
            label.text = LocalizedString.GetString(SkirmishVictoryManager.IsMultiPlayer ? (multiLabel) : (singleLabel));
        }
    }
}