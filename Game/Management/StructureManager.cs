using System.Collections.Generic;
using System.Linq;
using FracturedState;
using FracturedState.Game;
using FracturedState.Game.Data;
using FracturedState.Game.Management;
using FracturedState.Game.Nav;
using FracturedState.Game.Network;
using ThreeEyedGames;
using UnityEngine;
using UnityEngine.Rendering;

public class StructureManager : MonoBehaviour
{
    #region Statics
    private static Shader unoccupiedShader;
    private static Shader occupiedShader;

    public static bool HelperOn { get; private set; }

    public static void EliminationToggle(StructureManager structure)
    {
        // if the local team is in this structure by themselves then do nothing
        if (structure.occupyingUnits.Keys.Count == 1 && structure.occupyingUnits.ContainsKey(FracNet.Instance.LocalTeam))
        {
            return;
        }

        if (!structure.IsFriendlyOccupied)
        {
            structure.Toggle();
        }
    }

    private static readonly List<string> OwnedStructureNames = new List<string>();

    public static void ToggleHelperUI(bool enabled)
    {
        foreach (var s in FindObjectsOfType<StructureManager>())
        {
            if (s.HelperUI != null)
            {
                s.HelperUI.enabled = enabled;
            }
        }

        HelperOn = enabled;
    }

    public static void AddOwnedStructure(StructureManager manager)
    {
        var sName = manager.StructureData.Name;
        OwnedStructureNames.Add(sName);
        // if this is the only one of these buildings we have then check for tech unlocks and do fly outs
        if (OwnedStructureNames.Count(c => c == sName) > 1) return;
        var unlocks = manager.StructureData.Unlockables;
        if (unlocks == null) return;
        var faction = XmlCacheManager.Factions[FracNet.Instance.LocalTeam.Faction];
        foreach (var fUnit in faction.TrainableUnits)
        {
            var unitData = XmlCacheManager.Units[fUnit];
            // skip this unit if it has no prereqs
            if (unitData.PrerequisiteStructures == null || unitData.PrerequisiteStructures.Length == 0) continue;
            // check if this structure is a prereq
            var needsStructure = unitData.PrerequisiteStructures.Contains(sName);
            // skip this unit if this structure is not a prereq
            if (!needsStructure) continue;
            // send a fly out if this unit needs this structure and is now unlocked
            if (unitData.PrerequisiteStructures.All(HasStructure))
            {
                MultiplayerEventBroadcaster.UnlockUnit(unitData);
            }
        }
    }

    public static void RemoveOwnedStructure(StructureManager manager)
    {
        var sName = manager.StructureData.Name;
        OwnedStructureNames.Remove(sName);
        // raise event
        MultiplayerEventBroadcaster.LoseBuilding(sName);
        // if this is the last instance of this building that we owned then check prereqs
        var count = OwnedStructureNames.Count(s => s == sName);
        if (count == 0)
        {
            var faction = XmlCacheManager.Factions[FracNet.Instance.LocalTeam.Faction];
            foreach (var fUnit in faction.TrainableUnits)
            {
                var unitData = XmlCacheManager.Units[fUnit];
                if (unitData.PrerequisiteStructures == null || unitData.PrerequisiteStructures.Length == 0) continue;
                if (unitData.PrerequisiteStructures.Contains(sName))
                {
                    MultiplayerEventBroadcaster.LockUnit(unitData);
                }
            }
        }
    }

    public static bool HasStructure(string structureName)
    {
        return OwnedStructureNames.IndexOf(structureName) >= 0;
    }

    public static void ResetOwnedStructures()
    {
        OwnedStructureNames.Clear();
    }
    #endregion

