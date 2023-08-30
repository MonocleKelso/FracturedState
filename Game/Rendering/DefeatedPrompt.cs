using UnityEngine;

public class DefeatedPrompt : MonoBehaviour
{
    [SerializeField]
    private GUIStyle defeatStyle;
    [SerializeField]
    private GUIStyle defeatShadowStyle;
    [SerializeField]
    private GUIStyle titleStyle;
    [SerializeField]
    private GUIStyle titleStyleShadow;

    private Rect defeatRect;
    private Rect defeatShadow;
    private Rect startRect;
    private Rect startRectShadow;

    void OnGUI()
    {
        GUI.Label(startRectShadow, "YOU HAVE BEEN", titleStyleShadow);
        GUI.Label(startRect, "YOU HAVE BEEN", titleStyle);
        GUI.Label(defeatShadow, "DEFEATED", defeatShadowStyle);
        GUI.Label(defeatRect, "DEFEATED", defeatStyle);
    }

    void OnEnable()
    {
        defeatRect = new Rect(0, Screen.height * 0.5f - 250, Screen.width, 500);
        defeatShadow = new Rect(defeatRect);
        defeatShadow.yMin += 5;
        defeatShadow.xMin += 5;
        startRect = new Rect(0, Screen.height * 0.5f - 100, Screen.width, 50);
        startRectShadow = new Rect(startRect);
        startRectShadow.yMin += 3;
        startRectShadow.xMin += 3;
        StartCoroutine(Timer());
    }

    private System.Collections.IEnumerator Timer()
    {
        yield return new WaitForSeconds(4);
        gameObject.SetActive(false);
    }
}