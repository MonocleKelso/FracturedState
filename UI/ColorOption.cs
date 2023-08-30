using UnityEngine;
using UnityEngine.UI;

namespace FracturedState.UI
{
    public class ColorOption : MonoBehaviour
    {
        private void Start()
        {
            GetComponent<Toggle>().interactable = TeamColorSelect.IsColorAvailable(transform.GetSiblingIndex() - 1);
        }
    }
}