    #region private fields
    private Transform exteriorParent;
    private Transform interiorParent;
    private List<Transform> firePoints;
    private List<Transform> entrances;
    private List<Transform> exits;
    private Dictionary<Transform, Transform> linkLookup;
    private List<UnitManager> units;
    private List<GarrisonPointManager> garrisonPoints;
    private Renderer[] exteriorRenderers;
    private Renderer[] interiorRenderers;
    private static Color buildingShroudColor = Color.clear;
    private Renderer[] shroudRenderers;
    private int visibleTracker;
    private GarrisonSquad garrisonSquad;
    private readonly Dictionary<Team, List<UnitManager>> occupyingUnits = new Dictionary<Team, List<UnitManager>>();
    private readonly List<Decal> exteriorDecals = new List<Decal>();
    private readonly List<Decal> interiorDecals = new List<Decal>();
    #endregion
    
    #region Properties
    public Structure StructureData { get; private set; }
    public NavGrid NavigationGrid { get; private set; }
    public NavGrid UnitPlacementGrid { get; private set; }
    public Team OwnerTeam { get; set; }
    public bool IsFriendlyOccupied { get; private set; }
    public float CurrentPoints { get; private set; }
    public List<Transform> AllFirePoints => firePoints;
    public List<Transform> AvailableFirePoints { get; private set; }

    public List<GameObject> ContainedProps { get; } = new List<GameObject>();

    private StructureHelperUI HelperUI { get; set; }

    private static Transform worldTransform;
    public static Transform WorldTransform
    {
        get
        {
            if (worldTransform == null)
                worldTransform = GameObject.Find("StructureParent").transform;

            return worldTransform;
        }
    }
    #endregion

    private void Awake()
    {
        if (unoccupiedShader == null)
        {
            unoccupiedShader = Shader.Find("Standard");
        }
        if (occupiedShader == null)
        {
            occupiedShader = Shader.Find("FracturedState/Transparent-Building");
        }
    }

    /// <summary>
    /// Assigns the given navigation grid to this structure.
    /// This is a one-time operation so subsequent calls to this method do nothing
    /// </summary>
    public void SetNavGrid(NavGrid navGrid)
    {
        if (NavigationGrid == null)
            NavigationGrid = navGrid;
    }

    /// <summary>
    /// Assigns the given navigation grid to be used as the unit offset grid for this structure
    /// This is a one-time operation so subsequent calls to this method do nothing
    /// </summary>
    public void SetPlacementGrid(NavGrid grid)
    {
        if (UnitPlacementGrid == null)
            UnitPlacementGrid = grid;
    }

    /// <summary>
    /// Returns the waypoint that connects to the given waypoint.  This method does not distinguish between enter->exit or exit->enter
    /// </summary>
    public Transform GetLinkedTransform(Transform other)
    {
        Transform link;
        return linkLookup.TryGetValue(other, out link) ? link : null;
    }

    /// <summary>
    /// Returns the closest entrance to the given position
    /// </summary>
    public Transform GetClosestEntrance(Vector3 pos)
    {
        return GetClosestTransform(entrances, pos);
    }

    /// <summary>
    /// Returns the exit that links to the entrance that is closest to the given point
    /// </summary>
    public Transform GetClosestExitToWorldPoint(Vector3 pos)
    {
        var dist = float.MaxValue;
        Transform theTran = null;
        for (var i = 0; i < exits.Count; i++)
        {
            Transform t;
            if (linkLookup.TryGetValue(exits[i], out t))
            {
                var sqm = (t.position - pos).sqrMagnitude;
                if (sqm < dist)
                {
                    dist = sqm;
                    theTran = exits[i];
                }
            }
        }
        return theTran;
    }

    /// <summary>
    /// Returns the closest available firepoint to the given position and removes it from availability
    /// </summary>
    public Transform TakeClosestFirePoint(Vector3 position)
    {
        if (AvailableFirePoints != null && AvailableFirePoints.Count > 0)
        {
            var point = GetClosestTransform(AvailableFirePoints, position);
            if (point == null) return null;
            
            AvailableFirePoints.Remove(point);
            return point;
        }
        return null;
    }

