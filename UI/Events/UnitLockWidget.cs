using FracturedState.Game.Data;
using UnityEngine;
using UnityEngine.UI;

namespace FracturedState.UI.Events
{
    public class UnitLockWidget : MonoBehaviour
    {
        [SerializeField] private Text unitName;
        [SerializeField] private Image unitIcon;

        public void SetUnitData(UnitObject unit)
        {
            unitName.text = unit.Name;
            unitIcon.sprite = Sprite.Create(unit.Icon, new Rect(0, 0, 75, 75), new Vector2(0.5f, 0.5f), 100);
        }

        private void OnDestroy()
        {
            if (unitIcon.sprite == null) return;
            Destroy(unitIcon.sprite);
        }
    }
}