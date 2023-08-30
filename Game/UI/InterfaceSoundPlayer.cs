using FracturedState.Game;
using UnityEngine;

public class InterfaceSoundPlayer : MonoBehaviour
{
    [SerializeField()]
    private AudioClip buttonClick;
    [SerializeField()]
    private AudioClip buttonHover;
    [SerializeField()]
    private AudioClip smallButtonClick;
    [SerializeField()]
    private AudioClip techCapture;
    [SerializeField]
    private AudioClip territoryGain;
    [SerializeField]
    private AudioClip territoryLose;
    [SerializeField()]
    private AudioClip buildingEnter;

    private static InterfaceSoundPlayer instance;
    private static AudioSource src;
    private static float baseVolume;

    public static AudioClip BuildingEnter => instance.buildingEnter;


    private void Awake()
    {
        if (instance != null)
            throw new FracturedStateException("More than one InterfaceSoundPlayer instance declared");

        instance = this;
        src = GetComponent<AudioSource>();
        baseVolume = src.volume;
    }

    private static void PlaySound(AudioClip clip)
    {
        src.PlayOneShot(clip);
    }

    public static void UpdateVolume(float volume)
    {
        src.volume = baseVolume * volume;
    }

    public static void PlayButtonClick()
    {
        PlaySound(instance.buttonClick);
    }

    public static void PlayButtonHover()
    {
        PlaySound(instance.buttonHover);
    }

    public static void PlaySmallButtonClick()
    {
        PlaySound(instance.smallButtonClick);
    }

    public static void PlayCapture()
    {
        PlaySound(instance.techCapture);
    }

    public static void PlayTerritoryGain()
    {
        PlaySound(instance.territoryGain);
    }

    public static void PlayTerritoryLoss()
    {
        PlaySound(instance.territoryLose);
    }
}