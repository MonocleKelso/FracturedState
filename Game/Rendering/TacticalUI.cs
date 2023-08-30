using FracturedState.Game;
using UnityEngine;

public class TacticalUI : MonoBehaviour
{
    private const float BorderCorner = 206f;
    private const float BorderStretch = 64f;

    [SerializeField]
    private GUIStyle borderStyle;
    [SerializeField]
    private GUIStyle upperStretchStyle;
    [SerializeField]
    private GUIStyle lowerStretchStyle;
    [SerializeField]
    private GUIStyle leftStretchStyle;
    [SerializeField]
    private GUIStyle rightStretchStyle;

    [SerializeField]
    private Texture2D borderUpperLeft;
    [SerializeField]
    private Texture2D borderUpperRight;
    [SerializeField]
    private Texture2D borderLowerLeft;
    [SerializeField]
    private Texture2D borderLowerRight;

    private Rect r_borderUpperLeft;
    private Rect r_borderUpperRight;
    private Rect r_borderLowerLeft;
    private Rect r_borderLowerRight;
    private Rect r_borderUpperStretch;
    private Rect r_borderLowerStretch;
    private Rect r_borderLeftStretch;
    private Rect r_borderRightStretch;

    [SerializeField] private GUIStyle helperStyle;
    
    private KeyCode? toggleKey;

    private void OnEnable()
    {
        r_borderUpperLeft = new Rect(0, 0, BorderCorner, BorderCorner);
        r_borderUpperRight = new Rect(Screen.width - BorderCorner, 0, BorderCorner, BorderCorner);
        r_borderLowerLeft = new Rect(0, Screen.height - BorderCorner, BorderCorner, BorderCorner);
        r_borderLowerRight = new Rect(Screen.width - BorderCorner, Screen.height - BorderCorner, BorderCorner, BorderCorner);

        r_borderUpperStretch = new Rect(BorderCorner, 0, Screen.width - BorderCorner * 2, BorderStretch);
        r_borderLowerStretch = new Rect(BorderCorner, Screen.height - BorderStretch, Screen.width - BorderCorner * 2, BorderStretch);
        r_borderLeftStretch = new Rect(0, BorderCorner, BorderStretch, Screen.height - BorderCorner * 2);
        r_borderRightStretch = new Rect(Screen.width - BorderStretch, BorderCorner, BorderStretch, Screen.height - BorderCorner * 2);

        toggleKey = ProfileManager.GetActiveProfile()?.KeyBindConfig?.ToggleTacticalMode?.Key;
    }

    private void OnGUI()
    {
        GUI.Label(r_borderUpperLeft, borderUpperLeft, borderStyle);
        GUI.Label(r_borderUpperRight, borderUpperRight, borderStyle);
        GUI.Label(r_borderLowerLeft, borderLowerLeft, borderStyle);
        GUI.Label(r_borderLowerRight, borderLowerRight, borderStyle);

        if (toggleKey != null)
        {
            GUI.Label(new Rect(25, 25, 200, 200), $"Press {toggleKey.Value.ToString()} to leave tactical view", helperStyle);
        }
        
        GUI.Label(r_borderUpperStretch, GUIContent.none, upperStretchStyle);
        GUI.Label(r_borderLowerStretch, GUIContent.none, lowerStretchStyle);
        GUI.Label(r_borderLeftStretch, GUIContent.none, leftStretchStyle);
        GUI.Label(r_borderRightStretch, GUIContent.none, rightStretchStyle);
    }
}