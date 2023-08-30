using System.Collections.Generic;
using FracturedState.Game;
using FracturedState.Game.Data;
using FracturedState.Game.State;
using FracturedState.ModTools;
using ThreeEyedGames;
using UnityEngine;

public delegate void RightClickActionDelegate();

/// <summary>
/// The management class for the Map Editor module.  NOTE: Any Tool that declares this manager as its owner MUST explicitly set RightClickAction.
/// </summary>
public class MapEditorToolManager : MenuSuppressedManager
{
    private const string PropSelection = "PropSelection";

    public GameObject SelectedObject;
    public Camera MapCamera;
    public LayerMask ObjMask;
    public LayerMask TerrainMask;

    [SerializeField]
    private GizmoControllerCS gizmo;

    [SerializeField] private Color previewBackgroundColor;
    public Color PreviewBackgroundColor => previewBackgroundColor;
    
	private Vector3 clickPosition;
	private EditorCameraController camControl;

    private Transform selectedParent;

    public RightClickActionDelegate RightClickAction;

    private GameObject boundsDrawer;
    private ReflectionProbe terrainProbe;

    public int XUpperBound { get; private set; }
    public int XLowerBound { get; private set; }
    public int ZUpperBound { get; private set; }
    public int ZLowerBound { get; private set; }

    public List<TerritoryData> Territories { get; set; }
    private Dictionary<GameObject, TerritoryData> territoryAssignments;
    private Dictionary<GameObject, GameObject> territoryHelpers;

    private Transform mapParent;

    public override bool CursorInMenu()
    {
        return base.CursorInMenu() || InSelectWindow();
    }

    private void Start()
    {
        Territories = new List<TerritoryData>();
        territoryAssignments = new Dictionary<GameObject, TerritoryData>();
        territoryHelpers = new Dictionary<GameObject, GameObject>();
        gizmo.SetControlWinPosition(new Vector2(0, 57));
        mapParent = GameObject.Find("MapParent").transform;
        SetMapBounds(25, -25, 25, -25);
    }

    private void Update()
	{	
		if (Input.GetMouseButtonUp(1) && !camControl.CameraCanMove)
		{
			RightClickAction?.Invoke();
		}

        if (Input.GetMouseButton(0) && !CursorInMenu() && !gizmo.IsOverAxis())
        {
            stateMachine.ExecuteMouseDown();
        }
        else if (Input.GetMouseButtonUp(0) && !CursorInMenu() && !gizmo.IsOverAxis())
        {
            stateMachine.ExecuteState();
        }

        if (Input.GetKeyDown(KeyCode.Delete) && SelectedObject != null)
        {
            if (SelectedObject.layer == GameConstants.TerrainLayer)
            {
                RemoveTerritoryAssignment(SelectedObject);
            }
            Destroy(SelectedObject);
            Unselect();
        }
	}
	
    protected override void Init()
    {
        base.Init();
        stateMachine = new ToolStateMachine<BaseTool>(new SelectTool(this));
		camControl = MapCamera.GetComponent<EditorCameraController>();
    }

    public override void DrawToolBar()
    {
		if (GUILayout.Button("New"))
			stateMachine.ChangeState(new NewMapTool(this));

        if (GUILayout.Button("Open"))
            stateMachine.ChangeState(new OpenMapTool(this));

        if (GUILayout.Button("Publish"))
            stateMachine.ChangeState(new PublishMapTool(this));
		
        if (GUILayout.Button("Terrain"))
            stateMachine.ChangeState(new TerrainTool(this));

        if (GUILayout.Button("Territory"))
            stateMachine.ChangeState(new TerritoryTool(this));

        if (GUILayout.Button("Objects"))
            stateMachine.ChangeState(new PlaceObjectTool(this));
        
        if (GUILayout.Button("Decals"))
            stateMachine.ChangeState(new PlaceDecalTool(this));
    }

    public void DuplicateSelected(float x, float z)
    {
        if (SelectedObject != null)
        {
            var offset = new Vector3(x, 0, z);
            var dupe = Instantiate(SelectedObject, SelectedObject.transform.position + offset, SelectedObject.transform.rotation);
            dupe.name = SelectedObject.name;
            dupe.transform.parent = selectedParent;
            SetSelectedObject(dupe.transform);
        }
    }

