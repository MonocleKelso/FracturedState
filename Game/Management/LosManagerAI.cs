using FracturedState.Game;
using FracturedState.Game.AI;
using FracturedState.Game.Nav;
using FracturedState.Game.Network;
using UnityEngine;

public class LosManagerAI : LosManager
{
    protected override void OnTriggerEnter(Collider other)
    {
        if (squad == null) return;
        
        var cover = other.GetComponent<CoverManager>();
        if (cover != null)
        {
            squad.RegisterCoverObject(cover);
        }
        else if (unitManager.IsFriendly && other.gameObject.layer == GameConstants.ExteriorLayer)
        {
            var structure = other.transform.GetAbsoluteParent().GetComponent<StructureManager>();
            if (structure != null)
            {
                structure.AddVisible();
            }
        }
        else
        {
            var enemy = other.GetComponent<UnitManager>();
            if (enemy == null) return;
            
            if (enemy.OwnerTeam != unitManager.OwnerTeam)
            {
                var friendly = enemy.OwnerTeam.Side == unitManager.OwnerTeam.Side;
                if (unitManager.WorldState == State.Interior && !friendly)
                {
                    if (unitManager.IsOnFirePoint || (enemy.WorldState == State.Interior && enemy.CurrentStructure == unitManager.CurrentStructure))
                    {
                        squad.RegisterVisibleUnit(unitManager, enemy);
                    }
                }
                else if (!friendly)
                {
                    squad.RegisterVisibleUnit(unitManager, enemy);
                }

                if (!FracNet.Instance.IsHost || unitManager.StateMachine.CurrentState is UnitPendingState) return;
                
                var target = unitManager.DetermineTarget(null);
                if (target == null) return;
                
                unitManager.NetMsg.CmdSetTarget(target.NetMsg.NetworkId);
                unitManager.StateMachine.ChangeState(new UnitPendingState(unitManager));
            }
        }
    }

    protected override void OnTriggerExit(Collider other)
    {
        if (squad == null) return;
        
        var cover = other.GetComponent<CoverManager>();
        if (cover != null)
        {
            squad.UnregisterCoverObject(cover);
        }
        else if (unitManager.IsFriendly && other.transform.gameObject.layer == GameConstants.ExteriorLayer)
        {
            var structure = other.transform.GetAbsoluteParent().GetComponent<StructureManager>();
            if (structure != null)
            {
                structure.RemoveVisible();
            }
        }
        else
        {
            var unit = other.GetComponent<UnitManager>();
            if (unit != null)
            {
                squad.UnregisterVisibleUnit(unitManager, unit);
            }
        }
    }

    protected override void OnDestroy()
    {
        if (unitManager.IsFriendly)
        {
            base.OnDestroy();
        }
    }
}