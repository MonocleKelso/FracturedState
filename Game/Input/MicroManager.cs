using System.Collections.Generic;
using FracturedState;
using FracturedState.Game;
using FracturedState.Game.AI;
using FracturedState.Game.Data;
using FracturedState.Game.Nav;
using UnityEngine;
using Vectrosity;
using LineManager = FracturedState.Game.LineManager;

public class MicroManager : MonoBehaviour
{
    private const float MaxMicroRange = 30;
    [SerializeField] private GameObject microRangeHelper;
    private GameObject microRangeInstance;
    
    public bool AbilityActive { get; private set; }
    public bool InMicro { get; private set; }

    private Squad squad;
    private Ability activeAbility;
    private CoverManager tmpCover;
    private Transform coverOrFirePoint;
    private bool isFirePoint;

    private readonly List<GameObject> coverHelpers = new List<GameObject>();
    private readonly List<GameObject> firePointHelpers = new List<GameObject>();
    private readonly List<VectorLine> helperLines = new List<VectorLine>();
    private VectorLine currentLine;
    private Vector3? microDestination;
    private CoverManager[] nearbyCover;

    public UnitManager MicroUnit { get; set; }

    private void Awake()
    {
        microRangeInstance = Instantiate(microRangeHelper);
        microRangeInstance.SetActive(false);
    }

    public void Toggle()
    {
        if (InMicro)
        {
            if (MicroUnit != null)
            {
                if (coverOrFirePoint != null)
                {
                    if (tmpCover != null)
                    {
                        MicroUnit.SetMicroState(new MicroTakeCoverState(MicroUnit, tmpCover, coverOrFirePoint));
                    }
                    else
                    {
                        MicroUnit.SetMicroState(new MicroTakeFirepointState(MicroUnit, coverOrFirePoint));
                    }
                    
                    MicroUnit.PropagateMicroState();
                }
                else if (microDestination != null)
                {
                    MicroUnit.SetMicroState(new MicroMoveState(MicroUnit, microDestination.Value));
                    MicroUnit.PropagateMicroState();
                }
            }
            ResetMicroData();
        }
        else
        {
            if (MicroUnit != null)
            {
                squad = MicroUnit.Squad;
                microRangeInstance.SetActive(true);
            }
            else
            {
                for (var i = 0; i < SelectionManager.Instance.SelectedUnits.Count; i++)
                {
                    var unit = SelectionManager.Instance.SelectedUnits[i];
                    if (unit != null)
                    {
                        squad = unit.Squad;
                        break;
                    }
                }
                microRangeInstance.SetActive(false);
            }
        }
        InMicro = !InMicro;
    }

    private void ResetMicroData()
    {
        MicroUnit = null;
        activeAbility = null;
        coverOrFirePoint = null;
        microDestination = null;
        nearbyCover = null;
        microRangeInstance.SetActive(false);
        for (var c = 0; c < coverHelpers.Count; c++)
        {
            ObjectPool.Instance.ReturnPooledObject(coverHelpers[c]);
        }

        for (var i = 0; i < firePointHelpers.Count; i++)
        {
            ObjectPool.Instance.ReturnPooledObject(firePointHelpers[i]);
        }
        
        coverHelpers.Clear();
        firePointHelpers.Clear();
        
        for (var h = 0; h < helperLines.Count; h++)
        {
            LineManager.Instance.ReturnLine(helperLines[h]);
        }
        
        helperLines.Clear();
        currentLine = null;
    }

    public void ClearActiveAbility()
    {
        activeAbility = null;
        AbilityActive = false;
    }

    /// <summary>
    /// Checks mouse position against ability hotbars, micro'd units, and micro'd actions and draws the appropriate cursor.
    /// Returns a boolean to tell the cursor updater whether or not any of these checks succeeded and therefore if any
    /// further processing of the cursor is necessary in this frame
    /// </summary>
    /// <param name="cursor">The cursor settings object to update</param>
    /// <returns>A boolean indicating whether or not this manager updated the cursor</returns>
    public bool UpdateCursor(CursorSettings cursor)
    {
        // set cursor to pointer if in any of the hotbars;
        if (CursorInHotbars())
        {
            cursor.SetPointerCursor();
            return true;
        }

        if (activeAbility == null) return false;
        
        if (activeAbility.Targetting == TargetType.Enemy)
        {
            if (RaycastUtil.RaycastEnemyAtMouse() != null)
            {
                cursor.SetAttackCursor();
            }
            else
            {
                cursor.SetNoAttackCursor();
            }
        }
        else if (activeAbility.Targetting == TargetType.Ground)
        {
            return true;
        }
        else if (activeAbility.Targetting == TargetType.Friendly)
        {
            cursor.SetSelectCursor();
        }
        return true;

    }

    public static bool CursorInHotbars()
    {
        return RaycastUtil.IsMouseInUI();
    }


