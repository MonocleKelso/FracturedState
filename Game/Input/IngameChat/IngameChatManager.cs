using System.Collections;
using System.Collections.Generic;
using FracturedState.Game.Management;
using FracturedState.Game.Network;
using FracturedState.UI;
using UnityEngine;
using UnityEngine.UI;

namespace FracturedState.Game
{
    public class IngameChatManager : MonoBehaviour
    {
        public static IngameChatManager Instance { get; private set; }
        public bool ChatInputOpen { get; private set; }

        [SerializeField] private Transform entryContainer;
        [SerializeField] private Text chatEntryPrefab;
        [SerializeField] private InputField inputField;

        private bool inputFocused;
        
        private readonly List<Text> entries = new List<Text>();

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
        }

        private void Start()
        {
            SkirmishVictoryManager.OnMatchComplete += (_) => Clear();
            inputField.onEndEdit.AddListener(_ => inputFocused = Input.GetKey(KeyCode.Return));
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Return))
            {
                if (ChatInputOpen)
                {
                    if (inputFocused)
                    {
                        if (!string.IsNullOrWhiteSpace(inputField.text))
                        {
                            FracNet.Instance.NetworkActions.CmdAddAllyChatEvent(inputField.text);
                        }
                        inputField.text = "";
                        inputField.gameObject.SetActive(false);
                        inputFocused = false;
                        ChatInputOpen = false;
                    }
                    else
                    {
                        inputField.Select();
                        inputField.ActivateInputField();
                    }
                }
                else
                {
                    inputField.gameObject.SetActive(true);
                    inputField.Select();
                    inputField.ActivateInputField();
                    inputFocused = true;
                    ChatInputOpen = true;
                }
            }

            if (ChatInputOpen && Input.GetKeyUp(KeyCode.Escape))
            {
                inputField.text = "";
                inputField.gameObject.SetActive(false);
                inputFocused = false;
                ChatInputOpen = false;
            }
        }
        
        public void AddEntry(string message)
        {
            var t = Instantiate(chatEntryPrefab, entryContainer);
            entries.Add(t);
            StartCoroutine(Do(message, t));
        }

        public void RemoveEntry(Text t)
        {
            entries.Remove(t);
        }

        private IEnumerator Do(string message, Text t)
        {
            yield return null;
            t.text = message;
        }
        
        private void Clear()
        {
            foreach (var entry in entries)
            {
                if (entry == null) continue;
                
                entry.StopAllCoroutines();
                Destroy(entry.gameObject);
            }
            
            entries.Clear();
        }
    }
}