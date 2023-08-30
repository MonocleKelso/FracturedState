using FracturedState.Game.Management;
using UnityEngine;
using UnityEngine.UI;

namespace FracturedState.UI
{
    [RequireComponent(typeof(InputField))]
    public class SetGameName : MonoBehaviour
    {
        private InputField gameName;

        private void Awake()
        {
            gameName = GetComponent<InputField>();
            gameName.onValueChanged.AddListener(SetName);
        }

        private void SetName(string name)
        {
            SkirmishVictoryManager.GameName = name;
        }

        private void OnDestroy()
        {
            if (gameName != null)
            {
                gameName.onValueChanged.RemoveListener(SetName);
            }
        }
    }
}