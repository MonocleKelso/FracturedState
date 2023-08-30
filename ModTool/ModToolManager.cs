using UnityEngine;
using FracturedState.Game;

public class ModToolManager : MonoBehaviour
{
	public GameObject MapEditorManager;

    [SerializeField] private GUISkin skin;
    public GUISkin Skin => skin;

    private void Awake()
    {
        // load data if mod tools are standalone
        if (Loader.Instance == null)
        {
            // load game.xml and mod.xml if applicable
            FracturedState.Game.Data.ConfigSettings.Instance.LoadDefaultSettings();
            // load all cacheable xml data for the game
            XmlCacheManager.PopulateAllCaches();
        }
        Cursor.lockState = CursorLockMode.None;
        MapEditorManager.SetActive(true);
    }

    private void OnGUI()
    {
        GUI.skin = skin;
        GUI.Box(new Rect(0, 0, Screen.width, 27), GUIContent.none);
        GUILayout.BeginHorizontal(GUILayout.Width(Screen.width));

        GUILayout.Space(15);

		GUILayout.EndHorizontal();

        if (GUI.Button(new Rect(Screen.width - 35, 3, 30, 15), "Exit"))
        {
            Application.Quit();
        }
    }
}