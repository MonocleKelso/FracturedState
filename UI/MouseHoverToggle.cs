using UnityEngine;
using UnityEngine.EventSystems;

namespace FracturedState.UI
{
    public class MouseHoverToggle : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private GameObject toggle;
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            toggle.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            toggle.SetActive(false);
        }
    }
}