using UnityEngine;

namespace FracturedState.UI
{
    /// <summary>
    /// A UI component that will load the provided menu prefab as soon as it wakes up. This is an additive load so any other menus that are in the scene
    /// are not destroyed like when you use SwapCurrentMenuButton
    /// </summary>
    public class LoadMenuOnStart : MonoBehaviour
    {
        [SerializeField] private GameObject menu;

        private void Start()
        {
            var newMenu = Instantiate(menu, MenuContainer.Container);
            newMenu.transform.SetAsFirstSibling();
            MenuContainer.SetCurrentMenu(newMenu);
        }
    }
}