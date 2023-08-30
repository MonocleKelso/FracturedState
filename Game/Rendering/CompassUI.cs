using UnityEngine;
using FracturedState.Game;
using FracturedState.Game.Data;
using FracturedState.Game.Management;
using FracturedState.Game.Network;
using System.Collections;
using System.Collections.Generic;
using Code.Game.Rendering;

public class CompassUI : MonoBehaviour
{
    [SerializeField]
    private GUISkin skin;

    [SerializeField]
    private Texture2D nullIcon;

    [SerializeField]
    private Rect popCounter;
    [SerializeField]
    private Rect recruitBtn;

    [SerializeField]
    private Texture2D tacticalIcon;
    [SerializeField]
    private Rect tacticalBtn;

    [SerializeField]
    private Texture2D techIcon;
    [SerializeField]
    private Rect techBtn;

    [SerializeField]
    private Texture2D chatIcon;
    [SerializeField]
    private Rect chatBtn;
    
    [SerializeField]
    private Texture2D settingsIcon;
    [SerializeField]
    private Rect settingsBtn;

    [SerializeField]
    private Rect resetBtn;
    [SerializeField]
    private Rect exitBtn;
    [SerializeField]
    private Rect squadBg;
    [SerializeField]
    private Rect timer;
    [SerializeField] private Rect speed;
    [SerializeField]
    private Rect orderBtn;

    public Rect RecruitArea { get; private set; }
    public bool RecruitPanel { get; private set; }
    private bool toggling;

    public static CompassUI Instance { get; private set; }
    public bool Draw { get; set; }

    // private Rect toolTipRect;
    [SerializeField]
    private Rect availUnitsArea;
    [SerializeField]
    private Rect terrDropdownClosed;
    [SerializeField]
    private Rect terrDropdownOpen;
    private Rect terrViewRect;

    private GUIStyle smallBtnStyle;
    private GUIStyle bigBtnStyle;
    private GUIStyle recruitPanelStyle;
    private GUIStyle resetBtnStyle;
    private GUIStyle exitBtnStyle;
    private GUIStyle squadReqStyle;
    private GUIStyle timerStyle;
    private GUIStyle orderBtnStyle;
    private GUIStyle unitButton;
    private GUIStyle dropdownClosed;
    private GUIStyle dropdownPanel;
    private GUIStyle dropdownItem;

    private ReinforcementManager recruitManager;
    private List<UnitObject> availableUnits;

    private string timeDisplay;
    private int territories;
    private int selectedTerritory;
    private bool isDropdownOpen;

    private Team team;
    private Faction teamFaction;

    [SerializeField]
    private TimerUI timers;
    public TimerUI Timers => timers;

    [SerializeField] private UnitRecruitTooltip toolTip;

    private UnitObject tooltipUnit;

    public void Awake()
    {
        smallBtnStyle = skin.GetStyle("SmallButton");
        bigBtnStyle = skin.GetStyle("BigButton");
        recruitPanelStyle = skin.GetStyle("RecruitPanel");
        resetBtnStyle = skin.GetStyle("ResetButton");
        exitBtnStyle = skin.GetStyle("ExitButton");
        squadReqStyle = skin.GetStyle("SquadBG");
        timerStyle = skin.GetStyle("Timer");
        orderBtnStyle = skin.GetStyle("OrderButton");
        unitButton = skin.GetStyle("UnitButton");
        dropdownClosed = skin.GetStyle("DropdownClosed");
        dropdownPanel = skin.GetStyle("DropdownPanel");
        dropdownItem = skin.GetStyle("DropdownItem");
        Instance = this;
        // make sure notification manager is properly initialized before we turn stuff off
        FracturedState.UI.ScreenEdgeNotificationManager.Init();
        // turn off immediately so we don't render but ensure that Instance is set up correctly
        gameObject.SetActive(false);
    }

    private void Start()
    {
        // right-align buttons based on current resolution
        recruitBtn = AdjustRect(recruitBtn);
        tacticalBtn = AdjustRect(tacticalBtn);
        techBtn = AdjustRect(techBtn);
        chatBtn = AdjustRect(chatBtn);
        settingsBtn = AdjustRect(settingsBtn);
        popCounter = AdjustRect(popCounter);
        terrDropdownOpen = AdjustRect(terrDropdownOpen);

        isDropdownOpen = false;

        RecruitPanel = false;
    }

