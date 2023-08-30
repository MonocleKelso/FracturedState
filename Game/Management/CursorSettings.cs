using System.Collections;
using FracturedState;
using FracturedState.Game;
using FracturedState.Game.Data;
using FracturedState.Game.Management;
using FracturedState.Game.Nav;
using UnityEngine;

public class CursorSettings : MonoBehaviour
{
    #region Hotspots
    private static readonly Vector2 TopLeft = Vector2.zero;
    private static readonly Vector2 BottomMid = new Vector2(16, 32);
    private static readonly Vector2 MidMid = new Vector2(16, 16);
    #endregion

    #region Cursor definitions
    [SerializeField]
    private Texture2D defaultCursor;
    [SerializeField]
    private Texture2D selectCursor;
    [SerializeField]
    private Texture2D moveCursor;
    [SerializeField]
    private Texture2D enterCursor;
    [SerializeField]
    private Texture2D noEnterCursor;
    [SerializeField]
    private Texture2D attackCursor;
    [SerializeField]
    private Texture2D noAttackCursor;
    [SerializeField]
    private Texture2D blankCursor;
    #endregion

    private Texture2D currentCursor = null;

    [SerializeField]
    private CompassMenuManager compass;
    [SerializeField]
    private CompassUI compassUI;
    [SerializeField]
    private MicroManager microManager;
    [SerializeField]
    private CommandManager commandManager;

    public static CursorSettings Instance { get; private set; }

    private Ray _ray;
    private RaycastHit _hit;
    private Coroutine _cursorCheck;

    public void Awake()
    {
        if (Instance != null)
            throw new FracturedStateException("Multiple CursorSettings instances are not allowed");

        Instance = this;
    }

    public IEnumerator Start()
    {
        yield return new WaitForSeconds(0.2f);  // Unity bug - need to wait some time before assigning cursor
        SetPointerCursor();
    }

    public void StartCursorCheck()
    {
        _cursorCheck = StartCoroutine(CursorCoroutine());
    }

    public void StopCursorCheck()
    {
        StopCoroutine(_cursorCheck);
    }

    private IEnumerator CursorCoroutine()
    {
        yield return null;
        while (true)
        {
            CheckCursor();
            yield return null;
        }
    }

    private void CheckCursor()
    {
        // if escape menu is open then only use pointer
        if (commandManager.EscapeMenuOpen)
        {
            SetPointerCursor();
            return;
        }

        // if an ability is active then use it to determine cursor
        if (SkillManager.ActiveSKill != null)
        {
            var type = SkillManager.ActiveSKill.Targetting;
            if (type == TargetType.Ground)
            {
                SetBlankCursor();
            }
            else if (type == TargetType.Enemy)
            {
                if (RaycastUtil.RaycastEnemyAtMouse() != null)
                {
                    SetAttackCursor();
                }
                else
                {
                    SetNoAttackCursor();
                }
            }
            else if (type == TargetType.Friendly)
            {
                if (RaycastUtil.RaycastFriendlyAtMouse() != null)
                {
                    SetAttackCursor();
                }
                else
                {
                    SetNoAttackCursor();
                }
            }
            else if (type == TargetType.Structure)
            {
                _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                
                if (Physics.Raycast(_ray, out _hit, Mathf.Infinity, GameConstants.ExteriorMask))
                {
                    var structure = _hit.transform.GetAbsoluteParent().GetComponent<StructureManager>();
                    if (structure != null)
                    {
                        SetAttackCursor();
                    }
                    else
                    {
                        SetNoAttackCursor();
                    }
                }
                else
                {
                    SetNoAttackCursor();
                }
            }
            else
            {
                SetPointerCursor();
            }
            return;
        }

        // if we're in any UI element
        if (RaycastUtil.IsMouseInUI())
        {
            SetPointerCursor();
            return;
        }
        
        // if we are in the compass then assign pointer
        if (compass.enabled)
        {
            if (compass.CompassRect.Contains(Input.mousePosition))
            {
                SetPointerCursor();
                return;
            }
            if (compassUI.RecruitPanel && compassUI.RecruitArea.Contains(Input.mousePosition))
            {
                SetPointerCursor();
                return;
            }
        }

        // check friendly unit selection first
        var unit = RaycastUtil.RaycastFriendlyAtMouse();
        if (unit != null && unit.IsMine)
        {
            if (unit.Data.IsTransport && SelectionManager.Instance.SelectedUnits.Count > 0)
            {
                for (var i = 0; i < SelectionManager.Instance.SelectedUnits.Count; i++)
                {
                    if (SelectionManager.Instance.SelectedUnits[i] != null &&
                        !SelectionManager.Instance.SelectedUnits[i].Data.CanBePassenger)
                    {
                        SetNoEnterCursor();
                        return;
                    }
                }
                SetEnterCursor();
            }
            else
            {
                SetSelectCursor();
            }
            return;
        }

        // if we have units selected then check attack/enter/move
        if (SelectionManager.Instance.SelectedUnits.Count > 0)
        {
            // check cursor against ability hotbars
            // or micro actions/units
            if (microManager.UpdateCursor(this))
            {
                return;
            }

            _ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // structure enter and enemy attack
            
            if (Physics.Raycast(_ray, out _hit, Mathf.Infinity, GameConstants.EnemyUnitMask))
            {
                var u = _hit.transform.GetComponent<UnitManager>();
                if (VisibilityChecker.Instance.IsVisible(u))
                {
                    SetAttackCursor();
                    return;
                }
            }
            else if (Physics.Raycast(_ray, out _hit, Mathf.Infinity, GameConstants.ExteriorMask))
            {
                var structure = _hit.transform.GetAbsoluteParent().GetComponent<StructureManager>();
                if (structure != null)
                {
                    var allInside = true;
                    for (var i = 0; i < SelectionManager.Instance.SelectedUnits.Count; i++)
                    {
                        if (SelectionManager.Instance.SelectedUnits[i].WorldState == State.Exterior || SelectionManager.Instance.SelectedUnits[i].CurrentStructure != structure)
                        {
                            allInside = false;
                            break;
                        }
                    }
                    if (!allInside)
                    {
                        SetEnterCursor();
                    }
                    else
                    {
                        SetMoveCursor();
                    }
                    return;
                }
                
            } 

            // move
            if (RaycastUtil.IsMouseUnderTerrain() && !RaycastUtil.IsMouseInUI())
            {
                SetMoveCursor();
                return;
            }
        }

        SetPointerCursor();
    }

    private void SetCursor(Texture2D cursor, Vector2 spot)
    {
        if (currentCursor == cursor) return;
        Cursor.SetCursor(cursor, spot, CursorMode.Auto);
        currentCursor = cursor;
    }

    public void SetPointerCursor()
    {
        SetCursor(defaultCursor, TopLeft);
    }

    public void SetEnterCursor()
    {
        SetCursor(enterCursor, BottomMid);
    }

    public void SetNoEnterCursor()
    {
        SetCursor(noEnterCursor, BottomMid);
    }

    public void SetAttackCursor()
    {
        SetCursor(attackCursor, MidMid);
    }

    public void SetMoveCursor()
    {
        SetCursor(moveCursor, BottomMid);
    }

    public void SetSelectCursor()
    {
        SetCursor(selectCursor, MidMid);
    }

    public void SetNoAttackCursor()
    {
        SetCursor(noAttackCursor, MidMid);
    }

    public void SetBlankCursor()
    {
        SetCursor(blankCursor, MidMid);
    }
}