    private static Transform GetClosestTransform(List<Transform> trans, Vector3 pos)
    {
        var dist = float.MaxValue;
        Transform theTran = null;
        for (var i = 0; i < trans.Count; i++)
        {
            var sqm = (trans[i].position - pos).sqrMagnitude;
            if (sqm < dist)
            {
                dist = sqm;
                theTran = trans[i];
            }
        }
        return theTran;
    }

    /// <summary>
    /// Gives a firing position back to the managing structure.  This is called when a unit dies or leaves a firing position
    /// </summary>
    public void ReturnFirePoint(Transform fp)
    {
        AvailableFirePoints.Add(fp);
    }

    /// <summary>
    /// Removes the given firing position from the list of positions.  Called over the network where the owning player has already determined the
    /// firing position
    /// </summary>
    public void ForceRemoveFirePoint(Transform fp)
    {
        if (!AvailableFirePoints.Remove(fp))
        {
            throw new FracturedStateException("Attempted to force remove fire point " + fp.name + " from structure " + StructureData.Name +
                " but no point by that name was available");
        }
    }

    /// <summary>
    /// Attempts to find a fire point with the given name. First checks available points
    /// and returns without removing from availability. Next checks transform children.
    /// Returns null if not found
    /// </summary>
    /// <param name="fpName"></param>
    /// <returns></returns>
    public Transform FindFirePoint(string fpName)
    {
        for (var i = 0; i < AvailableFirePoints.Count; i++)
        {
            if (AvailableFirePoints[i].name == fpName)
            {
                return AvailableFirePoints[i];
            }
        }
        return interiorParent.GetChildByName(fpName);
    }

    public Transform GetExteriorChild(string childName)
    {
        return exteriorParent.Find(childName);
    }

    public Transform GetInteriorChild(string childName)
    {
        return interiorParent.Find(childName);
    }

