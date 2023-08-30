using FracturedState.Game;
using UnityEngine;

public class LosManager : MonoBehaviour
{
    protected UnitManager unitManager;
    protected Squad squad;

    public void Awake()
    {
        unitManager = transform.parent.GetComponent<UnitManager>();
    }

    public void SetSquad(Squad squad)
    {
        if (this.squad == null)
            this.squad = squad;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (squad == null) return;
        
        var u = other.GetComponent<UnitManager>();
        if (u != null && u.OwnerTeam != unitManager.OwnerTeam && !u.IsFriendly)
        {
            squad.RegisterVisibleUnit(unitManager, u);
        }
        else if (u == null)
        {
            // otherwise it's not a unit so check for cover or structure
            var cover = other.GetComponent<CoverManager>();
            if (cover != null)
            {
                squad.RegisterCoverObject(cover);
            }
            else if (other.gameObject.layer == GameConstants.ExteriorLayer)
            {
                var structure = other.transform.GetAbsoluteParent().GetComponent<StructureManager>();
                if (structure != null)
                {
                    structure.AddVisible();
                }
            }
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (squad == null) return;
        
        var u = other.GetComponent<UnitManager>();
        if (unitManager.IsOnFirePoint && (other.gameObject.layer == GameConstants.ExteriorEnemyLayer || other.gameObject.layer == GameConstants.InteriorEnemyLayer))
        {
            squad.UnregisterVisibleUnit(unitManager, u);
        }
        else
        {
            var layerCheck = (unitManager.gameObject.layer == GameConstants.ExteriorUnitLayer) ? GameConstants.ExteriorEnemyLayer : GameConstants.InteriorEnemyLayer;
            if (other.gameObject.layer == layerCheck)
            {
                squad.UnregisterVisibleUnit(unitManager, u);
            }
            else
            {
                var cover = other.gameObject.GetComponent<CoverManager>();
                if (cover != null)
                {
                    squad.UnregisterCoverObject(cover);
                }
                else if (other.transform.gameObject.layer == GameConstants.ExteriorLayer)
                {
                    var structure = other.transform.GetAbsoluteParent().GetComponent<StructureManager>();
                    if (structure != null)
                    {
                        structure.RemoveVisible();
                    }
                }
            }
        }
    }

    protected virtual void OnDestroy()
    {
        var sc = GetComponent<SphereCollider>();
        var nearby = Physics.OverlapSphere(unitManager.transform.position, sc.radius, GameConstants.ExteriorMask);
        foreach (var col in nearby)
        {
            var sm = col.transform.GetAbsoluteParent().GetComponent<StructureManager>();
            if (sm != null)
            {
                sm.RemoveVisible();
            }
        }
    }
}