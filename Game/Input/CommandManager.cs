using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FracturedState;
using FracturedState.Game;
using FracturedState.Game.Nav;
using FracturedState.Game.Management;
using FracturedState.Game.Data;
using System.Collections;
using FracturedState.Game.AI;
using FracturedState.UI;
using UnityEngine.SceneManagement;

public class CommandManager : MonoBehaviour
{
    private Transform cameraMaster;
    private CommonCameraController camControl;
    [SerializeField()]
    private MicroManager microManager;
    [SerializeField()]
    private CompassMenuManager compass;
    [SerializeField()]
    private GameObject territoryHelper;
    [SerializeField]
    private TacticalUI tacUI;
    [SerializeField]
    private EscapeMenu escapeMenu;
    [SerializeField] private GameObject facingHelperPrefab;
    private GameObject facingHelper;
    public bool FaceMoving => facingHelper != null && facingHelper.activeInHierarchy;
    private Vector3 faceMovePoint;

    public bool EscapeMenuOpen => escapeMenu.gameObject.activeSelf;

    private bool tacticalMode;
    public bool InTacticalTransition { get; private set; }

    private PlayerProfile playerProfile;

    private void Start()
    {
        camControl = Camera.main.GetComponent<CommonCameraController>();
        cameraMaster = camControl.transform.parent;
        facingHelper = Instantiate(facingHelperPrefab);
        facingHelper.SetLayerRecursively(GameConstants.TerrainLayer);
        facingHelper.SetActive(false);
    }

    private void OnEnable()
    {
        tacticalMode = false;
        playerProfile = ProfileManager.GetActiveProfile();
    }

    private void OnDisable()
    {
        tacUI.enabled = false;
    }

    public void ResetTerritoryHelper()
    {
        var children = territoryHelper.GetComponentsInChildren<Transform>(true);
        foreach (var t in children)
        {
            if (t != territoryHelper.transform)
            {
                Destroy(t.gameObject);
            }
        }
    }

    public void ToggleEscapeMenu()
    {
        if (SkillManager.ActiveSKill == null)
            escapeMenu.gameObject.SetActive(!escapeMenu.gameObject.activeSelf);
    }

