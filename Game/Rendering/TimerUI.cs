using System.Collections.Generic;
using FracturedState.Game;
using FracturedState.Game.Management;
using UnityEngine;

public class TimerUI : MonoBehaviour
{
    [SerializeField]
    private int terrStart;
    [SerializeField]
    private int terrMinWidth;
    [SerializeField]
    private int terrHeight;
    [SerializeField]
    private int squadTimerWidth;
    [SerializeField]
    private int squadTimerHeight;
    [SerializeField]
    private int squadTimerTopMargin;
    [SerializeField]
    private int squadTimerIndent;
    [SerializeField]
    private int squadTimerDupeAdjust;

    [SerializeField]
    private GUISkin skin;

    [SerializeField]
    private Rect gameTime;
    public Rect GameTimeRect => gameTime;

    [SerializeField] private CommandManager commandManager;

    private GUIStyle gameTimeStyle;
    private GUIStyle squadTimerStyle;
    private GUIStyle territoryBackground;
    private GUIStyle territoryName;
    private GUIStyle territoryOwner;
    private GUIStyle territoryNoOwner;

    private GUIStyle ownerStyle;

    public Rect TerritoryReadOutRect { get; private set; }

    private Rect currentTerrRect;
    private Rect terrOwnerRect;

    private ReinforcementManager recruitManager;

    private string gameTimeDisplay;
    public float ElapsedGameTime { get; private set; }

    private readonly List<string> procTerr = new List<string>();

    private string visibleTerritory;
    private string visibleOwner;

    public void SetRecruitManager(ReinforcementManager manager)
    {
        recruitManager = manager;
    }

    private void Awake()
    {
        gameTimeStyle = skin.GetStyle("GameTime");
        squadTimerStyle = skin.GetStyle("SquadTimer");
        territoryBackground = skin.GetStyle("Territory");
        territoryName = skin.GetStyle("TerritoryLeft");
        territoryOwner = skin.GetStyle("TerritoryRight");
        territoryNoOwner = skin.GetStyle("TerritoryEmpty");
    }

    private void OnEnable()
    {
        ElapsedGameTime = 0;
        TerritoryReadOutRect = new Rect((Screen.width * 0.5f) - 170, 0, 340, 38);
        currentTerrRect = new Rect(0, 0, TerritoryReadOutRect.width * 0.5f, TerritoryReadOutRect.height);
        terrOwnerRect = new Rect(TerritoryReadOutRect.width * 0.5f, 0, TerritoryReadOutRect.width * 0.5f, TerritoryReadOutRect.height);
        ownerStyle = territoryOwner;
    }

    private void OnDisable()
    {
        SkirmishVictoryManager.GameTimeSnapshot = gameTimeDisplay;
    }

    private void Update()
    {
        // update current game time
        ElapsedGameTime += Time.deltaTime;
        var seconds = Mathf.RoundToInt(ElapsedGameTime);
        gameTimeDisplay = $"{seconds / 3600:00}:{(seconds / 60) % 60:00}:{seconds % 60:00}";
        var ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, GameConstants.TerrainMask))
        {
            var t = TerritoryManager.Instance.GetTerrainAssignment(hit.collider.gameObject);
            if (t != null)
            {
                visibleTerritory = t.Name;
                var owner = TerritoryManager.Instance.GetTerritoryOwner(hit.collider.gameObject);
                if (owner == null)
                {
                    visibleOwner = TerritoryManager.Instance.GetTerritoryTeamCount(hit.collider.gameObject) > 0 ? "Contested" : "Neutral";
                    ownerStyle = territoryNoOwner;
                }
                else
                {
                    visibleOwner = owner.PlayerName;
                    ownerStyle = territoryOwner;
                }
            }
            else
            {
                visibleTerritory = null;
                visibleOwner = null;
            }
        }
        else
        {
            visibleTerritory = null;
            visibleOwner = null;
        }
    }

    private void OnGUI()
    {
        if (!CompassUI.Instance.Draw || commandManager.EscapeMenuOpen)
            return;

        GUI.skin = skin;

        // current territory
        GUI.BeginGroup(TerritoryReadOutRect, GUIContent.none, territoryBackground);
        GUI.Label(currentTerrRect, visibleTerritory, territoryName);
        GUI.Label(terrOwnerRect, visibleOwner, ownerStyle);
        GUI.EndGroup();

        // game clock
        GUI.Label(gameTime, gameTimeDisplay, gameTimeStyle);

        // territory countdown timers
        if (recruitManager.PendingRequests.Count <= 0)
            return;

        SquadRequest hoverReq = null;
        var hover = default(Rect);
        procTerr.Clear();
        var offset = 0;
        for (var i = 0; i < recruitManager.PendingRequests.Count; i++)
        {
            var req = recruitManager.PendingRequests[i];
            if (!procTerr.Contains(req.Territory))
            {
                // draw territory label
                procTerr.Add(req.Territory);
                var width = req.Territory.Length * 10;
                width = (width > terrMinWidth) ? width : terrMinWidth;
                var terr = new Rect(0, terrStart + (terrHeight * procTerr.Count - 1) + (squadTimerHeight * offset), width, terrHeight);
                GUI.Label(terr, req.Territory);
                // draw each timer for that territory
                var reqs = recruitManager.GetRequestsByTerritory(req.Territory);
                for (var r = 0; r < reqs.Length; r++)
                {
                    var t = new Rect(width - squadTimerWidth - squadTimerIndent, (terr.yMin + terrHeight + (squadTimerHeight * r)) - squadTimerTopMargin, squadTimerWidth, squadTimerHeight);
                    t.yMin -= squadTimerDupeAdjust * r;
                    var sec = Mathf.RoundToInt(reqs[r].TimeCost);
                    var time = $"{(sec / 60) % 60:0}:{sec % 60:00}";
                    GUI.Label(t, time, squadTimerStyle);
                    if (t.Contains(Event.current.mousePosition))
                    {
                        hoverReq = reqs[r];
                        hover = t;
                    }
                }
                offset += reqs.Length;
                
            }
        }
        if (hoverReq != null)
        {
            if (Event.current.isMouse && Event.current.type == EventType.MouseDown && Event.current.button == 1)
            {
                InterfaceSoundPlayer.PlaySmallButtonClick();
                recruitManager.RemoveRequest(hoverReq);
            }
            else
            {
                float w = hoverReq.Units.Count == 1 ? 65 : 108;
                float h;
                if (hoverReq.Units.Count % 2 == 0)
                {
                    h = 44 * (hoverReq.Units.Count / 2) + 22;
                }
                else
                {
                    h = 44 * ((hoverReq.Units.Count / 2) + 1) + 22;
                }
                var hov = new Rect(hover.x + hover.width, hover.y, w, h);
                GUILayout.BeginArea(hov, skin.GetStyle("Loadout"));
                for (var u = 0; u < hoverReq.Units.Count; u++)
                {
                    if (u % 2 == 0)
                    {
                        GUILayout.BeginHorizontal();
                    }
                    GUILayout.Label(new GUIContent(hoverReq.Units[u].Icon), skin.GetStyle("UnitIcon"));
                    if (u % 2 == 1)
                    {
                        GUILayout.EndHorizontal();
                    }
                }
                if (hoverReq.Units.Count % 2 == 1)
                {
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndArea();
            }
        }
    }
}