    /// <summary>
    /// Sets the data object for this structure and populates the firing points, entrance and exit lists, and the waypoint link lookup
    /// </summary>
    public void SetData(Structure data)
    {
        StructureData = data;
        var ext = (data.Model.ExteriorModel.Contains('/')) ? (data.Model.ExteriorModel.Substring(data.Model.ExteriorModel.LastIndexOf('/') + 1)) : (data.Model.ExteriorModel);
        var intr = (data.Model.InteriorModel.Contains('/')) ? (data.Model.InteriorModel.Substring(data.Model.InteriorModel.LastIndexOf('/') + 1)) : (data.Model.InteriorModel);
        exteriorParent = transform.Find(ext);
        interiorParent = transform.Find(intr);

        if ((StructureData.Entrances != null && StructureData.Entrances.Length > 0) ||
            (StructureData.Exits != null && StructureData.Exits.Length > 0))
        {
            if (exteriorParent != null && interiorParent != null)
            {
                // set territory ownership for this building
                // only enterable buildings apply to territory control
                var hit = RaycastUtil.RaycastTerrain(transform.position + Vector3.up);
                TerritoryManager.Instance.AddStructureAssignment(this, hit.transform.gameObject);

                // add helper UI component
                HelperUI = gameObject.AddComponent<StructureHelperUI>();
                HelperUI.enabled = false;

                entrances = new List<Transform>();
                exits = new List<Transform>();
                linkLookup = new Dictionary<Transform, Transform>();
                units = new List<UnitManager>();

                exteriorRenderers = exteriorParent.GetComponentsInChildren<Renderer>();
                var extList = new List<Renderer>();
                foreach (var exteriorRenderer in exteriorRenderers)
                {
                    if (exteriorRenderer.GetComponent<ParticleSystem>() == null)
                    {
                        extList.Add(exteriorRenderer);
                    }
                }
                exteriorRenderers = extList.ToArray<Renderer>();
                interiorRenderers = interiorParent.GetComponentsInChildren<Renderer>();
                
                foreach (var entrance in StructureData.Entrances)
                {
                    var entTran = exteriorParent.Find(entrance.Point);
                    var extTran = interiorParent.Find(entrance.GoesTo);
                    if (entTran != null && extTran != null)
                    {
                        entrances.Add(entTran);
                        exits.Add(extTran);
                        linkLookup[entTran] = extTran;
                    }
                    else
                    {
                        throw new FracturedStateException("Bad Entrance or Exit reference for structure: \"" + StructureData.Name + "\"");
                    }
                }
                foreach (var exit in StructureData.Exits)
                {
                    var extTran = interiorParent.Find(exit.Point);
                    var entTran = exteriorParent.Find(exit.GoesTo);
                    if (entTran != null && extTran != null)
                    {
                        if (!entrances.Contains(entTran))
                            entrances.Add(entTran);

                        if (!exits.Contains(extTran))
                            exits.Add(extTran);

                        linkLookup[extTran] = entTran;
                    }
                    else
                    {
                        throw new FracturedStateException("Bad Entrance or Exit reference for structure: \"" + StructureData.Name + "\"");
                    }
                }

                foreach (var interiorRenderer in interiorRenderers)
                {
                    interiorRenderer.enabled = false;
                }
            }
            else
            {
                throw new FracturedStateException(StructureData.Name + " declares entrances and exits but does not have valid interior and/or exterior models set.");
            }
        }

        if (StructureData.FirePoints != null && StructureData.FirePoints.Length > 0)
        {
            firePoints = new List<Transform>();
            AvailableFirePoints = new List<Transform>();
            foreach (var firePoint in StructureData.FirePoints)
            {
                var fp = interiorParent.Find(firePoint.Name);
                if (fp != null)
                {
                    firePoints.Add(fp);
                }
                else
                {
                    throw new FracturedStateException(StructureData.Name + " references fire point " + firePoint.Name + " that doesn't exit");
                }
            }
            AvailableFirePoints.AddRange(firePoints);
        }

        if (StructureData.Garrisons != null && StructureData.Garrisons.Length > 0)
        {
            garrisonSquad = new GarrisonSquad(this);
            garrisonPoints = new List<GarrisonPointManager>();
            var tList = new List<Transform>();
            foreach (var g in StructureData.Garrisons)
            {
                foreach (var p in g.Points)
                {
                    var point = exteriorParent.Find(p.Point);
                    if (tList.Contains(point)) continue;
                    
                    tList.Add(point);
                    var mgmt = point.gameObject.AddComponent<GarrisonPointManager>();
                    mgmt.Init(this);
                    garrisonPoints.Add(mgmt);
                }
            }
        }
    }

    public void CheckFirePoints()
    {
        if (firePoints == null) return;
        
        for (var i = firePoints.Count - 1; i >= 0; i--)
        {
            var ray = new Ray(firePoints[i].position + Vector3.up * 100, -Vector3.up);
            if (RaycastUtil.RayCheckInterior(ray))
            {
                AvailableFirePoints.Remove(firePoints[i]);
                firePoints.RemoveAt(i);
            }
        }
    }

    public UnitManager GetUnitForFirePoint(Transform firePoint)
    {
        if (firePoints == null) return null;

        foreach (var kvp in occupyingUnits)
        {
            foreach (var unit in kvp.Value)
            {
                if (unit.CurrentFirePoint == firePoint)
                {
                    return unit;
                }
            }
        }

        return null;
    }