    private void Update()
    {
        if (InMicro && MicroUnit == null)
        {
            ResetMicroData();
            InMicro = false;
            return;
        }

        if (!InMicro) return;

        if (MicroUnit == null) return;

        if (MicroUnit.StateMachine.CurrentState is MicroUseAbilityState)
        {
            MicroUnit = null;
            return;
        }
        
        // toggle off if we release mouse button while microing a unit
        if (Input.GetMouseButtonUp(0))
        {
            Toggle();
            return;
        }
        
        DrawCoverHelpers();
        DrawFirepointHelpers();
        
        // evaluate potential move postion and nearby cover/fire points
        if (RaycastUtil.IsMouseUnderTerrain())
        {
            var destination = RaycastUtil.RaycastTerrainAtMouse().point;
            // additional check when micro moving inside to make sure you can't micro out of a building
            if (MicroUnit != null && MicroUnit.WorldState == State.Interior && MicroUnit.CurrentStructure != null)
            {
                if (!MicroUnit.CurrentStructure.ContainsPoint(new Vector3(destination.x, destination.y + 1, destination.z)))
                {
                    return;
                }
            }
            microDestination = destination;
            coverOrFirePoint = null;
            var dist = ConfigSettings.Instance.Values.CursorPointSnapDistance;
            if (nearbyCover != null)
            {
                foreach (var nc in nearbyCover)
                {
                    if (!nc.CanOccupy(MicroUnit))
                        continue;

                    var cp = nc.GetClosestPointToPosition(destination);
                    var toTarget = (cp.position - destination).sqrMagnitude;
                    if (toTarget < dist)
                    {
                        coverOrFirePoint = cp;
                        tmpCover = nc;
                        dist = toTarget;
                        microDestination = cp.position;
                    }
                }
            }

            if (MicroUnit?.CurrentStructure != null)
            {
                var firepoints = MicroUnit.CurrentStructure.AvailableFirePoints;
                for (var i = 0; i < firepoints.Count; i++)
                {
                    var toTarget = (destination - firepoints[i].position).sqrMagnitude;
                    if (toTarget < dist)
                    {
                        coverOrFirePoint = firepoints[i];
                        tmpCover = null;
                        dist = toTarget;
                        microDestination = firepoints[i].position;
                    }
                }
            }
            
            // clamp micro movement to within range of average squad mate position
            if (MicroUnit?.Squad != null)
            {
                var avg = MicroUnit.Squad.GetAveragePosition();
                microRangeInstance.transform.position = avg + Vector3.up * 0.2f;
                var toMicro = microDestination.Value - avg;
                toMicro = Vector3.ClampMagnitude(toMicro, MaxMicroRange * 0.5f) + avg;
                microDestination = toMicro;
            }
                        
            // update helper line
            if (currentLine == null)
            {
                currentLine = LineManager.Instance.GetMoveLine();
                helperLines.Add(currentLine);
            }
            currentLine.points3[0] = MicroUnit.transform.position + Vector3.up * 0.2f;
            currentLine.points3[1] = microDestination.Value + Vector3.up * 0.2f;
        }
        else
        {
            microDestination = null;
            if (currentLine != null)
            {
                LineManager.Instance.ReturnLine(currentLine);
                currentLine = null;
            }
        }
    }

    private void DrawCoverHelpers()
    {
        if (!MicroUnit.Data.CanTakeCover) return;
        
        // turn off all the helpers we've used so far in case a spot was taken since the last time we evaluated
        for (var i = 0; i < coverHelpers.Count; i++)
        {
            coverHelpers[i].SetActive(false);
        }
        
        nearbyCover = squad.GetNearbyCover();
        var coverCounter = 0;
        for (var i = 0; i < nearbyCover.Length; i++)
        {
            if (!nearbyCover[i].CanOccupy(MicroUnit)) continue;
            
            for (var p = 0; p < nearbyCover[i].EmptyPoints.Count; p++)
            {
                if (coverCounter >= coverHelpers.Count)
                {
                    var helper = ObjectPool.Instance.GetPooledObject(DataLocationConstants.CoverHelperPrefab);
                    helper.SetLayerRecursively(nearbyCover[i].gameObject.layer);
                    helper.transform.position = nearbyCover[i].EmptyPoints[p].position;
                    coverHelpers.Add(helper);
                }
                else
                {
                    var h = coverHelpers[coverCounter];
                    h.SetLayerRecursively(nearbyCover[i].gameObject.layer);
                    h.transform.position = nearbyCover[i].EmptyPoints[p].position;
                    h.SetActive(true);
                }
                coverCounter++;
            }
        }
    }

    private void DrawFirepointHelpers()
    {
        if (MicroUnit.CurrentStructure == null) return;

        // turn off all helpers we've used so far in case a spot was taken since the last time we evaluated
        for (var i = 0; i < firePointHelpers.Count; i++)
        {
            firePointHelpers[i].SetActive(false);
        }
        
        var firePoints = MicroUnit.CurrentStructure.AvailableFirePoints;
        var fpCounter = 0;
        for (var i = 0; i < firePoints.Count; i++)
        {
            GameObject helper;
            if (fpCounter >= firePointHelpers.Count)
            {
                helper = ObjectPool.Instance.GetPooledObject(DataLocationConstants.FirepointHelperPrefab);
                firePointHelpers.Add(helper);
            }
            else
            {
                helper = firePointHelpers[fpCounter];
                helper.SetActive(true);
            }

            helper.SetLayerRecursively(GameConstants.InteriorLayer);
            helper.transform.position = firePoints[i].position + Vector3.up * 0.5f;
            fpCounter++;
        }
    }
}