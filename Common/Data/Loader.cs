using System;
using System.Collections;
using System.Globalization;
using FracturedState;
using FracturedState.Game;
using FracturedState.Game.Data;
using FracturedState.Game.Management;
using FracturedState.Game.Network;
using FracturedState.UI;
using FracturedState.UI.Events;
using Monocle.Threading;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Loader : MonoBehaviour
{
    public static Loader Instance { get; private set; }

    [SerializeField] private GameObject startMenu;

    [SerializeField] private WorldRecruitButton _worldRecruitButton;
    public WorldRecruitButton RecruitButton => _worldRecruitButton;
    
    [SerializeField]
    private GameObject defeatWarning;

    [SerializeField] public Gradient HealthGradient;

    public bool ProbeRenderComplete { get; private set; }

    private Coroutine warningFade;
    private bool runLocalWarning;

    private IEnumerator Start()
    {
        LocalizedString.Init();
        ScreenResolutionSelect.StoreAvailableResolutions();
        Instance = this;

#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
        SteamManager.Init();
#endif

        // callback to print error messages on the screen
        Application.logMessageReceived += ExtensionMethods.HandleError;

        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;

        // Load player profile data
        var profile = ProfileManager.GetActiveProfile();
        MusicManager.Instance.SetVolume(profile.GameSettings.MusicVolume);
        InterfaceSoundPlayer.UpdateVolume(profile.GameSettings.UIVolume);
        UnitBarkManager.Instance.UpdateVolume(profile.GameSettings.UIVolume);
        yield return null;

        // load game.xml and mod.xml if applicable
        ConfigSettings.Instance.LoadDefaultSettings();

        // load all cacheable xml data and Python scripts for the game
        XmlCacheManager.PopulateAllCaches();
        yield return null;
        // initialize pool of unit selection objects
        ObjectPool.Instance.InitSelectionPool();
        yield return null;

        Instantiate(startMenu);
        Cursor.visible = true;


        // seed random number
        var now = DateTime.Now;
        Random.InitState(now.Hour * now.Minute * now.Second * now.Millisecond);
    }

    private static ReflectionProbe BuildTerrainProbe()
    {
        var map = SkirmishVictoryManager.CurrentMap;
        return BuildTerrainProbe(map.XUpperBound, map.XLowerBound, map.ZUpperBound, map.ZLowerBound);
    }

    public static ReflectionProbe BuildTerrainProbe(int xMax, int xMin, int yMax, int yMin)
    {
        var terrainProbe = new GameObject("TerrainProbe");
        var tProbe = terrainProbe.AddComponent<ReflectionProbe>();
        tProbe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
        tProbe.mode = ReflectionProbeMode.Realtime;
        tProbe.timeSlicingMode = ReflectionProbeTimeSlicingMode.AllFacesAtOnce;
        tProbe.cullingMask = GameConstants.ReflectionMask;
        var res = (int)ProfileManager.GetActiveProfile().GameSettings.ReflectionQuality;
        if (res < 16) res = 16;
        tProbe.resolution = res;
        tProbe.center = new Vector3((xMax + xMin) / 2f, 20, (yMax + yMin) / 2f);
        tProbe.size = new Vector3(Mathf.Abs(xMax) + Mathf.Abs(xMin) + 10, 40, Mathf.Abs(yMax) + Mathf.Abs(yMin) + 10);
        return tProbe;
    }

    public IEnumerator RenderProbes(GameObject[] terrains, GameObject[] structures, GameObject[] props)
    {
        ProbeRenderComplete = false;
        Action<GameObject, bool> structureToggle = (s, structureEnabled) =>
        {
            var sm = s.GetComponent<StructureManager>();
            if (sm != null)
            {
                sm.ToggleExterior(structureEnabled);
            }
            else
            {
                SetRenderEnabled(s.GetComponentsInChildren<Renderer>(true), structureEnabled);
            }
        };
        yield return null;
        // turn all structures and props off and render terrain reflections
        if (structures != null)
        {
            foreach (var s in structures)
            {
                structureToggle(s, false);
            }
        }
        if (props != null)
        {
            foreach (var p in props)
            {
                var renders = p.GetComponentsInChildren<Renderer>();
                if (renders != null)
                {
                    SetRenderEnabled(renders, false);
                }
            }
        }
        yield return null;
        var tProbe = BuildTerrainProbe();
        tProbe.transform.SetParent(terrains[0].transform.parent, true);
        var tId = tProbe.RenderProbe();
        while (!tProbe.IsFinishedRendering(tId))
        {
            yield return null;
        }
        yield return null;
        // turn props back on
        if (props != null)
        {
            foreach (var p in props)
            {
                var renders = p.GetComponentsInChildren<Renderer>();
                if (renders != null)
                {
                    SetRenderEnabled(renders, true);
                }
            }
        }
        // turn structures back on
        if (structures != null)
        {
            foreach (var s in structures)
            {
                structureToggle(s, true);
            }
        }
        yield return null;
        // turn each structure off one at a time and render probes for that structure
        if (structures != null)
        {
            foreach (var s in structures)
            {
                var probes = s.GetComponentsInChildren<ReflectionProbe>();
                if (probes != null && probes.Length > 0)
                {
                    structureToggle(s, false);
                    yield return null;
                    foreach (var probe in probes)
                    {
                        var id = probe.RenderProbe();
                        while (!probe.IsFinishedRendering(id))
                        {
                            yield return null;
                        }
                    }
                    structureToggle(s, true);
                    yield return null;
                }
            }
            FracNet.Instance.NetworkActions.CmdUpdateMapProgress(0.8f);
        }
        // turn each prop off one at a time and render probes for that prop
        if (props != null)
        {
            foreach (var p in props)
            {
                var probes = p.GetComponentsInChildren<ReflectionProbe>();
                if (probes != null && probes.Length > 0)
                {
                    var renders = p.GetComponentsInChildren<Renderer>();
                    SetRenderEnabled(renders, false);
                    yield return null;
                    foreach (var probe in probes)
                    {
                        var id = probe.RenderProbe();
                        while (!probe.IsFinishedRendering(id))
                        {
                            yield return null;
                        }
                    }
                    SetRenderEnabled(renders, true);
                }
            }
        }
        ProbeRenderComplete = true;
    }

    private static void SetRenderEnabled(Renderer[] renders, bool renderEnabled)
    {
        if (renders == null) return;
        
        foreach (var r in renders)
        {
            r.enabled = renderEnabled;
        }
    }

    public void OnApplicationQuit()
    {
        PathRequestManager.Instance.Cleanup();
        FracNet.Instance.Disconnect();
    }

    public IEnumerator EvalTeamElimination(Team team)
    {
        if (team == FracNet.Instance.LocalTeam)
        {
            Instance.LocalWarning();
        }
        else
        {
            EventCanvas.CountdownPlayer(team);
        }

        var timer = 60f;
        // these checks seem redundant but we want the coroutine to complete if the team becomes not eliminated
        // or surrenders so that if they capture a territory and lose it again within the 60 seconds we don't want
        // overlapping eval coroutines running
        while (timer > 0 && team.SurrenderTime <= 0 && SkirmishVictoryManager.IsTeamEliminated(team))
        {
            timer -= Time.deltaTime;
            yield return null;
        }
        if (!SkirmishVictoryManager.IsTeamEliminated(team) || team.SurrenderTime > 0) yield break;
        
        team.OnDefeated(team);
    }
    
    public void InterruptLocalWarning()
    {
        LocalEliminationWarning.Stop();
        runLocalWarning = false;
        MusicManager.Instance.StopWarningTrack();
    }

    public void LocalWarning()
    {
        if (!runLocalWarning)
        {
            runLocalWarning = true;
            MusicManager.Instance.PlayWarningTrack();
            LocalEliminationWarning.Begin();
        }
    }

    public IEnumerator InformMatchEnd(Team winner)
    {
        AISimulator.Instance.StopSimulation();
        yield return new WaitForSeconds(5);
        if (winner.IsHuman)
        {
            var wAction = GlobalNetworkActions.GetActions(winner);
            wAction.RpcEndMatch();
        }
        else
        {
            FracNet.Instance.NetworkActions.RpcEndMatchAI(winner.PlayerName);
        }
    }


    public void ShowDefeatIndicator()
    {
        defeatWarning.SetActive(true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F12))
        {
            ScreenCapture.CaptureScreenshot(DataLocationConstants.GameRootPath + DataLocationConstants.ScreenshotDirectory +
                "/" + DateTime.Now.ToString(CultureInfo.InvariantCulture).Replace("/", "-").Replace(" ", "_").Replace(":", "-") + ".png");
        }
    }
}