    public void SetShroudMask(Material shroudMaterial, Color shroudColor)
    {
        buildingShroudColor = shroudColor;
        if (exteriorParent == null) return;
        
        var shroudTran = Instantiate(exteriorParent, transform.position, exteriorParent.rotation);
        var probes = shroudTran.GetComponentsInChildren<ReflectionProbe>();
        foreach (var probe in probes)
        {
            Destroy(probe.gameObject);
        }
        shroudRenderers = shroudTran.GetComponentsInChildren<MeshRenderer>();
        foreach (var r in shroudRenderers)
        {
            r.material = shroudMaterial;
            r.material.color = shroudColor;
            r.shadowCastingMode = ShadowCastingMode.Off;
            r.receiveShadows = false;
        }
        var particles = shroudTran.GetComponentsInChildren<ParticleSystem>();
        foreach (var particle in particles)
        {
            Destroy(particle);
        }
        var cols = shroudTran.GetComponentsInChildren<Collider>();
        foreach (var col in cols)
        {
            Destroy(col.gameObject);
        }
        shroudTran.gameObject.SetLayerRecursively(1);
        shroudTran.tag = GameConstants.BuildingShroudTag;
    }

    public void AddVisible()
    {
        if (visibleTracker == 0)
        {
            foreach (var shroud in shroudRenderers)
            {
                shroud.material.color = Color.white;
            }
        }
        visibleTracker++;
    }

    public void RemoveVisible()
    {
        if (visibleTracker > 0)
        {
            visibleTracker--;
            if (visibleTracker == 0)
            {
                foreach (var shroud in shroudRenderers)
                {
                    shroud.material.color = buildingShroudColor;
                }
            }
        }
    }

    public Transform GetParticleBone(string pointName)
    {
        foreach (var firePoint in StructureData.FirePoints)
        {
            if (firePoint.Name == pointName)
            {
                return transform.GetChildByName(firePoint.ParticleBone);
            }
        }

        return null;
    }

    /// <summary>
    /// Returns true if the given point is contained within the exterior collider bounds of this building
    /// </summary>
    public bool ContainsPoint(Vector3 point)
    {
        var cols = exteriorParent.GetComponentsInChildren<Collider>();
        foreach (var col in cols)
        {
            if (col.bounds.Contains(point))
                return true;
        }
        return false;
    }

    public void ContributePoints(UnitManager unit)
    {
        // if this building is already captured then don't add points
        if (CurrentPoints >= StructureData.CapturePoints)
            return;

        // if more than one team is in this building don't add points
        if (occupyingUnits.Keys.Count > 1)
            return;

        // if this unit technically isn't synced to this building yet then don't add points
        if (!occupyingUnits.ContainsKey(unit.OwnerTeam))
        {
            return;
        }

        var occupying = occupyingUnits[unit.OwnerTeam];
        if (!occupying.Contains(unit))
        {
            return;
        }

        // if one team is in this building but the owning team isn't this unit's team then
        // possession has changed and we need to reset to 0 and restart capture process
        if (OwnerTeam == null || (OwnerTeam != unit.OwnerTeam && OwnerTeam.Side != unit.OwnerTeam.Side))
        {
            if (OwnerTeam == null)
            {
                OwnerTeam = unit.OwnerTeam;
                
                // this is here to cover instances where a player relinquishes a building but still has only their
                // units inside it - effectively capping it again
                if (OwnerTeam == FracNet.Instance.LocalTeam)
                {
                    var cp = GetComponent<CaptureProgressUI>();
                    cp.enabled = true;
                    cp.SetColor(OwnerTeam.TeamColor.UnityColor);
                }
            }
            else
            {
                var realOwner = TerritoryManager.Instance.GetStructureOwner(this);
                if (realOwner != unit.OwnerTeam)
                {
                    CurrentPoints = 0;
                    OwnerTeam = unit.OwnerTeam;
                }
                // if the owner came back into a building that started to cap but didn't finish
                else
                {
                    CurrentPoints = StructureData.CapturePoints;
                    OwnerTeam = realOwner;
                }
            }
        }
        
        if (unit.IsMine || unit.AISimulate || unit.IsFriendly)
        {
            CurrentPoints += unit.Data.CapturePoints * Time.deltaTime;
            if (CurrentPoints >= StructureData.CapturePoints && (unit.IsMine || unit.AISimulate))
            {
                unit.NetMsg.CmdCaptureStructure(GetComponent<Identity>().UID);
            }
        }
    }

