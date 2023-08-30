using System;
using FracturedState.Game;
using FracturedState.Game.Mutators;
using FracturedState.Game.Network;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FracturedState.UI
{
    [Serializable]
    public class CostEvent : UnityEvent<int> { }
    
    public class MutatorButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private string mutator;
        private int cost;
        [SerializeField] private string tooltip;
        [SerializeField] private Text tipDisplay;
        [SerializeField] private CostEvent costDeducted;

        [SerializeField] private Sprite normalBase;
        [SerializeField] private Sprite normalHover;
        [SerializeField] private Sprite selectedBase;
        [SerializeField] private Sprite selectedHover;

        private Image image;
        private Button button;
        
        private bool on;

        private void Awake()
        {
            var tName = $"FracturedState.Game.Mutators.{mutator}";
            var t = Type.GetType(tName);
            if (t == null) throw new FracturedStateException("Bad Mutator instance");
            var i = Activator.CreateInstance(t, null) as IMutator;
            if (i == null) throw new FracturedStateException("Mutator instance is not IMutator");
            cost = i.Cost;

            image = GetComponent<Image>();
            button = GetComponent<Button>();

            if (FracNet.Instance.LocalTeam.HasMutator(tName))
            {
                image.sprite = selectedBase;
                var ss = button.spriteState;
                ss.highlightedSprite = selectedHover;
                button.spriteState = ss;
            }
        }

        public void Add()
        {
            if (!on)
            {
                FracNet.Instance.NetworkActions.CmdAddMutator(mutator);
            }
            else
            {
                FracNet.Instance.NetworkActions.CmdRemoveMutator(mutator);
            }

            on = !on;

            var ss = button.spriteState;
            
            if (on)
            {
                costDeducted.Invoke(cost);
                image.sprite = selectedBase;
                ss.highlightedSprite = selectedHover;
            }
            else
            {
                costDeducted.Invoke(-cost);
                image.sprite = normalBase;
                ss.highlightedSprite = normalHover;
            }
            
            button.spriteState = ss;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            tipDisplay.text = tooltip;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            tipDisplay.text = string.Empty;
        }
    }
}