using UnityEngine;

namespace FracturedState.UI
{
    /// <summary>
    /// A component that changes which menu the user is currently viewing
    /// </summary>
    public class SwapCurrentMenuButton : MonoBehaviour
    {
        /// <summary>
        /// Swap out the current menu for the given menu
        /// </summary>
        public void Swap(GameObject menu)
        {
            MenuContainer.DisplayMenu(menu);
        }

        /// <summary>
        /// Swap out the current menu for the given menu and remove the top and bottom button bars from the screen
        /// </summary>
        public void SwapHideBars(GameObject menu)
        {
            MenuContainer.HideBars();
            Swap(menu);
        }

        /// <summary>
        /// Swap out the current menu for the given menu and show the top and bottom button bars on the screen
        /// </summary>
        public void SwapShowBars(GameObject menu)
        {
            MenuContainer.Showbars();
            Swap(menu);
        }

        /// <summary>
        /// Flushes the menu cache by deleting all cached menus and then clearing the cache lookup
        /// </summary>
        public void FlushMenuCache()
        {
            MenuContainer.FlushMenuCache();
        }
    }
}