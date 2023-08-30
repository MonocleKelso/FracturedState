using UnityEngine;

namespace FracturedState.UI
{
    /// <summary>
    /// A marker component that causes the menu it's attached to to be cached by the menu manager.
    /// Use this for menus that need stateful information or need to be updated when they aren't active (such as lobbies)
    /// </summary>
    public class MarkCacheable : MonoBehaviour
    {
        // cache in Start instead of Awake because we change instantiated menu names to match their
        // prefab names (dropping the (Clone) part) and Awake fires before that name change happens
        private void Start()
        {
            MenuContainer.MakeMenuCached(gameObject);
        }
    }
}