    public void SelectDecal(Decal decal)
    {
        if (SelectedObject != null)
        {
            Unselect();
        }

        SelectedObject = decal.gameObject;
        gizmo.SetSelectedObject(decal.transform);
        gizmo.SetSnapping(false);
        gizmo.SetAllowedModes(true, true, true);
        if (gizmo.IsHidden())
            gizmo.Show(GizmoControllerCS.GIZMO_MODE.TRANSLATE);
    }
    
    public void SetSelectedObject(Transform selected)
    {
        if (SelectedObject != null)
        {
            Unselect();
        }
        selectedParent = selected.parent;
        // if the user selects a structure then also include any props inside that structure
        if (selectedParent != null && selectedParent.name == "Structures")
        {
            var includeProps = new List<Transform>();
            var propParent = mapParent.Find("Props");
            foreach (Transform prop in propParent)
            {
                if (prop.gameObject.layer == GameConstants.InteriorLayer)
                {
                    var ray = new Ray(prop.position + Vector3.up * 100, Vector3.down);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, 101, ObjMask))
                    {
                        if (hit.transform.GetAbsoluteParent() == selected)
                        {
                            includeProps.Add(prop);
                        }
                    }
                }
            }
            if (includeProps.Count > 0)
            {
                var tp = selected.Find(PropSelection);
                var p = tp == null ? new GameObject(PropSelection) : tp.gameObject;
                p.transform.position = selected.position;
                p.transform.rotation = selected.rotation;
                p.transform.parent = selected;
                for (var i = 0; i < includeProps.Count; i++)
                {
                    includeProps[i].parent = p.transform;
                }
            }
        }
        selected.parent = null;
        gizmo.SetSelectedObject(selected);
        SelectedObject = selected.gameObject;
        gizmo.SetAllowedModes(true, true, false);
        if (gizmo.IsHidden())
        {
            gizmo.Show(GizmoControllerCS.GIZMO_MODE.TRANSLATE);
        }
        gizmo.SetSnapping(selected.gameObject.layer == GameConstants.TerrainLayer);
    }

    public void Unselect()
    {
        if (gizmo.IsHidden()) return;
        
        gizmo.Hide();
        var p = SelectedObject.transform.Find(PropSelection);
        if (p != null)
        {
            var props = new List<Transform>();
            var i = 0;
            while (i < p.childCount)
            {
                props.Add(p.GetChild(i++));
            }
            var propParent = mapParent.Find("Props");
            foreach (var prop in props)
            {
                prop.parent = propParent;
            }
            Destroy(p.gameObject);
        }
        if (selectedParent != null)
        {
            SelectedObject.transform.parent = selectedParent;
            selectedParent = null;
        }
        SelectedObject = null;
    }

    public bool InSelectWindow()
    {
        if (gizmo.IsHidden())
            return false;

        var mousePos = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
        return gizmo.ControlRect.Contains(mousePos) || gizmo.DupeRect.Contains(mousePos);
    }

    public void SetMapBounds(int xUpper, int xLower, int zUpper, int zLower)
    {
        XUpperBound = xUpper;
        XLowerBound = xLower;
        ZUpperBound = zUpper;
        ZLowerBound = zLower;
        if (boundsDrawer != null)
        {
            DestroyImmediate(boundsDrawer);
        }

        boundsDrawer = new GameObject("BoundsDrawer");
        var line = boundsDrawer.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.material = new Material(Shader.Find("Particles/Additive"));
        line.positionCount = 5;
        line.SetPosition(0, new Vector3(XLowerBound, 0, ZLowerBound));
        line.SetPosition(1, new Vector3(XLowerBound, 0, ZUpperBound));
        line.SetPosition(2, new Vector3(XUpperBound, 0, ZUpperBound));
        line.SetPosition(3, new Vector3(XUpperBound, 0, ZLowerBound));
        line.SetPosition(4, new Vector3(XLowerBound, 0, ZLowerBound));
        line.startColor = Color.green;
        line.endColor = Color.green;
        boundsDrawer.layer = GameConstants.ExteriorLayer;

        if (terrainProbe != null)
        {
            DestroyImmediate(terrainProbe);
        }
        terrainProbe = Loader.BuildTerrainProbe(xUpper, xLower, zUpper, zLower);
        terrainProbe.RenderProbe();
    }

    public void SetBoundshelperActive(bool active)
    {
        if (boundsDrawer == null) return;
        
        boundsDrawer.SetActive(active);
    }
    
    public void ToggleTerritoryHelpers(bool on)
    {
        foreach (var t in territoryHelpers.Values)
        {
            t.SetActive(on);
        }
    }

    public void AssignTerritory(GameObject terrain, TerritoryData territory)
    {
        territoryAssignments[terrain] = territory;
        if (territoryHelpers.ContainsKey(terrain))
        {
            territoryHelpers[terrain].GetComponent<Renderer>().material.color = territory.UnityColor;
        }
        else
        {
            var helper = Instantiate(terrain, terrain.transform.position, terrain.transform.rotation);
            helper.transform.position += Vector3.up * 0.1f;
            Destroy(helper.GetComponent<Collider>());
            var m = new Material(Shader.Find("Diffuse")) {color = territory.UnityColor};
            helper.GetComponent<Renderer>().material = m;
            territoryHelpers[terrain] = helper;
        }
    }

    public void RemoveTerritoryAssignment(GameObject terrain)
    {
        GameObject helper;
        if (territoryHelpers.TryGetValue(terrain, out helper))
        {
            Destroy(helper);
            territoryHelpers.Remove(terrain);
        }
        if (territoryAssignments.ContainsKey(terrain))
        {
            territoryAssignments.Remove(terrain);
        }
    }

    public void RemoveTerritoryDefinition(TerritoryData territory)
    {
        var removeTerrains = new List<GameObject>();
        var terrainKeys = territoryAssignments.Keys;
        foreach (var key in terrainKeys)
        {
            if (territory == territoryAssignments[key])
            {
                removeTerrains.Add(key);
            }
        }
        foreach (var terrain in removeTerrains)
        {
            RemoveTerritoryAssignment(terrain);
        }
        Territories.Remove(territory);
    }

    public void UpdateTerritoryData(TerritoryData territory)
    {
        var keys = territoryHelpers.Keys;
        foreach (var key in keys)
        {
            if (territoryAssignments[key] == territory)
            {
                territoryHelpers[key].GetComponent<Renderer>().material.color = territory.UnityColor;
            }
        }
    }

    public int GetTerritoryIndex(GameObject terrain)
    {
        TerritoryData t;
        if (territoryAssignments.TryGetValue(terrain, out t))
        {
            return Territories.IndexOf(t);
        }
        return -1;
    }

    /// <summary>
    /// Removes everything associated with the current map and releases the resources used by those objects
    /// </summary>
    public void ClearCurrentMap()
	{
        var worldParent = GameObject.Find("MapParent");
        var terrainParent = worldParent.transform.Find("Terrain");
        var structParent = worldParent.transform.Find("Structures");
        var propParent = worldParent.transform.Find("Props");
        foreach (Transform t in terrainParent)
        {
            if (t != terrainParent)
            {
                Destroy(t.gameObject);
            }
        }
        foreach (Transform t in structParent)
        {
            if (t != structParent)
            {
                Destroy(t.gameObject);
            }
        }
        foreach (Transform t in propParent)
        {
            if (t != propParent)
            {
                Destroy(t.gameObject);
            }
        }
        foreach (var h in territoryHelpers.Values)
        {
            Destroy(h);
        }

	    var decals = FindObjectsOfType<Decal>();
	    if (decals != null)
	    {
	        foreach (var decal in decals)
	        {
	            Destroy(decal.gameObject);
	        }
	    }
        territoryHelpers.Clear();
        while (GameObject.Find("mapStartingPoint(Clone)") != null)
        {
            DestroyImmediate(GameObject.Find("mapStartingPoint(Clone)"));
        }
        Resources.UnloadUnusedAssets();
        SetMapBounds(25, -25, 25, -25);
	}
}