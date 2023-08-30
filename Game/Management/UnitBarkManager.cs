using UnityEngine;
using System.Collections.Generic;
using FracturedState.Game;
using FracturedState.Game.Data;

public class UnitBarkManager : MonoBehaviour
{
    public static UnitBarkManager Instance { get; private set; }

    private const float TimeBetweenDamageBarks = 5;
    private const int DamageBarkChange = 75;
    private const float CustomBarkTime = 30;

    private const float TimeBetweenDeathBarks = 5;
    
    [SerializeField]
    AudioSource newSquadSource;
    [SerializeField]
    AudioSource abilitySource;
    [SerializeField]
    AudioSource damageSource;

    AudioSource audioSource;
    Dictionary<string, AudioClip> barks;
    float lastDamageBarkTime;
    float lastDeathBarkTime;
    float last2DBarkTime;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        barks = new Dictionary<string, AudioClip>();
        Instance = this;
    }

    public void UpdateVolume(float volume)
    {
        newSquadSource.volume = volume;
        abilitySource.volume = volume;
        damageSource.volume = volume;
        audioSource.volume = volume;
    }

    public bool DamageBarkRoll()
    {
        return Time.time - lastDamageBarkTime > TimeBetweenDamageBarks && Random.Range(0, 100) > DamageBarkChange;
    }

    public void SelectBark(UnitObject unit)
    {
        if (unit.Voices != null)
        {
            RandomBark(unit.Voices.Select);
        }
    }

    public void MoveBark(UnitObject unit)
    {
        if (unit.Voices != null)
        {
            RandomBark(unit.Voices.Move);
        }
    }

    public void EnterBark(UnitObject unit)
    {
        if (unit.Voices != null)
        {
            RandomBark(unit.Voices.Enter);
        }
    }

    public void AttackBark(UnitObject unit)
    {
        if (unit.Voices != null)
        {
            RandomBark(unit.Voices.Attack);
        }
    }

    public void RetreatBark(UnitObject unit)
    {
        if (unit.Voices != null)
        {
            RandomBark(unit.Voices.Retreat);
        }
    }

    public void EnterTransportBark(UnitObject unit)
    {
        if (unit.Voices != null)
        {
            RandomBark(unit.Voices.TransportEnter);
        }
    }

    public void TakeDamageBark(UnitObject unit)
    {
        if (unit.Voices != null)
        {
            if (!damageSource.isPlaying)
            {
                RandomBark(unit.Voices.Damage, damageSource);
                lastDamageBarkTime = Time.time;
            }
        }
    }

    public void DeathBark(UnitObject unit)
    {
        if (unit.Voices != null && Time.time - lastDeathBarkTime > TimeBetweenDeathBarks)
        {
            if (!damageSource.isPlaying)
            {
                RandomBark(unit.Voices.Death, damageSource);
                lastDeathBarkTime = Time.time;
            }
        }
    }

    public void NewSquadBark(Faction faction)
    {
        if (!string.IsNullOrEmpty(faction.NewSquadSound))
        {
            AudioClip clip;
            if (!barks.TryGetValue(faction.NewSquadSound, out clip))
            {
                clip = DataUtil.LoadBuiltInBark(faction.NewSquadSound);
            }
            if (newSquadSource.isPlaying)
            {
                newSquadSource.Stop();
            }
            newSquadSource.clip = clip;
            newSquadSource.Play();
        }
    }
    
    public void AbilityBark(Ability ability)
    {
        RandomBark(ability.Barks, abilitySource);
    }

    public void Random2DBark(string[] barks)
    {
        if (!audioSource.isPlaying)
        {
            if (Time.time - last2DBarkTime > CustomBarkTime)
            {
                last2DBarkTime = Time.time;
                RandomBark(barks);
            }
        }
    }

    void RandomBark(string[] clips, AudioSource src = null)
    {
        if (clips != null && clips.Length > 0)
        {
            if (src == null)
                src = audioSource;

            string rand = clips[Random.Range(0, clips.Length)];
            AudioClip clip;
            if (!barks.TryGetValue(rand, out clip))
            {
                clip = DataUtil.LoadBuiltInBark(rand);
                barks[rand] = clip;
            }
            if (src.isPlaying)
            {
                src.Stop();
            }
            src.clip = clip;
            src.Play();
        }
    }
}