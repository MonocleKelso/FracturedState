using UnityEngine;

namespace FracturedState.UI
{
    public static class GUIExtension
    {
        public static int Dropdown(string[] elements, int selectedIndex, ref bool open, Rect btnRect, Rect scrollRect, ref Vector2 scrollPos, GUIStyle btnStyle, GUIStyle scrollStyle)
        {
            if (!open)
            {
                if (GUI.Button(btnRect, elements[selectedIndex], btnStyle))
                {
                    open = true;
                }
            }
            else
            {
                float height = (17 * elements.Length > scrollRect.height) ? 17 * elements.Length : scrollRect.height;
                scrollPos = GUI.BeginScrollView(scrollRect, scrollPos, new Rect(0, 0, scrollRect.width - 25, height), false, true);
                for (int i = 0; i < elements.Length; i++)
                {
                    if (GUI.Button(new Rect(0, 17 * i, 151, 17), elements[i], btnStyle))
                    {
                        selectedIndex = i;
                        open = false;
                    }
                }
                GUI.EndScrollView();
            }
            return selectedIndex;
        }
    }
}