    public void SpawnGarrison(Team team)
    {
        if (garrisonPoints != null)
        {
            for (var u = 0; u < garrisonPoints.Count; u++)
            {
                garrisonPoints[u].KillUnit();
            }
        }

        if (team == null) return;
        
        var garrison = GetGarrisonInfo(team.Faction);
        if (garrison == null) return;
        
        for (var u = 0; u < garrisonPoints.Count; u++)
        {
            foreach (var p in garrison.Points)
            {
                if (p.Point == garrisonPoints[u].transform.name)
                {
                    garrisonPoints[u].SpawnUnit(p);
                }
            }
        }
    }

    public void RespawnGarrisonUnit(UnitManager unit)
    {
        for (var i = 0; i < garrisonPoints.Count; i++)
        {
            if (garrisonPoints[i].Unit == unit)
            {
                garrisonPoints[i].RespawnUnit();
            }
        }
    }

    private GarrisonInfo GetGarrisonInfo(string faction)
    {
        if (StructureData.Garrisons != null && StructureData.Garrisons.Length > 0)
        {
            foreach (var g in StructureData.Garrisons)
            {
                if (g.Faction == faction)
                {
                    return g;
                }
            }
        }
        return null;
    }

    public GarrisonPointManager GetGarrisonPoint(string pointName)
    {
        if (garrisonPoints == null) return null;
        
        foreach (var garrison in garrisonPoints)
        {
            if (garrison.name == pointName)
            {
                return garrison;
            }
        }
        return null;
    }

    public void Reset()
    {
        if (StructureData.CanBeCaptured)
        {
            CurrentPoints = 0;
        }
        OwnerTeam = TerritoryManager.Instance.GetStructureOwner(this);
    }

    private void RegisterVisibility(UnitManager enteringUnit)
    {
        // if our unit enters then register every enemy unit in the structure as visible to him
        if (enteringUnit.IsMine || enteringUnit.AISimulate || enteringUnit.IsFriendly)
        {
            using (var teams = occupyingUnits.Keys.GetEnumerator())
            {
                while (teams.MoveNext())
                {
                    if (teams.Current == null) continue;
                    if (teams.Current == enteringUnit.OwnerTeam) continue;
                    if (teams.Current.Side == enteringUnit.OwnerTeam.Side) continue;
                    
                    var teamUnits = occupyingUnits[teams.Current];
                    for (var i = 0; i < teamUnits.Count; i++)
                    {
                        if (teamUnits[i] != null)
                        {
                            enteringUnit.Squad.RegisterVisibleUnit(enteringUnit, teamUnits[i]);
                        }
                    }
                }
            }
        }
        
        // hosts need to register entering units with AI units
        if (FracNet.Instance.IsHost)
        {
            var aiTeams = occupyingUnits.Keys.Where(t => !t.IsHuman);
            foreach (var team in aiTeams)
            {
                if (enteringUnit.OwnerTeam != team && enteringUnit.OwnerTeam.Side != team.Side)
                {
                    var oUnits = occupyingUnits[team];
                    foreach (var unit in oUnits)
                    {
                        if (unit == null) continue;
                        
                        unit.Squad.RegisterVisibleUnit(unit, enteringUnit);
                    }
                }
            }
        }
        
        // if an enemy unit enters then register him with all of our units
        if (!enteringUnit.IsMine)
        {
            for (var i = 0; i < units.Count; i++)
            {
                if (units[i] != null)
                {
                    units[i].Squad.RegisterVisibleUnit(units[i], enteringUnit);
                }
            }
        }
    }

