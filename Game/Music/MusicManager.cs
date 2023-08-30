using System.Collections;
using System.Linq;
using FracturedState.Game;
using FracturedState.Game.Management;
using FracturedState.Game.Music;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [SerializeField]
    private AudioSource introSource;
    [SerializeField]
    private AudioSource loopSource;
    [SerializeField]
    private AudioClip mainTheme;
    [SerializeField]
    private AudioClip warningTrack;
    [SerializeField]
    private AudioClip warningHit;
    [SerializeField]
    private FactionMusic[] FactionMusicSettings;

    private const float BaseVolume = 0.25f;
    public float Volume { get; private set; }

    public static MusicManager Instance;

    private FactionMusic playerMusic;

    private int combatUnits;
    public bool InCombatTrack { get; private set; }

    private readonly TrackStateMachine stateMachine = new TrackStateMachine();
    private const float CombatSwapWaitTime = 10;
    private float combatSwapTime;

    private void Awake()
    {
        if (Instance != null)
            throw new FracturedStateException("Cannot have more than one MusicManager instance");

        Instance = this;
    }

    private void Start()
    {
        PlayMainTheme();
    }

    private void Update()
    {
        stateMachine.Update();
        if (combatSwapTime > 0)
        {
            combatSwapTime -= Time.deltaTime;
            if (combatSwapTime <= 0 && !InCombatTrack)
            {
                PlayAmbientLoop();
            }
        }
    }

    public void SetVolume(float volume)
    {
        Volume = BaseVolume * volume;
        introSource.volume = Volume;
        loopSource.volume = Volume;
    }

    public void AddCombatUnit()
    {
        combatUnits++;
        if (!InCombatTrack && introSource.clip != warningTrack && introSource.clip != warningHit)
        {
            InCombatTrack = true;
            PlayCombatTrack();
        }
    }

    public void RemoveCombatUnit()
    {
        combatUnits--;
        if (combatUnits < 0)
            combatUnits = 0;
        if (combatUnits == 0 && introSource.clip != warningTrack && introSource.clip != warningHit)
        {
            InCombatTrack = false;
            combatSwapTime = CombatSwapWaitTime;
        }
    }

    public void PlayMainTheme()
    {
        stateMachine.ChangeState(new MainThemeTrackState(mainTheme, introSource, loopSource));
        combatSwapTime = -1;
    }

    public void SetPlayerFaction(string faction)
    {
        playerMusic = FactionMusicSettings.FirstOrDefault(m => m.FactionName == faction);
    }

    public void PlayAmbientLoop()
    {
        if (!(stateMachine.CurrentState is AmbientLoopTrackState))
            stateMachine.ChangeState(new AmbientLoopTrackState(playerMusic, introSource, loopSource, loopSource.clip == mainTheme));
    }

    private void PlayCombatTrack()
    {
        if (!(stateMachine.CurrentState is CombatTrackState))
            stateMachine.ChangeState(new CombatTrackState(playerMusic, introSource, loopSource));
    }

    public void PlayWinTrack()
    {
        PlaySingleTrack(playerMusic.WinTrack);
    }

    public void PlayLoseTrack()
    {
        PlaySingleTrack(playerMusic.LoseTrack);
    }

    private void PlaySingleTrack(AudioClip track)
    {
        stateMachine.ChangeState(new SingleTrackState(track, introSource, loopSource));
    }

    public void PlayWarningTrack()
    {
        stateMachine.ChangeState(new WarningTrackState(warningTrack, introSource, loopSource));
    }

    public void StopWarningTrack()
    {
        if (!SkirmishVictoryManager.MatchInProgress || !(stateMachine.CurrentState is WarningTrackState)) return;
        introSource.Stop();
        if (combatUnits > 0)
        {
            PlayCombatTrack();
        }
        else
        {
            PlayAmbientLoop();
        }
    }

    public void PlayWarningHit()
    {
        stateMachine.ChangeState(null);
        introSource.Stop();
        loopSource.Stop();
        introSource.volume = Volume;
        introSource.clip = warningHit;
        loopSource.volume = 0;
        introSource.Play();
        StartCoroutine(WaitForHitDoAmbient());
    }

    private IEnumerator WaitForHitDoAmbient()
    {
        yield return new WaitForSeconds(warningHit.length);
        PlayAmbientLoop();
    }

    public void FadeOut()
    {
        if (loopSource.clip != null)
        {
            StartCoroutine(FadeOutExecute());
        }
    }

    private IEnumerator FadeOutExecute()
    {
        while (loopSource.volume > 0)
        {
            loopSource.volume -= 0.2f * Time.deltaTime;
            yield return null;
        }
    }
}