    private void Update()
	{
#if UNITY_EDITOR
        // force fire for projectile testing only when in the Unity editor
        if (Input.GetKeyDown(KeyCode.LeftShift) && SelectionManager.Instance.SelectedUnits.Count > 0 && SelectionManager.Instance.SquadCount == 1)
        {
            var hit = RaycastUtil.RaycastTerrainAtMouse();
            if (hit.collider != null)
            {
                foreach (var unit in SelectionManager.Instance.SelectedUnits)
                {
                    if (unit.Data.WeaponData?.ProjectileData != null)
                    {
                        if (unit.Data.WeaponData.SoundEffects != null && unit.Data.WeaponData.SoundEffects.Length > 0)
                        {
                            unit.GetComponent<AudioSource>().PlayOneShot(DataUtil.LoadBuiltInSound(unit.Data.WeaponData.SoundEffects[Random.Range(0, unit.Data.WeaponData.SoundEffects.Length)]));
                        }
                        var proj = ObjectPool.Instance.GetPooledModelAtLocation(unit.Data.WeaponData.ProjectileData.Model, unit.transform.position);
                        var layer = unit.WorldState == State.Exterior ? GameConstants.ExteriorLayer : GameConstants.InteriorLayer;
                        proj.SetLayerRecursively(layer);
                        if (unit.PrimaryMuzzleFlash != null)
                        {
                            proj.transform.position = unit.PrimaryMuzzleFlash.transform.position;
                        }
                        else
                        {
                            proj.transform.position = unit.transform.position;
                        }
                        proj.transform.LookAt(hit.point);
                        var pb = proj.GetComponent<ProjectileBehaviour>();
                        if (pb == null)
                            pb = proj.AddComponent<ProjectileBehaviour>();
                        pb.Init(unit.Data.WeaponData, unit, hit.point);
                    }
                }
            }
        }
#endif

        // escape menu
        if (!IngameChatManager.Instance.ChatInputOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleEscapeMenu();
        }
        // don't process other commands while escape menu is open
        if (escapeMenu.gameObject.activeSelf)
            return;

        // toggle recruit panel open/closed
        if (!IngameChatManager.Instance.ChatInputOpen && Input.GetKeyDown(playerProfile.KeyBindConfig.ToggleRecruitPanel.Key))
        {
            compass.UI.ToggleRecruitPanel();
        }

        // switch to/from tactical view
        if (!IngameChatManager.Instance.ChatInputOpen && !microManager.InMicro && !InTacticalTransition && Input.GetKeyDown(playerProfile.KeyBindConfig.ToggleTacticalMode.Key))
        {
            StartCoroutine(TacticalTransition());
            return;
        }

        // micro processing
        if (Input.GetMouseButton(0) && !microManager.InMicro && SelectionManager.Instance.SelectedUnits.Count > 0 && !SelectionManager.Instance.IsDragSelecting
            && !RaycastUtil.RayCheckUi())
        {
            var unit = RaycastUtil.RaycastFriendlyAtMouse();
            if (unit != null && unit.LocoMotor != null && !(unit.StateMachine.CurrentState is MicroUseAbilityState) && SelectionManager.Instance.SelectedUnits.Contains(unit))
            {
                microManager.MicroUnit = unit;
                microManager.Toggle();
                return;
            }
        }

        var inCompass = !tacticalMode && (compass.CompassRect.Contains(Input.mousePosition) || (compass.UI.RecruitPanel && compass.UI.RecruitArea.Contains(Input.mousePosition)));

        // right click processing with shift to display facing move helper
        if (Input.GetKey(playerProfile.KeyBindConfig.ToggleFacingMove.Key) && SelectionManager.Instance.SelectedUnits.Count > 0 && SkillManager.ActiveSKill == null)
        {
            if (Input.GetMouseButton(1))
            {
                var hit = RaycastUtil.RaycastTerrainAtMouse();
                if (hit.transform != null)
                {
                    var point = hit.point;
                    if (!facingHelper.activeInHierarchy)
                    {
                        faceMovePoint = point;
                        facingHelper.SetActive(true);
                        facingHelper.transform.position = point + Vector3.up * 0.2f;
                    }
                    else
                    {
                        var dir = (point + Vector3.up * 0.2f) - facingHelper.transform.position;
                        var a = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
                        facingHelper.transform.rotation = Quaternion.AngleAxis(a, Vector3.up);
                    }
                }
            }
            else if (Input.GetMouseButtonUp(1))
            {
                facingHelper.SetActive(false);
                DoClickEffect(faceMovePoint);
                var uniqueSquads = new List<Squad>();
                for (var i = 0; i < SelectionManager.Instance.SelectedUnits.Count; i++)
                {
                    if (SelectionManager.Instance.SelectedUnits[i] != null && SelectionManager.Instance.SelectedUnits[i].AcceptInput &&
                        !uniqueSquads.Contains(SelectionManager.Instance.SelectedUnits[i].Squad))
                    {
                        uniqueSquads.Add(SelectionManager.Instance.SelectedUnits[i].Squad);
                    }
                }
                for (var i = 0; i < uniqueSquads.Count; i++)
                {
                    uniqueSquads[i].SquadFacingMove(faceMovePoint, facingHelper.transform.rotation);
                }
            }
        }
        // right click up processing
        else if (Input.GetMouseButtonUp(1) && !camControl.CameraCanMove && SelectionManager.Instance.SelectedUnits.Count > 0 && !inCompass && SkillManager.ActiveSKill == null)
        {
            if (microManager.AbilityActive)
            {
                microManager.ClearActiveAbility();
                return;
            }

            var enemy = RaycastUtil.RaycastEnemyAtMouse();
            if (enemy != null && VisibilityChecker.Instance.IsVisible(enemy))
            {
                UnitObject barkUnit = null;
                for (var i = 0; i < SelectionManager.Instance.SelectedUnits.Count; i++)
                {
                    var unit = SelectionManager.Instance.SelectedUnits[i];
                    if (unit != null && unit.AcceptInput)
                    {
                        var target = unit.DetermineTarget(enemy);
                        if (target != null)
                        {
                            unit.NetMsg.CmdSetTarget(target.NetMsg.NetworkId);
                            barkUnit = SelectionManager.Instance.SelectedUnits[i].Data;
                        }
                    }
                }
                if (barkUnit != null)
                {
                    UnitBarkManager.Instance.AttackBark(barkUnit);
                }
                return;
            }

            var friendly = RaycastUtil.RaycastFriendlyAtMouse();
            if (friendly != null && friendly.Data.IsTransport)
            {
                
                var unitCount = SelectionManager.Instance.SelectedUnits.Count;
                if (SelectionManager.Instance.SelectedUnits.Contains(friendly))
                {
                    unitCount--;
                }
                var canEnter = friendly.CanFitSquad(unitCount);
                if (canEnter)
                {
                    for (var i = 0; i < SelectionManager.Instance.SelectedUnits.Count; i++)
                    {
                        var sm = SelectionManager.Instance.SelectedUnits[i];
                        if (sm == friendly) continue;
                        if (sm == null || !sm.AcceptInput || !sm.Data.CanBePassenger)
                        {
                            canEnter = false;
                            break;
                        }
                    }
                    if (canEnter)
                    {
                        for (var i = 0; i < SelectionManager.Instance.SelectedUnits.Count; i++)
                        {
                            if (SelectionManager.Instance.SelectedUnits[i] != friendly)
                            {
                                SelectionManager.Instance.SelectedUnits[i].RequestTransportEnter(friendly);
                            }
                        }
                        return;

                    }
                }
            }

            var hit = RaycastUtil.RaycastExteriorAtMouse();
            if (hit.transform != null)
            {
                var t = hit.transform.GetAbsoluteParent();
                var structure = t.GetComponent<StructureManager>();
                if (structure != null)
                {
                    var point = RaycastUtil.RaycastTerrainAtMouse().point;
                    DoClickEffect(point);
                    var inStructure = structure.ContainsPoint(new Vector3(point.x, point.y + 1, point.z));
                    var attackMove = Input.GetKey(KeyCode.LeftControl);
                    for (var i = 0; i < SelectionManager.Instance.SelectedUnits.Count; i++)
                    {
                        if (SelectionManager.Instance.SelectedUnits[i] != null && SelectionManager.Instance.SelectedUnits[i].IsAlive && SelectionManager.Instance.SelectedUnits[i].AcceptInput)
                        {
                            SelectionManager.Instance.SelectedUnits[i].Squad.AttackMove = attackMove;
                            if (SelectionManager.Instance.SelectedUnits[i].CurrentStructure == structure && SelectionManager.Instance.SelectedUnits[i].WorldState == State.Interior)
                            {
                                if (inStructure)
                                {
                                    SelectionManager.Instance.SelectedUnits[i].OnSingleMoveIssued(point);
                                    if (SelectionManager.Instance.SelectedUnits.FirstOrDefault(u => u.WorldState == State.Exterior) == null)
                                    {
                                        UnitBarkManager.Instance.MoveBark(SelectionManager.Instance.SelectedUnits[i].Data);
                                    }
                                }
                                else
                                {
                                    SelectionManager.Instance.SelectedUnits[i].OnExitNetworkIssued(point);
                                }
                            }
                            else
                            {
                                if (inStructure || !structure.IsFriendlyOccupied)
                                {
                                    UnitObject barkUnit = null;
                                    if (SelectionManager.Instance.SelectedUnits.FirstOrDefault(u => u.Data.CanEnterBuilding == false) == null)
                                    {
                                        SelectionManager.Instance.SelectedUnits[i].OnEnterNetworkIssued(structure);
                                        barkUnit = SelectionManager.Instance.SelectedUnits[i].Data;
                                    }
                                    if (barkUnit != null)
                                    {
                                        UnitBarkManager.Instance.EnterBark(barkUnit);
                                    }
                                }
                                else
                                {
                                    SelectionManager.Instance.SelectedUnits[i].OnSingleMoveIssued(point);
                                }
                            }
                        }
                    }
                    return;
                }
            }

            hit = RaycastUtil.RaycastTerrainAtMouse();
            if (hit.transform != null)
            {
                if (!facingHelper.activeSelf)
                {
                    var attackMove = Input.GetKey(KeyCode.LeftControl);
                    if (attackMove)
                    {
                        DoAttackClickEffect(hit.point);
                    }
                    else
                    {
                        DoClickEffect(hit.point);
                    }
                    
                    var uniqueSquads = new List<Squad>();
                    for (var i = 0; i < SelectionManager.Instance.SelectedUnits.Count; i++)
                    {
                        var unit = SelectionManager.Instance.SelectedUnits[i];
                        if (unit != null && unit.AcceptInput && !uniqueSquads.Contains(unit.Squad))
                        {
                            unit.Squad.AttackMove = attackMove;
                            uniqueSquads.Add(unit.Squad);
                            unit.OnMoveIssued(hit.point);
                        }
                    }
                }
                else
                {
                    DoClickEffect(facingHelper.transform.position);
                    var uniqueSquads = new List<Squad>();
                    for (var i = 0; i < SelectionManager.Instance.SelectedUnits.Count; i++)
                    {
                        if (SelectionManager.Instance.SelectedUnits[i] != null && SelectionManager.Instance.SelectedUnits[i].AcceptInput &&
                            !uniqueSquads.Contains(SelectionManager.Instance.SelectedUnits[i].Squad))
                        {
                            uniqueSquads.Add(SelectionManager.Instance.SelectedUnits[i].Squad);
                        }
                    }
                    for (var i = 0; i < uniqueSquads.Count; i++)
                    {
                        uniqueSquads[i].SquadFacingMove(faceMovePoint, facingHelper.transform.rotation);
                    }
                    facingHelper.SetActive(false);
                }
            }
        }

        // pressing alt key brings up helper UI elements
        if (Input.GetKeyUp(KeyCode.LeftAlt))
        {
            ToggleHelpers();
        }
	}