    private void UnRegisterVisibility(UnitManager leavingUnit)
    {
        // if our unit exits the structure then unregister every enemy unit in the structure
        if (leavingUnit.IsMine)
        {
            using (var teams = occupyingUnits.Keys.GetEnumerator())
            {
                while (teams.MoveNext())
                {
                    if (teams.Current == null || teams.Current == leavingUnit.OwnerTeam) continue;
                    
                    var teamUnits = occupyingUnits[teams.Current];
                    for (var i = 0; i < teamUnits.Count; i++)
                    {
                        if (teamUnits[i] != null)
                        {
                            leavingUnit.Squad?.UnregisterVisibleUnit(leavingUnit, teamUnits[i]);
                        }
                    }
                }
            }
        }
        // if an enemy exits the structure then unregister him with all of our units
        else
        {
            for (var i = 0; i < units.Count; i++)
            {
                if (units[i] != null)
                {
                    units[i].Squad?.UnregisterVisibleUnit(units[i], leavingUnit);
                }
            }
        }
    }

    public void Occupy(UnitManager unit)
    {
        List<UnitManager> teamUnits;
        var team = unit.OwnerTeam;
        if (occupyingUnits.TryGetValue(team, out teamUnits))
        {
            teamUnits.Add(unit);
        }
        else
        {
            occupyingUnits[team] = new List<UnitManager>() { unit };
        }
        
        if (occupyingUnits.Keys.Count == 1 && (OwnerTeam == null || (OwnerTeam != team && OwnerTeam.Side != team.Side)))
        {
            if (StructureData.CanBeCaptured)
            {
                Reset();
            }
        }

        if (unit.IsMine || SkirmishVictoryManager.IsSpectating || !FracNet.Instance.LocalTeam.IsActive || unit.IsFriendly)
        {
            if (unit.IsMine)
            {
                unit.GetComponent<AudioSource>().PlayOneShot(InterfaceSoundPlayer.BuildingEnter);
            }
            if (units.Count == 0)
            {
                Toggle();
            }
            units.Add(unit);
        }

        RegisterVisibility(unit);
    }

    public void Leave(UnitManager unit)
    {
        var team = unit.OwnerTeam;
        List<UnitManager> teamUnits;
        if (occupyingUnits.TryGetValue(team, out teamUnits))
        {
            teamUnits.Remove(unit);
            if (teamUnits.Count == 0)
            {
                occupyingUnits.Remove(team);
                // process ownership of non tech buildings here because there is no points contribution
                if (FracNet.Instance.IsHost && !StructureData.CanBeCaptured)
                {
                    if (occupyingUnits.Keys.Count == 1 && team == OwnerTeam)
                    {
                        // route this through FracLAN instead to avoid dead units not firing their release methods
                        FracNet.Instance.ReleaseStructure(GetComponent<Identity>().UID, unit.OwnerTeam);
                        var ownerUnits = occupyingUnits[occupyingUnits.Keys.First()];
                        ownerUnits[0].NetMsg.CmdCaptureStructure(GetComponent<Identity>().UID);
                    }
                    else if (team == OwnerTeam)
                    {
                        // route this through FracLAN instead to avoid dead units not firing their release methods
                        FracNet.Instance.ReleaseStructure(GetComponent<Identity>().UID, unit.OwnerTeam);
                    }
                }

                if (StructureData.CanBeCaptured)
                {
                    if (occupyingUnits.Keys.Count == 1)
                    {
                        var ownerUnits = occupyingUnits[occupyingUnits.Keys.First()];
                        if (OwnerTeam == null || (OwnerTeam != ownerUnits[0].OwnerTeam && OwnerTeam.Side != ownerUnits[0].OwnerTeam.Side))
                        {
                            if (ownerUnits[0].IsMine || ownerUnits[0].IsFriendly)
                            {
                                if (TerritoryManager.Instance.GetStructureOwner(this) != ownerUnits[0].OwnerTeam)
                                {
                                    var cp = GetComponent<CaptureProgressUI>();
                                    cp.enabled = true;
                                    cp.SetColor(ownerUnits[0].OwnerTeam.TeamColor.UnityColor);
                                    CurrentPoints = 0;
                                }
                                else
                                {
                                    CurrentPoints = StructureData.CapturePoints;
                                }
                            }
                            else
                            {
                                OwnerTeam = ownerUnits[0].OwnerTeam;
                                CurrentPoints = 0;
                            }
                        }
                    }
                    else if (occupyingUnits.Keys.Count == 0 && CurrentPoints < StructureData.CapturePoints)
                    {
                        Reset();
                    }
                }
            }
        }

        if (unit.IsMine || SkirmishVictoryManager.IsSpectating || !FracNet.Instance.LocalTeam.IsActive || unit.IsFriendly)
        {
            units.Remove(unit);
            if (units.Count == 0)
            {
                Toggle();
            }
            if (unit.IsAlive && unit.IsMine)
                unit.GetComponent<AudioSource>().PlayOneShot(InterfaceSoundPlayer.BuildingEnter);
        }

        UnRegisterVisibility(unit);
    }

