using UnityEngine;
using System.Collections.Generic;
using FracturedState;
using FracturedState.Game;
using UnityEngine.Events;

public class SelectionManager : MonoBehaviour
{
	public static SelectionManager Instance { get; private set ; }

	private const float MinMouseDragDistance = 4f;
	
    [SerializeField()]
    private MicroManager microManager;
    [SerializeField()]
    private Texture2D selectTexture;

    [SerializeField()]
    private CompassMenuManager compass;
    [SerializeField()]
    private CompassUI compassUI;

    [SerializeField]
    private CommandManager commandManager;

    private Rect selectionRect;
	private Vector3 mouseDragStart;
    public bool IsDragSelecting { get; private set; }
    public List<UnitManager> SelectedUnits { get; private set; }

    private List<UnitManager> availableUnits = new List<UnitManager>();
    private HashSet<Squad> selectedSquads = new HashSet<Squad>();

    public int SquadCount => selectedSquads.Count;

    public UnityEvent OnSelectionChanged;

    public void Reset()
    {
        availableUnits = new List<UnitManager>();
        selectedSquads = new HashSet<Squad>();
    }

    public void RecalculateSquadSelection()
    {
        selectedSquads.Clear();
        foreach (var unit in SelectedUnits)
        {
            selectedSquads.Add(unit.Squad);
        }
    }

    public void AddUnit(UnitManager unit)
    {
        if (!SelectedUnits.Contains(unit))
        {
            SelectedUnits.Add(unit);
            selectedSquads.Add(unit.Squad);
        }

        OnSelectionChanged?.Invoke();
    }
    
    public void RemoveUnit(UnitManager unit)
    {
        if (SelectedUnits.Remove(unit))
        {
            selectedSquads.Clear();
            foreach (var u in SelectedUnits)
            {
                selectedSquads.Add(u.Squad);
            }
        }

        OnSelectionChanged?.Invoke();
    }

    public void ClearSelection()
    {
        while (SelectedUnits.Count > 0)
        {
            SelectedUnits[0].OnDeSelected(true);
        }
        microManager.ClearActiveAbility();
    }

    public void RegisterUnit(UnitManager unit)
    {
        availableUnits.Add(unit);
    }

    public void UnregisterUnit(UnitManager unit)
    {
        availableUnits.Remove(unit);
    }

    private void Awake()
	{
		Instance = this;
        Instance.SelectedUnits = new List<UnitManager>();
	}

    private void OnGUI()
    {
        if (IsDragSelecting)
        {
            GUI.DrawTexture(selectionRect, selectTexture);
        }
    }

    private void Update()
	{
        if (commandManager.EscapeMenuOpen)
            return;

        if (microManager.InMicro)
            return;

	    if (SkillManager.ActiveSKill != null)
	        return;

        if (!IsDragSelecting && (RaycastUtil.IsMouseInUI() || compass.CompassRect.Contains(Input.mousePosition) || (compassUI.RecruitPanel && compassUI.RecruitArea.Contains(Input.mousePosition))))
            return;

		if (Input.GetMouseButtonDown(0))
		{
			mouseDragStart = Input.mousePosition;
			return;
		}
		
		if (Input.GetMouseButtonUp(0))
		{
			if (IsDragSelecting)
			{
                if (!Input.GetKey(KeyCode.LeftShift))
                {
                    ClearSelection();
                }
				
				foreach (var unit in availableUnits)
                {
                    if (unit != null && unit.transform != null)
                    {
                        if (unit.Transport == null && unit.Data.IsSelectable && !SelectedUnits.Contains(unit))
                        {
                            var pos = Camera.main.WorldToScreenPoint(unit.transform.position);
                            if (selectionRect.Contains(new Vector2(pos.x, Screen.height - pos.y)))
                            {
                                unit.OnSelected(true);
                            }
                        }
                    }
                }
				
                IsDragSelecting = false;
                selectionRect = new Rect();
			}
			else if (!MicroManager.CursorInHotbars())
			{
                if (!microManager.AbilityActive)
                {
                    var uMan = RaycastUtil.RaycastFriendlyAtMouse();
                    if (uMan != null && uMan.IsMine && uMan.Data.IsSelectable)
                    {
                        if (!Input.GetKey(KeyCode.LeftShift))
                        {
                            ClearSelection();
                        }
                        uMan.OnSelected(true);
                    }
                    else if (uMan == null)
                    {
                        ClearSelection();
                    }
                }
			}
			return;
		}

        if (Input.GetMouseButton(0) && !(compass.CompassRect.Contains(Input.mousePosition) || (compassUI.RecruitPanel && compassUI.RecruitArea.Contains(Input.mousePosition))))
		{
			if (!IsDragSelecting)
			{
				IsDragSelecting = (mouseDragStart - Input.mousePosition).magnitude > MinMouseDragDistance;
			}
			else
			{
                var curMousePos = Input.mousePosition;
                var width = Mathf.Abs(curMousePos.x - mouseDragStart.x);
                var height = Mathf.Abs(curMousePos.y - mouseDragStart.y);

                // left to right
                if (mouseDragStart.x < curMousePos.x)
                {
                    // left to right, top to bottom
                    if (mouseDragStart.y > curMousePos.y)
                    {
                        selectionRect = new Rect(mouseDragStart.x, Screen.height - mouseDragStart.y, width, height);
                    }
                    // left to right, bottom to top
                    else
                    {
                        selectionRect = new Rect(mouseDragStart.x, Screen.height - curMousePos.y, width, height);
                    }
                }
                // right to left
                else
                {
                    // right to left, top to bottom
                    if (mouseDragStart.y > curMousePos.y)
                    {
                        selectionRect = new Rect(curMousePos.x, Screen.height - mouseDragStart.y, width, height);
                    }
                    // right to left, bottom to top
                    else
                    {
                        selectionRect = new Rect(curMousePos.x, Screen.height - curMousePos.y, width, height);
                    }
                }
			}
		}
	}
}