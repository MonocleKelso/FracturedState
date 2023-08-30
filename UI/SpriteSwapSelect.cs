using UnityEngine;
using UnityEngine.UI;

namespace FracturedState.UI
{
    [RequireComponent(typeof(Dropdown))]
    public class SpriteSwapSelect : MonoBehaviour
    {
        [System.Serializable]
        public class SelectStyleInfo
        {
            public Sprite normal;
            public SpriteState state;
        }

        [SerializeField] private SelectStyleInfo[] styles;

        [SerializeField] private SelectStyleInfo[] optionStyles;

        [SerializeField] private Toggle optionTemplate;

        private Dropdown dropDown;

        private void Start()
        {
            dropDown = GetComponent<Dropdown>();
        }

        public void SwapSprites(int index)
        {
            if (index >= 0 && index < styles.Length && index < optionStyles.Length)
            {
                var style = styles[index];
                var img = dropDown.targetGraphic as UnityEngine.UI.Image;
                if (img != null)
                {
                    img.sprite = style.normal;
                }
                dropDown.spriteState = style.state;
                if (optionTemplate != null)
                {
                    var optStyle = optionStyles[index];
                    var optImg = optionTemplate.targetGraphic as UnityEngine.UI.Image;
                    if (optImg != null)
                    {
                        optImg.sprite = optStyle.normal;
                    }
                    optionTemplate.spriteState = optStyle.state;
                }
            }
        }
    }
}