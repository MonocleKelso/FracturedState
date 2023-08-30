using FracturedState.Game.Data;
using UnityEngine;
using UnityEngine.UI;

namespace FracturedState.UI
{
    public class UnitTemplate : MonoBehaviour
    {
        [SerializeField] private Text unitName;

        [SerializeField] private Text flavorText;

        [SerializeField] private UnityEngine.UI.Image icon;

        public void SetUnit(UnitObject unit)
        {
            unitName.text = unit.Name;
            flavorText.text = unit.ShortDescription;
            icon.sprite = Sprite.Create(unit.Icon, new Rect(0, 0, unit.Icon.width, unit.Icon.height), Vector2.zero);
        }
    }
}