    public void ToggleHelpers()
    {
        territoryHelper.SetActive(!StructureManager.HelperOn);
        StructureManager.ToggleHelperUI(!StructureManager.HelperOn);
    }

    private static void DoClickEffect(Vector3 point)
    {
        ClickEffect(point, "MoveClicks/MoveClick");
    }

    private static void DoAttackClickEffect(Vector3 point)
    {
        ClickEffect(point, "MoveClicks/AttackMoveClick");
    }

    private static void ClickEffect(Vector3 point, string particleName)
    {
        var moveSys = ParticlePool.Instance.GetSystem(particleName);
        moveSys.transform.position = point + Vector3.up * 0.2f;
    }

    private void OnApplicationFocus(bool focusStatus)
    {
        if (SceneManager.GetActiveScene().buildIndex == 0 && !focusStatus && territoryHelper.activeSelf && territoryHelper.transform.childCount > 0)
        {
            StructureManager.ToggleHelperUI(false);
            territoryHelper.SetActive(false);
        }
    }

    private IEnumerator TacticalTransition()
    {
        tacticalMode = !tacticalMode;
        tacUI.enabled = tacticalMode;
        camControl.CompassManager.SetEnabled(!tacticalMode);
        camControl.MonitorHeight = !tacticalMode;
        ScreenEdgeNotificationManager.Instance.SetUpdate(!tacticalMode);
        InTacticalTransition = true;

        var curPos = cameraMaster.transform.position;
        var destination = curPos;

        Quaternion destRot;
        var curRot = camControl.transform.rotation;

        if (tacticalMode)
        {
            destRot = Quaternion.Euler(90, camControl.transform.rotation.eulerAngles.y, camControl.transform.rotation.eulerAngles.z);
            destination.y = ConfigSettings.Instance.Values.CameraTacticalHeight;
        }
        else
        {
            destRot = Quaternion.Euler(ConfigSettings.Instance.Values.CameraDefaultAngle, camControl.transform.rotation.eulerAngles.y, camControl.transform.rotation.eulerAngles.z);
            destination.y = ConfigSettings.Instance.Values.CameraDefaultHeight;
        }

        var time = 0f;
        var moveTime = 0.5f;
        var rotateTime = tacticalMode ? 0.15f : moveTime;
        while (time < moveTime)
        {
            time += Time.deltaTime;
            cameraMaster.transform.position = Vector3.Lerp(curPos, destination, Mathf.Clamp(time / moveTime, 0, 1));
            camControl.transform.rotation = Quaternion.Slerp(curRot, destRot, Mathf.Clamp(time / rotateTime, 0, 1));
            yield return null;
        }
        InTacticalTransition = false;
    }
}