    public void ToggleExterior(bool renderEnabled)
    {
        ToggleRenderers(exteriorRenderers, renderEnabled);
    }

    public void ToggleInterior(bool renderEnabled)
    {
        ToggleRenderers(interiorRenderers, renderEnabled);
    }

    private static void ToggleRenderers(Renderer[] renders, bool enabled)
    {
        if (renders == null) return;
        
        foreach (var r in renders)
        {
            r.enabled = enabled;
        }
    }

    private void Toggle()
    {
        var newShader = (IsFriendlyOccupied) ? unoccupiedShader : occupiedShader;
        foreach (var exterior in exteriorRenderers)
        {
            exterior.material.shader = newShader;
        }
        
        foreach (var interior in interiorRenderers)
        {
            interior.enabled = !interior.enabled;
        }
        
        for (var i = 0; i < ContainedProps.Count; i++)
        {
            ContainedProps[i].SetActive(!IsFriendlyOccupied);
        }

        for (var i = 0; i < exteriorDecals.Count; i++)
        {
            exteriorDecals[i].gameObject.SetActive(IsFriendlyOccupied);
        }

        for (var i = 0; i < interiorDecals.Count; i++)
        {
            interiorDecals[i].gameObject.SetActive(!IsFriendlyOccupied);
        }
        
        IsFriendlyOccupied = !IsFriendlyOccupied;
        if (IsFriendlyOccupied && StructureData.CanBeCaptured && occupyingUnits.Keys.Count == 1)
        {
            var ownerUnits = occupyingUnits[occupyingUnits.Keys.First()];
            var unit = ownerUnits[0];
            if ((unit.IsMine || unit.IsFriendly) && OwnerTeam != unit.OwnerTeam)
            {
                OwnerTeam = TerritoryManager.Instance.GetStructureOwner(this);
                if (OwnerTeam == null || (OwnerTeam != unit.OwnerTeam && OwnerTeam.Side != unit.OwnerTeam.Side))
                {
                    var cp = GetComponent<CaptureProgressUI>();
                    cp.enabled = true;
                    cp.SetColor(unit.OwnerTeam.TeamColor.UnityColor);
                    Reset();
                }
                else
                {
                    CurrentPoints = StructureData.CapturePoints;
                }
            }
        }
        else if (!IsFriendlyOccupied && StructureData.CanBeCaptured)
        {
            GetComponent<CaptureProgressUI>().enabled = false;
        }
    }

    public void AddDecal(Decal decal)
    {
        if (decal.LimitTo == null) return;

        if (decal.LimitTo.transform == exteriorParent)
        {
            exteriorDecals.Add(decal);
        }
        else if (decal.LimitTo.transform == interiorParent)
        {
            interiorDecals.Add(decal);
            decal.gameObject.SetActive(false);
        }
    }
    
    public void AddProp(GameObject prop, CoverManager cm)
    {
        ContainedProps.Add(prop);
        if (cm != null)
        {
            cm.CheckInteriorPoints(this);
        }
    }

    public void AddGarrisonUnit(UnitManager unit)
    {
        garrisonSquad.AddSquadUnit(unit);
    }
}