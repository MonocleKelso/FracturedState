using System.Collections;
using System.Collections.Generic;
using FracturedState.Game.Network;
using UnityEngine;
using UnityEngine.UI;

namespace FracturedState.UI
{
    public class Chat : MonoBehaviour
    {
        private const int MaxMessageCount = 50;

        private static List<string> messages = new List<string>();
        public static void AddMessage(string message)
        {
            messages.Add(message);
            if (messages.Count > MaxMessageCount)
            {
                messages.RemoveAt(0);
            }
            if (activeInstance != null)
            {
                activeInstance.DisplayMessage(message, true);
            }
        }

        public static void ClearMessages()
        {
            messages.Clear();
            if (activeInstance != null)
            {
                for (int i = 0; i < activeInstance.chatParent.childCount; i++)
                {
                    Destroy(activeInstance.chatParent.GetChild(i).gameObject);
                }
            }
        }

        private static Chat activeInstance;
        private static float scrollPos;

        [SerializeField] private InputField chatInput;
        [SerializeField] private Transform chatParent;
        [SerializeField] private Text chatEntry;
        [SerializeField] private Scrollbar scrollBar;

        private void OnEnable()
        {
            activeInstance = this;
            // rebuild chat messages for the currently active instance
            if (messages.Count > 0)
            {
                for (int i = 0; i < chatParent.childCount; i++)
                {
                    Destroy(chatParent.GetChild(i).gameObject);
                }
                for (int i = 0; i < messages.Count; i++)
                {
                    DisplayMessage(messages[i], false);
                }
            }
            scrollBar.onValueChanged.RemoveAllListeners();
            scrollBar.onValueChanged.AddListener(val => { scrollPos = scrollBar.value; });
            StartCoroutine(Scroll(scrollPos));
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Return) && !string.IsNullOrEmpty(chatInput.text))
            {
                FracNet.Instance.NetworkActions.CmdAddChatMessage(chatInput.text);
                chatInput.text = "";
                chatInput.ActivateInputField();
            }
        }

        private void DisplayMessage(string message, bool scrollToBottom)
        {
            var entry = Instantiate(chatEntry, chatParent);
            entry.text = message;
            if (chatParent.childCount > MaxMessageCount)
            {
                Destroy(chatParent.GetChild(0).gameObject);
            }
            if (scrollToBottom && enabled && gameObject.activeInHierarchy)
                StartCoroutine(Scroll(0));
        }

        // this is awful but we need to wait 2 frames for the UI to update itself before we reset scroll
        // position otherwise scrolling to the bottom doesn't actually scroll to the bottom
        private IEnumerator Scroll(float pos)
        {
            yield return null;
            yield return null;
            scrollBar.value = pos;
        }
    }
}