    public void Init()
    {
        team = FracNet.Instance.NetworkActions.LocalTeam;
        team.RecruitManager = new ReinforcementManager();
        recruitManager = team.RecruitManager;
        timers.SetRecruitManager(recruitManager);
        territories = 0;
        team.SquadPopulation = 0;
        team.TeamPopulation = 0;
        availableUnits = new List<UnitObject>();
        if (team.IsSpectator) return;
        
        teamFaction = XmlCacheManager.Factions[team.Faction];
        var units = teamFaction.TrainableUnits;
        for (var i = 0; i < units.Length; i++)
        {
            if (units[i] != teamFaction.SquadUnit)
            {
                availableUnits.Add(XmlCacheManager.Units[units[i]]);
            }
        }
    }

    private void OnEnable()
    {
        Draw = true;
        if (team == null)
        {
            Init();
        }
    }

    public void SetRecruitPanel(Rect compassArea)
    {
        RecruitArea = new Rect(Screen.width - 316, compassArea.height, 316, 366);
    }

    public void IncreaseTerritoryCounter()
    {
        RefreshTeamPopulation();
        InterfaceSoundPlayer.PlayTerritoryGain();
        FracNet.StopLocalWarning();
    }

    public void DecreaseTerritoryCounter()
    {
        RefreshTeamPopulation();
        if (territories == 0)
        {
            RecruitPanel = false;
        }

        // only play loss if we still have some territories left otherwise elimination stuff takes precedence
        if (TerritoryManager.Instance.GetOwnedTerritoryCount(team) > 0)
        {
            InterfaceSoundPlayer.PlayTerritoryLoss();
        }
    }

    public void AddSquad()
    {
        team.SquadPopulation++;
        if (team.SquadPopulation >= team.TeamPopulation)
        {
            RecruitPanel = false;
        }
    }

    public void RemoveSquad()
    {
        team.SquadPopulation--;
    }

    private void RefreshTeamPopulation()
    {
        var terrs = TerritoryManager.Instance.GetOwnedTerritories(team);
        team.TeamPopulation = 0;
        territories = 0;
        if (terrs == null) return;
        
        for (var i = 0; i < terrs.Count; i++)
        {
            team.TeamPopulation += terrs[i].PopulationBonus;
            if (terrs[i].Recruit)
            {
                territories++;
            }
        }
    }

    // adjust rectangles to be right-aligned based on screen resolution
    private Rect AdjustRect(Rect r)
    {
        r.x = Screen.width - r.x - r.width;
        return r;
    }

    private void Update()
    {
        if (team.SquadPopulation < team.TeamPopulation)
        {
            recruitManager.UpdateRequestTimes(Time.deltaTime);
        }
    }

    private IEnumerator SlidePanelIn()
    {
        toggling = true;
        RecruitPanel = true;
        var x = RecruitArea.xMin;
        RecruitArea = new Rect(Screen.width, RecruitArea.yMin, RecruitArea.width, RecruitArea.height);
        while (RecruitArea.xMin - (950 * Time.deltaTime) > x)
        {
            RecruitArea = new Rect(RecruitArea.xMin - (950 * Time.deltaTime), RecruitArea.yMin, RecruitArea.width, RecruitArea.height);
            yield return null;
        }
        RecruitArea = new Rect(x, RecruitArea.yMin, RecruitArea.width, RecruitArea.height);
        toggling = false;
    }

    private IEnumerator SlidePanelOut()
    {
        toggling = true;
        var x = RecruitArea.xMin;
        while (RecruitArea.xMin + (950 * Time.deltaTime) < Screen.width)
        {
            RecruitArea = new Rect(RecruitArea.xMin + (950 * Time.deltaTime), RecruitArea.yMin, RecruitArea.width, RecruitArea.height);
            yield return null;
        }
        RecruitPanel = false;
        RecruitArea = new Rect(x, RecruitArea.yMin, RecruitArea.width, RecruitArea.height);
        toggling = false;
    }

    public void ToggleRecruitPanel()
    {
        if (toggling)
            return;

        if (RecruitPanel)
        {
            InterfaceSoundPlayer.PlaySmallButtonClick();
            StartCoroutine(SlidePanelOut());
        }
        else
        {
            InterfaceSoundPlayer.PlaySmallButtonClick();
            recruitManager.CreateRequest();
            selectedTerritory = 0;
            StartCoroutine(SlidePanelIn());
        }
    }

