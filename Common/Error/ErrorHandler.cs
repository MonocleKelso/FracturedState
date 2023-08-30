using FracturedState.Game;
using FracturedState.UI;
using UnityEngine;

public class ErrorHandler : MonoBehaviour
{
    public static bool HasError { get; private set; }

    string logString;
    string stackTrace;
    bool detailOpen;
    Vector2 scrollPos;

    public static void HandleSyncError(SyncException ex)
    {
        ExtensionMethods.HandleError(ex.Message, ex.InnerException.Message + "\n" + ex.InnerException.StackTrace, LogType.Exception);
    }

    public void SetError(string logString, string stackTrace)
    {
        HasError = true;
        this.logString = logString;
        this.stackTrace = stackTrace;
    }

    void OnGUI()
    {
        var w = UnityEngine.Screen.width;
        var h = UnityEngine.Screen.height;
        if (GUI.Button(new Rect(w - 105, h - 35, 100, 30), "! Error !"))
        {
            detailOpen = !detailOpen;
        }

        if (detailOpen)
        {
            var r = new Rect(w / 2 - 250, h / 2 - 250, 500, 500);
            GUILayout.BeginArea(r);
            GUILayout.BeginVertical();
            GUILayout.Label(logString);
            scrollPos = GUILayout.BeginScrollView(scrollPos);
            GUILayout.Label(stackTrace);
            GUILayout.EndScrollView();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Copy error to Clipboard", GUILayout.MinHeight(30)))
            {
                GUIUtility.systemCopyBuffer = logString + "\n" + stackTrace;
            }
            GUILayout.Space(15);
            if (GUILayout.Button("Dismiss", GUILayout.MinHeight(30)))
            {
                Destroy(gameObject);
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }

    void OnDestroy()
    {
        HasError = false;
    }
}