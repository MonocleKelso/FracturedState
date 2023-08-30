using FracturedState.Game.AI;
using FracturedState.Game.Data;
using FracturedState.Game.Management;
using FracturedState.Game.Network;
using System.Collections;
using UnityEngine;

public class GarrisonPointManager : MonoBehaviour
{
    StructureManager owner;
    Team currentTeam;
    GarrisonPointInfo currentInfo;
    UnitManager currentUnit;
    Coroutine spawner;

    public UnitManager Unit { get { return currentUnit; } }

    public void Init(StructureManager structure)
    {
        owner = structure;
    }

    public void SpawnUnit(GarrisonPointInfo info)
    {
        currentInfo = info;
        if(spawner != null)
        {
            StopCoroutine(spawner);
        }
        spawner = StartCoroutine(SpawnUnit());
    }

    public void RespawnUnit()
    {
        if (currentInfo != null)
        {
            if (spawner != null)
            {
                StopCoroutine(spawner);
            }
            spawner = StartCoroutine(SpawnUnit());
        }
    }

    public void Stop()
    {
        if (spawner != null)
        {
            StopCoroutine(spawner);
            spawner = null;
        }
    }

    public void SetCurrentUnit(UnitManager unit)
    {
        if (currentUnit != null)
        {
            KillUnit();
        }
        currentUnit = unit;
        owner.AddGarrisonUnit(unit);
        unit.StateMachine.ChangeState(new UnitGarrisonIdleState(unit));
    }

    public void KillUnit()
    {
        Stop();
        if (currentUnit != null && currentUnit.DamageProcessor != null)
        {
            currentUnit.DamageProcessor.TakeDamage(int.MaxValue, null, Weapon.DummyName);
        }
    }

    IEnumerator SpawnUnit()
    {
        currentTeam = owner.OwnerTeam;
        yield return new WaitForSeconds(currentInfo.RespawnTime);
        if (currentTeam == owner.OwnerTeam && (currentUnit == null || currentUnit.OwnerTeam != owner.OwnerTeam))
        {
            FracNet.Instance.SpawnGarrison(owner, owner.OwnerTeam, currentInfo.Unit, currentInfo.Point);
        }
    }
}