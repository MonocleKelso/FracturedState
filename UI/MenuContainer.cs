using System.Collections.Generic;
using UnityEngine;

namespace FracturedState.UI
{
    /// <summary>
    /// An organization UI component meant to be placed on a top level menu GameObject so that it can manage other menu screens
    /// </summary>
    public class MenuContainer : MonoBehaviour
    {
        private static MenuContainer instance;
        public static Transform Container { get; private set; }
        private static GameObject currentMenu;

        [SerializeField] private GameObject topBarContainer;

        [SerializeField] private GameObject bottomBarContainer;

        [SerializeField] private GameObject loadingScreen;

        [SerializeField] private GameObject endGame;

        [SerializeField] private LoadingPrompt loadingPrompt;
        private LoadingPrompt promptInstance;

        private static readonly Dictionary<string, GameObject> CachedMenus = new Dictionary<string, GameObject>();

        private void Awake()
        {
            Container = transform;
            if (instance != null)
            {
                Destroy(instance.gameObject);
            }
            instance = this;
            foreach (Transform t in transform)
            {
                if (t.gameObject.name != topBarContainer.name &&
                    t.gameObject.name != bottomBarContainer.name)
                {
                    Destroy(t.gameObject);
                }
            }
        }

        public static void ShowEndGame()
        {
            DisplayMenu(instance.endGame);
        }
        
        public static LoadingPrompt ShowPrompt()
        {
            return instance.promptInstance ? instance.promptInstance : (instance.promptInstance = Instantiate(instance.loadingPrompt, instance.transform));
        }

        public static void HidePrompt()
        {
            if (instance.promptInstance != null)
            {
                Destroy(instance.promptInstance.gameObject);
                instance.promptInstance = null;
            }
        }

        public static void MakeMenuCached(GameObject menu)
        {
            if (CachedMenus.ContainsKey(menu.name))
            {
                Destroy(CachedMenus[menu.name]);
            }
            CachedMenus[menu.name] = menu;
        }

        public static void FlushMenuCache()
        {
            foreach (var key in CachedMenus.Keys)
            {
                Destroy(CachedMenus[key]);
            }
            CachedMenus.Clear();
        }

        public static void ShowLoadingScreen()
        {
            FlushMenuCache();
            if (currentMenu != null)
            {
                Destroy(currentMenu);
                currentMenu = null;
            }
            DisplayMenu(instance.loadingScreen);
        }

        public static void HideBars()
        {
            instance.ToggleBars(false);
        }

        public static void Showbars()
        {
            instance.ToggleBars(true);
        }

        public static void DisplayMenu(GameObject menu)
        {
            GameObject newMenu;
            if (CachedMenus.TryGetValue(menu.name, out newMenu))
            {
                newMenu.SetActive(true);
            }
            else
            {
                newMenu = Instantiate(menu, Container);
                newMenu.name = menu.name;
            }
            newMenu.transform.SetAsFirstSibling();
            SetCurrentMenu(newMenu);
        }

        public static void SetCurrentMenu(GameObject menu)
        {
            if (currentMenu != null)
            {
                if (currentMenu.GetComponent<MarkCacheable>() != null)
                {
                    currentMenu.SetActive(false);
                }
                else
                {
                    Destroy(currentMenu);
                }
            }
            currentMenu = menu;
        }

        public static void HideMenus()
        {
            if (currentMenu != null)
            {
                Destroy(currentMenu);
            }
            FlushMenuCache();
        }

        private void ToggleBars(bool enableBars)
        {
            if (topBarContainer != null)
                topBarContainer.SetActive(enableBars);
            if (bottomBarContainer != null)
                bottomBarContainer.SetActive(enableBars);
        }
    }
}