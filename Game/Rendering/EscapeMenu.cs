using FracturedState.Game.Management;
using FracturedState.Game.Network;
using FracturedState.Game.StatTracker;
using FracturedState.UI;
using UnityEngine;
using Screen = UnityEngine.Screen;

public class EscapeMenu : MonoBehaviour
{
    [SerializeField] private GUIStyle bgStyle;
    
    [SerializeField] private GUIStyle buttonStyle;

    [SerializeField] private GameObject settingsMenu;

    private Vector2 pos;
    private bool settingsOpen;

    public void CloseSettings()
    {
        settingsOpen = false;
        settingsMenu.SetActive(false);
    }
    
    private void OnEnable()
    {
        var x = (Screen.width / 2f) - (bgStyle.fixedWidth / 2f);
        var y = (Screen.height / 2f) - (bgStyle.fixedHeight / 2f);
        pos = new Vector2(x, y);
        CloseSettings();
    }

    private void OnGUI()
    {
        if (settingsOpen) return;
        
        GUI.BeginGroup(new Rect(pos.x, pos.y, bgStyle.fixedWidth, bgStyle.fixedHeight), bgStyle);

        // return
        if (GUI.Button(new Rect(23, 30, buttonStyle.fixedWidth, buttonStyle.fixedHeight), "Return To Game", buttonStyle))
        {
            gameObject.SetActive(false);
        }

        // options
        if (GUI.Button(new Rect(23, 77, buttonStyle.fixedWidth, buttonStyle.fixedHeight), "Options", buttonStyle))
        {
            settingsOpen = true;
            settingsMenu.SetActive(true);
        }

        // menu exit
        if (GUI.Button(new Rect(23, 124, buttonStyle.fixedWidth, buttonStyle.fixedHeight), "Exit To Menu", buttonStyle))
        {
            FracNet.Instance.Disconnect();
            AISimulator.Instance.StopSimulation();
            AISimulator.Instance.ClearTeams();
            SkirmishVictoryManager.PostGameWorldCleanUp();
            CompassUI.Instance.gameObject.SetActive(false);
            MatchStatTracker.Reset();
            FracNet.Instance.Disconnect();
            MenuContainer.FlushMenuCache();
            MenuContainer.SetCurrentMenu(null);
            Instantiate(MenuContainer.Container);
            MenuContainer.Showbars();
            gameObject.SetActive(false);
            MusicManager.Instance.PlayMainTheme();
        }

        // desktop exit
        if (GUI.Button(new Rect(23, 171, buttonStyle.fixedWidth, buttonStyle.fixedHeight), "Exit To Desktop", buttonStyle))
        {
            Application.Quit();
        }

        GUI.EndGroup();
    }
}