    public void SetTerritory(TerritoryData territory)
    {
        var terrs = TerritoryManager.Instance.GetOwnedTerritories(team);
        for (int i = 0; i < terrs.Count; i++)
        {
            if (terrs[i] == territory)
            {
                selectedTerritory = i;
                break;
            }
        }
    }
    
    private void OnGUI()
    {
        tooltipUnit = null;
        if (!Draw)
            return;

        GUI.skin = skin;

        // population
        GUI.Label(popCounter, team.SquadPopulation + "/" + team.TeamPopulation);

        GUI.enabled = !team.IsSpectator && team.IsActive;
        // recruit
        if (GUI.Button(recruitBtn, "RECRUIT", bigBtnStyle))
        {
            ToggleRecruitPanel();
        }

        // helper swap
        if (GUI.Button(tacticalBtn, tacticalIcon, smallBtnStyle))
        {
            InterfaceSoundPlayer.PlaySmallButtonClick();
            FindObjectOfType<CommandManager>().ToggleHelpers();
        }

        // tech tree preview
        if (GUI.Button(techBtn, techIcon, smallBtnStyle))
        {
            InterfaceSoundPlayer.PlaySmallButtonClick();
        }

        // chat
        if (GUI.Button(chatBtn, chatIcon, smallBtnStyle))
        {
            InterfaceSoundPlayer.PlaySmallButtonClick();
        }

        // ingame menu
        if (GUI.Button(settingsBtn, settingsIcon, smallBtnStyle))
        {
            InterfaceSoundPlayer.PlaySmallButtonClick();
            FindObjectOfType<CommandManager>().ToggleEscapeMenu();
        }
        
        // recruit panel
        if (RecruitPanel)
        {
            var availTerrs = TerritoryManager.Instance.GetOwnedTerritories(team);
            var terrNames = new List<string>();
            if (availTerrs != null && availTerrs.Count > 0)
            {
                for (var i = 0; i < availTerrs.Count; i++)
                {
                    if (availTerrs[i].Recruit)
                    {
                        terrNames.Add(availTerrs[i].Name);
                    }
                }
                if (selectedTerritory >= terrNames.Count)
                {
                    selectedTerritory = terrNames.Count - 1;
                }
            }
            else
            {
                isDropdownOpen = false;
            }
            var haveTerrs = terrNames.Count > 0;
            if (haveTerrs && selectedTerritory == -1)
            {
                selectedTerritory = 0;
            }

            GUI.BeginGroup(RecruitArea, recruitPanelStyle);
            // exit button
            if (GUI.Button(exitBtn, GUIContent.none, exitBtnStyle))
            {
                ToggleRecruitPanel();
            }
            // reset button
            GUI.enabled = haveTerrs;
            if (GUI.Button(resetBtn, "RESET", resetBtnStyle))
            {
                InterfaceSoundPlayer.PlaySmallButtonClick();
                recruitManager.CreateRequest();
                recruitManager.SetTerritory(terrNames[0]);
                selectedTerritory = 0;
            }
            // current request loadout
            GUI.enabled = true;
            GUILayout.BeginArea(squadBg, GUIContent.none, squadReqStyle);
            for (int i = 0; i < ConfigSettings.Instance.Values.SquadPopulationCap; i++)
            {
                if (i % 2 == 0)
                {
                    GUILayout.BeginHorizontal();
                }
                var unit = recruitManager.GetUnitAtIndex(i);
                if (unit != null)
                {
                    var c = new GUIContent(unit.Icon);
                    var unitBtn = GUILayoutUtility.GetRect(c, unitButton);
                    if (unitBtn.Contains(Event.current.mousePosition))
                    {
                        tooltipUnit = unit;
                    }
                    if (GUI.Button(unitBtn, c, unitButton))
                    {
                        InterfaceSoundPlayer.PlaySmallButtonClick();
                        recruitManager.RemoveUnit(unit);
                    }
                }
                else
                {
                    GUILayout.Button(nullIcon, unitButton);
                }
                if (i % 2 == 1)
                {
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndArea();
            // unit ordering buttons
            GUI.BeginGroup(availUnitsArea);
            int rowCount = 0, colCount = 0;
            for (var i = 0; i < availableUnits.Count; i++)
            {
                if (i > 0 && i % 4 == 0)
                {
                    colCount++;
                    rowCount = 0;
                }

                var unit = availableUnits[i];
                var unitEnabled = (recruitManager.RequestPopCost + unit.PopulationCost <= ConfigSettings.Instance.Values.SquadPopulationCap);
                if (unitEnabled && unit.PrerequisiteStructures != null)
                {
                    foreach (var structure in unit.PrerequisiteStructures)
                    {
                        if (!StructureManager.HasStructure(structure))
                        {
                            unitEnabled = false;
                            break;
                        }
                    }
                }
                GUI.enabled = unitEnabled;
                
                var top = unitButton.fixedHeight * rowCount + unitButton.margin.top * rowCount;
                var right = unitButton.fixedWidth * colCount + unitButton.margin.left * colCount;
                var rect = new Rect(right, top, unitButton.fixedWidth, unitButton.fixedHeight);
                var content = new GUIContent(unit.Icon, unit.Name);
                
                if (rect.Contains(Event.current.mousePosition))
                {
                    tooltipUnit = unit;
                }
                
                if (GUI.Button(rect, content, unitButton))
                {
                    InterfaceSoundPlayer.PlaySmallButtonClick();
                    recruitManager.AddUnit(unit);
                }
                
                rowCount++;
            }
            GUI.EndGroup();
            
            // timer
            var seconds = Mathf.RoundToInt(recruitManager.RequestRecruitCost);
            var theTime = $"{(seconds / 60) % 60:0}:{seconds % 60:00}";
            GUI.enabled = true;
            GUI.Label(timer, $"Time: {theTime}", timerStyle);
            
            // loadout speed
            var squadSpeed = 0f;
            for (var i = 0; i < recruitManager.CurrentRequest.Units.Count; i++)
            {
                squadSpeed += recruitManager.CurrentRequest.Units[i].Physics.MaxSpeed;
            }
            squadSpeed /= recruitManager.CurrentRequest.Units.Count;
            GUI.Label(speed, $"Speed: {squadSpeed:#0.0}", timerStyle);
            
            // order squad button
            GUI.enabled = haveTerrs && team.SquadPopulation + recruitManager.PendingRequests.Count < team.TeamPopulation && !isDropdownOpen;
            if (GUI.Button(orderBtn, "RECRUIT", orderBtnStyle))
            {
                recruitManager.SetTerritory(terrNames[selectedTerritory]);
                InterfaceSoundPlayer.PlaySmallButtonClick();
                recruitManager.QueueRequest();
            }
            GUI.enabled = GUI.enabled = haveTerrs && team.SquadPopulation + recruitManager.PendingRequests.Count < team.TeamPopulation;
            // closed territory selection
            if (!isDropdownOpen)
            {
                if (haveTerrs)
                {
                    if (GUI.Button(terrDropdownClosed, terrNames[selectedTerritory], dropdownClosed))
                    {
                        isDropdownOpen = true;
                    }
                }
                else
                {
                    GUI.Button(terrDropdownClosed, "", dropdownClosed);
                }
            }
            GUI.EndGroup();

            // open territory selection
            // outside of recruit panel group so that the bottom doesn't clip off - has to be drawn separately
            if (haveTerrs && isDropdownOpen)
            {
                // process clicks outside dropdown in order to close it
                if (Event.current != null && (Event.current.type == EventType.MouseDown || Event.current.type == EventType.Used) && !terrDropdownOpen.Contains(Event.current.mousePosition))
                {
                    isDropdownOpen = false;
                }

                GUILayout.BeginArea(terrDropdownOpen, GUIContent.none, dropdownPanel);
                GUILayout.BeginVertical();
                for (var i = 0; i < terrNames.Count; i++)
                {
                    if (GUILayout.Button(terrNames[i], dropdownItem))
                    {
                        recruitManager.SetTerritory(terrNames[i]);
                        selectedTerritory = i;
                        isDropdownOpen = false;
                    }
                }
                GUILayout.EndVertical();
                GUILayout.EndArea();
            }
            
            if (Event.current.type == EventType.repaint)
            {
                toolTip.SetUnit(tooltipUnit?.Name ?? string.Empty);
            }
            
        }
        GUI.enabled = true;
    }
}