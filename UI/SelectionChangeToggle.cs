using UnityEngine;

namespace FracturedState.UI
{
    public class SelectionChangeToggle : MonoBehaviour
    {
        private void Start()
        {
            SelectionManager.Instance.OnSelectionChanged.AddListener(() =>
            {
                gameObject.SetActive(SelectionManager.Instance.SelectedUnits.Count > 0);
            });
            
            gameObject.SetActive(false);
        }
    }
}