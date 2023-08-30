using UnityEngine;

public class ProximityManager : MonoBehaviour
{
    private UnitManager unitManager;

    private void Awake()
    {
        unitManager = transform.parent.GetComponent<UnitManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        var target = other.GetComponent<UnitManager>();
        if (target != null && target.NetMsg != null && unitManager != null && unitManager.NetMsg != null &&
            !target.IsMine && !target.IsFriendly && target.WorldState == unitManager.WorldState)
        {
            unitManager.NetMsg.CmdSetTarget(target.NetMsg.NetworkId);
        }
    }
}