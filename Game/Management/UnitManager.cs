using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using FracturedState.Game;
using FracturedState.Game.AI;
using FracturedState.Game.Data;
using FracturedState.Game.Management;
using FracturedState.Game.Management.StructureBonus;
using FracturedState.Game.Modules;
using FracturedState.Game.Nav;
using FracturedState.Game.Network;
using FracturedState.Scripting;
using HighlightingSystem;
using JetBrains.Annotations;
using Steamworks;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

public class UnitManager : MonoBehaviour, IComparable<UnitManager>
{
    private static GameObject _shroudHelper;

    public UnitStateMachine StateMachine { get; private set; }

    public UnitMessages NetMsg { get; private set; }
    public Animation AnimControl { get; private set; }

    public bool IsMine { get; set; }

    public bool IsFriendly => OwnerTeam.Side == FracNet.Instance.LocalTeam.Side;

    public bool AcceptInput { get; set; }
    public bool IsIdle { get; set; }

    private GameObject _extModel;

    public StateEffectManager EffectManager { get; private set; }

    // move state
    public Locomotor LocoMotor { get; set; }
    public Vector3 CurrentVelocity;
    public State WorldState { get; set; }
    public MoveCommandDelegate OnMoveIssued;
    public MoveCommandDelegate OnSingleMoveIssued;

    // enter/exit structure state
    public StructureManager CurrentStructure { get; set; }
    public Transform CurrentFirePoint { get; set; }
    public bool IsOnFirePoint => CurrentFirePoint != null;
    public EnterCommandDelegate OnEnterIssued;
    public EnterCommandDelegate OnEnterNetworkIssued;
    public ExitCommandDelegate OnExitIssued;
    public ExitCommandDelegate OnExitNetworkIssued;
    public TakeFirePointDelegate OnTakeFirePointIssued;

    // cover
    public bool InCover { get; set; }
    public CoverManager CurrentCover { get; set; }
    public CoverPoint CurrentCoverPoint { get; set; }
    public TakeCoverPointDelegate OnTakeCoverPointIssued;

    // attack state
    public float LastFiredTime { get; set; }
    public TargetChangedDelegate OnTargetChanged;
    // attack state particle effects
    public ParticleSystem PrimaryMuzzleFlash { get; private set; }
    public ParticleSystem CoverMuzzleFlash { get; private set; }
    public ParticleSystem FirePointMuzzleFlash { get; set; }
    public Transform WeaponHitBone { get; private set; }
    public Weapon ContextualWeapon
    {
        get
        {
            if (CurrentCover != null && Data.CoverWeaponData != null)
                return Data.CoverWeaponData;

            return Data.WeaponData;
        }
    }
    public ParticleSystem ContextualMuzzleFlash
    {
        get
        {
           if (CurrentCoverPoint != null && Data.CoverWeaponData != null)
                return CoverMuzzleFlash;

            return PrimaryMuzzleFlash;
        }
    }

    // health
    public DamageModule DamageProcessor { get; private set; }
    public bool IsAlive => DamageProcessor != null && DamageProcessor.IsAlive;

    // micro management
    private MicroBaseState _microState;
    private AbilityManager _abilityManager;

    // management
    public UnitObject Data { get; private set; }
    public Squad Squad { get; private set; }
    public UnitSelectedDelegate OnSelected;
    public UnitUnSelectedDelegate OnDeSelected;
    private GameObject _selectionProjector;

    // micro actions
    public bool IsMicroing { get; set; }

    // transport properties
    private GameObject _occupiedModel;
    private int _currentCapacity;
    private Transform[] _transportPoints;
    private List<Transform> _fullTransportPoints;
    public List<UnitManager> Passengers { get; private set; }
    public UnitManager Transport { get; set; } // for passengers to know what unit they are riding in
    public Transform PassengerSlot { get; set; }

    public UnitDataBridge Stats { get; private set; }

    public Team OwnerTeam { get; private set; }
    public bool AIControlled { get; private set; }
    public bool AISimulate => AIControlled && FracNet.Instance.IsHost;

    #region Properties

    private static Transform _worldUnitParent;
    public static Transform WorldUnitParent
    {
        get
        {
            if (_worldUnitParent == null)
            {
                _worldUnitParent = GameObject.Find("UnitParent").transform;
            }
            return _worldUnitParent;
        }
    }

    private float _unitRadius = -1f;
    public float UnitRadius
    {
        get
        {
            if (_unitRadius >= 0) return _unitRadius;
            if (Data.BoundsBox != null)
            {
                Vector3 bounds;
                Data.BoundsBox.Bounds.TryVector3(out bounds);
                _unitRadius = (bounds.x > bounds.z) ? bounds.x : bounds.z;
            }
            else
            {
                _unitRadius = 0.1f;
            }
            return _unitRadius;
        }
    }

    public UnitManager CurrentTarget
    {
        get
        {
            var state = StateMachine.CurrentState as UnitAttackState;
            return state?.Target;
        }
    }

    public bool IsMicroPrepped => _microState != null;

    #endregion

    #region Initialization

    public void AddAbility(string ability)
    {
        if (_abilityManager == null)
            _abilityManager = new AbilityManager();

        var added = _abilityManager.AddAbility(ability);
        if (!added) return;
        
        var script = XmlCacheManager.Abilities[ability].Script;
        if (string.IsNullOrEmpty(script)) return;

        var t = ScriptManager.ResolveScriptType(script);
        if (t.IsSubclassOf(typeof(MutatorAbility)))
        {
            var loaded = ScriptManager.CreateAbilityScriptInstance(script, this);
            loaded.ExecuteAbility();
        }
    }

    public void RemoveAbility(string ability)
    {
        if (_abilityManager == null || !_abilityManager.HasAbility(ability)) return;
        
        _abilityManager?.RemoveAbility(ability);
        var script = XmlCacheManager.Abilities[ability].Script;
        if (string.IsNullOrEmpty(script)) return;
        
        var t = ScriptManager.ResolveScriptType(script);
        if (t.IsSubclassOf(typeof(MutatorAbility)))
        {
            var loaded = ScriptManager.CreateAbilityScriptInstance(script, this) as MutatorAbility;
            loaded?.Remove();
        }
    }

    public Ability[] GetPassiveAbilities()
    {
        return _abilityManager?.PassiveAbilities;
    }

    public Ability GetAbility(string abilityName)
    {
        return _abilityManager?.GetAbilityData(abilityName);
    }

    public void SetSquad(Squad squad)
    {
        Squad = squad;
        if (transform == null) return;
        var vision = transform.Find("Vision");
        if (vision == null) return;
        var los = vision.GetComponent<LosManager>();
        los.SetSquad(Squad);
    }

    public void CreateNetworkedAIUnit(string unitName, string playerName)
    {
        Data = UnitObject.DeepClone(XmlCacheManager.Units[unitName]);
        IsMine = false;
        AcceptInput = true;
        AIControlled = true;
        OwnerTeam = SkirmishVictoryManager.GetTeam(playerName);
        CreateUnit(unitName, OwnerTeam.TeamColor.UnityColor);
        RegisterNetworkEvents();
        gameObject.layer = GameConstants.ExteriorEnemyLayer;
        if (Data.VisionRange > 0 && (AISimulate || IsFriendly))
        {
            var v = new GameObject("Vision");
            v.transform.position = transform.position;
            v.transform.parent = transform;
            v.AddComponent<LosManagerAI>();
            var c = v.AddComponent<SphereCollider>();
            c.radius = Data.VisionRange;
            c.isTrigger = true;
        }
        StateMachine.ChangeState(new UnitIdleState(this));

        if (!FracNet.Instance.LocalTeam.IsSpectator && FracNet.Instance.LocalTeam.IsActive && !IsFriendly)
        {
            VisibilityChecker.Instance.RegisterUnit(this);
        }
    }

    public void CreateNetworkedUnit(string unitName, int ownerId)
    {
        Data = UnitObject.DeepClone(XmlCacheManager.Units[unitName]);
        OwnerTeam = SkirmishVictoryManager.GetTeam(ownerId);
        IsMine = OwnerTeam == FracNet.Instance.NetworkActions.LocalTeam;
        AcceptInput = true;
        
        CreateUnit(unitName, OwnerTeam.TeamColor.UnityColor);
        RegisterNetworkEvents();
        if (IsMine)
        {
            if (Data.IsSelectable)
            {
                SelectionManager.Instance.RegisterUnit(this);
            }
            
            if (Data.UseProximity)
            {
                var p = new GameObject("Proximity");
                p.transform.position = transform.position;
                p.transform.parent = transform;
                p.AddComponent<ProximityManager>();
                var c = p.AddComponent<SphereCollider>();
                c.radius = Data.WeaponData.Range;
                c.isTrigger = true;
            }

            if (Data.VisionRange > 0)
            {
                var v = new GameObject("Vision");
                v.transform.position = transform.position;
                v.transform.parent = transform;
                v.AddComponent<LosManager>();
                var c = v.AddComponent<SphereCollider>();
                c.radius = Data.VisionRange;
                c.isTrigger = true;
            }
        }
        else if (IsFriendly)
        {
            gameObject.layer = GameConstants.ExteriorEnemyLayer;
            if (Data.VisionRange > 0)
            {
                var v = new GameObject("Vision");
                v.transform.position = transform.position;
                v.transform.parent = transform;
                v.AddComponent<LosManager>();
                var c = v.AddComponent<SphereCollider>();
                c.radius = Data.VisionRange;
                c.isTrigger = true;
            }
        }
        else
        {
            gameObject.layer = GameConstants.ExteriorEnemyLayer;
            if (!FracNet.Instance.NetworkActions.LocalTeam.IsSpectator && FracNet.Instance.LocalTeam.IsActive)
            {
                VisibilityChecker.Instance.RegisterUnit(this);
            }
        }
    }

    private void CreateUnit(string unitName, Color teamColor)
	{
        foreach (var m in OwnerTeam.Mutators)
        {
            if (m.CanMutate(this))
            {
                m.Mutate(this);
            }
        }
        
	    // apply any structure based bonuses before any other processing
	    StructureBonusManager.ApplyUnitBonuses(OwnerTeam, this);
	    
        NetMsg = GetComponent<UnitMessages>();
        if (!string.IsNullOrEmpty(Data.DamageModule))
        {
            var qName = $"FracturedState.Game.Modules.{Data.DamageModule}";
            var type = Type.GetType(qName);
            DamageProcessor = gameObject.AddComponent(type) as DamageModule;
        }
        if (Data.Abilities != null && Data.Abilities.Length > 0)
        {
            _abilityManager = new AbilityManager(Data.Abilities);
        }
        SetInitialDelegates();
		Stats = new UnitDataBridge(unitName, this);
		StateMachine = new UnitStateMachine();
        GetComponent<AudioSource>().volume *= ProfileManager.GetEffectsVolumeFromProfile();
        if (Data.Model != null)
        {
            var modelContainer = new GameObject(GameConstants.ModelContainerName);
            modelContainer.transform.position = transform.position;
            modelContainer.transform.rotation = transform.rotation;
            if (Data.Model.ExteriorModel != string.Empty)
            {
                _extModel = DataUtil.LoadBuiltInModel(Data.Model.ExteriorModel);
                _extModel.transform.position = transform.position;
                _extModel.transform.rotation = transform.rotation;
                _extModel.transform.parent = modelContainer.transform;
            }
            modelContainer.transform.parent = transform;
            modelContainer.SetLayerRecursively(GameConstants.ExteriorUnitLayer);

            // set housecolor
            GetComponent<Highlighter>().ConstantOn(teamColor);

            // get references to particle effect bones
            if (Data.ParticleBones != null && Data.ParticleBones.WeaponHit != string.Empty)
            {
                WeaponHitBone = modelContainer.transform.GetChildByName(Data.ParticleBones.WeaponHit);
                if (WeaponHitBone == null)
                {
                    throw new FracturedStateException(Data.Name + " declares particle effect bone " + Data.ParticleBones.WeaponHit + " but no bone can be found.");
                }
            }
            AnimControl = GetComponentInChildren<Animation>();

            // done here because point lookup is contingent upon having some kind of model information
            if (Data.TransportLogic != null)
            {
                Passengers = new List<UnitManager>();
                var pointCheck = transform;
                if (!string.IsNullOrEmpty(Data.TransportLogic.OccupiedModelName))
                {
                    _occupiedModel = DataUtil.LoadBuiltInModel(Data.TransportLogic.OccupiedModelName);
                    _occupiedModel.SetLayerRecursively(GameConstants.ExteriorLayer);
                    _occupiedModel.transform.position = transform.position;
                    _occupiedModel.transform.rotation = transform.rotation;
                    _occupiedModel.transform.parent = modelContainer.transform;
                    pointCheck = _occupiedModel.transform;
                }
                _fullTransportPoints = new List<Transform>();
                _transportPoints = new Transform[Data.TransportLogic.PassengerPoints.Length];
                for (var i = 0; i < Data.TransportLogic.PassengerPoints.Length; i++)
                {
                    _transportPoints[i] = pointCheck.GetChildByName(Data.TransportLogic.PassengerPoints[i]);
                    if (_transportPoints[i] == null)
                    {
                        throw new FracturedStateException("Cannot find transport slot " + Data.TransportLogic.PassengerPoints[i] + " in " + Data.Name);
                    }
                }
                if (_occupiedModel != null)
                {
                    _occupiedModel.SetActive(false);
                }
            }

            // load references to state-specific particle effects
            if (Data.StatefulEffects != null)
            {
                EffectManager = new StateEffectManager(this);
            }
        }

        SetWeaponData(Data.WeaponName);
	    SetCoverWeaponData(Data.CoverWeaponName);

        if (Data.ArmorName != string.Empty)
        {
            Data.ArmorValues = XmlCacheManager.Armors[Data.ArmorName];
        }
        
        if (Data.BoundsBox != null)
        {
            Vector3 center, bounds;
            if (Data.BoundsBox.Center.TryVector3(out center) && Data.BoundsBox.Bounds.TryVector3(out bounds))
            {
                var bc = gameObject.GetComponent<BoxCollider>();
                bc.center = center;
                bc.size = bounds;
            }
        }
        else
        {
            gameObject.GetComponent<BoxCollider>().enabled = false;
        }
        if (IsMine || IsFriendly)
        {
            if (_shroudHelper == null)
            {
                var sc = FindObjectOfType<ShroudCamera>();
                _shroudHelper = sc.ShroudPrefab;
            }
            var shroud = Instantiate(_shroudHelper);
            shroud.transform.position = transform.position;
            shroud.transform.localScale = new Vector3(Data.VisionRange * 2.1f, Data.VisionRange * 2.1f, 0);
            shroud.GetComponent<ShroudFollower>().SetTarget(transform);
        }
        gameObject.name = unitName;
        gameObject.tag = GameConstants.UnitTag;
        transform.parent = WorldUnitParent;
        WorldState = State.Exterior;
        IsIdle = true;
        EffectManager?.PlayIdleSystems();
        if (AnimControl != null && Data.Animations?.Idle != null && Data.Animations.Idle.Length > 0)
        {
            AnimControl.Play(Data.Animations.Idle[Random.Range(0, Data.Animations.Idle.Length)], PlayMode.StopAll);
        }
    }

    private void SetInitialDelegates()
	{
        OnTargetChanged = delegate(UnitManager target)
        {
            if (target == null || !IsAlive) return;
            
            if (target.Transport != null)
            {
                target = target.Transport;
            }

            // if we're in a fire point
            if (CurrentFirePoint != null)
            {
                StateMachine.ChangeState(new UnitFirePointAttackState(this, CurrentFirePoint, target));
            }
            // if we're in a transport
            else if (Transport != null && PassengerSlot != null)
            {
                StateMachine.ChangeState(new PassengerAttackState(this, PassengerSlot, target));
            }
            // if we ARE a transport
            else if (Data.IsTransport)
            {
                StateMachine.ChangeState(new TransportAttackState(this, target));
            }
            else
            {
                // garrison units just get normal attack state for now
                if (Data.IsGarrisonUnit)
                {
                    StateMachine.ChangeState(new UnitAttackState(this, target));
                    return;
                }
                // if we are outside and the target is inside or we're in different buildings
                if (target.WorldState == State.Interior && CurrentStructure != target.CurrentStructure)
                {
                    if (CurrentStructure != null)
                    {
                        StateMachine.ChangeState(new UnitEnterStructureAttackState(this, CurrentStructure, target.CurrentStructure, target));
                    }
                    else
                    {
                        StateMachine.ChangeState(new UnitEnterStructureAttackState(this, target.CurrentStructure, target));
                    }
                }
                // if we are inside and the target is outside
                else if (target.WorldState == State.Exterior && WorldState == State.Interior)
                {
                    StateMachine.ChangeState(new UnitExitStructureAttackState(this, target));
                }
                else
                {
                    StateMachine.ChangeState(new UnitAttackState(this, target));
                }
            }
        };
        OnEnterIssued = delegate(StructureManager structure)
        {
            if (WorldState == State.Interior)
            {
                if (CurrentFirePoint != null)
                {
                    CurrentStructure.ReturnFirePoint(CurrentFirePoint);
                    CurrentFirePoint = null;
                }
                StateMachine.ChangeState(new UnitEnterStructureState(this, CurrentStructure, structure));
            }
            else
            {
                StateMachine.ChangeState(new UnitEnterStructureState(this, structure));
            }
        };
        OnExitIssued = delegate(Vector3 destination)
        {
            StateMachine.ChangeState(new UnitExitStructureState(this, destination));
        };
        OnTakeFirePointIssued = delegate(Transform firePoint)
        {
            CurrentFirePoint = firePoint;
            StateMachine.ChangeState(new UnitTakeFirePointState(this, CurrentFirePoint));
        };
        OnTakeCoverPointIssued = delegate(CoverManager cover, Transform coverPoint)
        {
            StateMachine.ChangeState(new UnitTakeCoverState(this, cover, coverPoint));
        };
	}

    private void RegisterNetworkEvents()
	{
        if (IsMine || AISimulate)
        {
            OnMoveIssued += delegate(Vector3 destination) { Squad.SquadMove(destination); };
            OnSingleMoveIssued += delegate(Vector3 destination) { Squad.SingleMove(this, destination); };
            OnEnterNetworkIssued += delegate(StructureManager structure)
            {
                var id = structure.gameObject.GetComponent<Identity>();
                if (id != null)
                {
                    NetMsg.CmdEnterStructure(id.UID);
                }
                else
                {
                    throw new FracturedStateException("Attempted to enter structure with invalid UID");
                }
            };
            OnExitNetworkIssued += delegate(Vector3 destination)
            {
                NetMsg.CmdExitStructure(destination);
            };
        }

        // select/deselect
	    if (!IsMine) return;
	    
	    OnSelected = delegate(bool propagate)
	    {
	        if (transform == null) return;
	        
	        // play a select bark
	        if (propagate)
	        {
	            UnitBarkManager.Instance.SelectBark(Data);
	        }
	        SelectionManager.Instance.AddUnit(this);
	        // get a selection projector and apply it
	        if (_selectionProjector == null)
	        {
	            _selectionProjector = ObjectPool.Instance.GetSelectionProjector();
                _selectionProjector.GetComponent<SelectionProjectorFollow>().SetTarget(this);
                _selectionProjector.transform.GetChild(0).localScale = Vector3.one * Data.SelectionScale;
	            _selectionProjector.transform.position = transform.position;
	            _selectionProjector.SetActive(true);
	        }
	        // send squad message
	        if (propagate)
	            Squad?.InformSquadSelection(this, true);
	    };
	    OnDeSelected = delegate(bool propagate)
	    {
	        // return selection projector
	        if (_selectionProjector != null)
	        {
	            ObjectPool.Instance.ReturnSelectionProjector(_selectionProjector);
	            _selectionProjector = null;
	        }
	        SelectionManager.Instance.RemoveUnit(this);
	        
	        if (propagate)
	            Squad?.InformSquadSelection(this, false);
	    };
	}

    #endregion

    public IEnumerator RunDelayedAction(Action action, IState stateCheck, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (IsAlive && StateMachine.CurrentState == stateCheck)
        {
            action();
        }
    }

    private void Update()
    {
        if (StateMachine?.CurrentState != null && IsAlive)
            StateMachine.CurrentState.Execute();

        if (CurrentStructure != null && CurrentStructure.StructureData.CanBeCaptured && WorldState == State.Interior)
            CurrentStructure.ContributePoints(this);

        Stats?.Update();

        if (!IsMine) return;

        _abilityManager?.Update();
    }

    private void ResetSelection()
    {
        if (_selectionProjector != null)
        {
            ObjectPool.Instance.ReturnSelectionProjector(_selectionProjector);
        }
    }

    public void UpdateSelectionLayer()
    {
        if (_selectionProjector != null)
        {
            _selectionProjector.SetLayerRecursively(gameObject.layer);
        }
    }

    public void ReturnFirePoint()
    {
        if (CurrentFirePoint == null) return;
        
        CurrentStructure.ReturnFirePoint(CurrentFirePoint);
        CurrentFirePoint = null;
    }

    #region Transport Logic

    public void RequestTransportEnter(UnitManager transport)
    {
        NetMsg.CmdEnterTransport(transport.GetComponent<NetworkIdentity>());
    }

    public void RequestTransportExit()
    {
        if (NetMsg != null)
        {
            NetMsg.CmdExitTransport();
        }
    }

    public bool CanFitSquad(int unitCount)
    {
        return _currentCapacity + unitCount <= _transportPoints.Length;
    }

    public Transform GetNextTransportSlot()
    {
        if (Data.TransportLogic.Capacity > _currentCapacity)
        {
            for (var i = 0; i < _transportPoints.Length; i++)
            {
                if (!_fullTransportPoints.Contains(_transportPoints[i]))
                {
                    _fullTransportPoints.Add(_transportPoints[i]);
                    if (IsMine && _currentCapacity == 0 && _occupiedModel != null)
                    {
                        _occupiedModel.SetActive(true);
                        _extModel.SetActive(false);
                        GetComponent<Highlighter>().ReinitMaterials();
                    }
                    _currentCapacity++;
                    return _transportPoints[i];
                }
            }
        }
        return null;
    }

    public void ReturnTransportSlot(Transform point)
    {
        _fullTransportPoints.Remove(point);
        _currentCapacity--;
        if (IsMine && _currentCapacity == 0 && _occupiedModel != null)
        {
            _extModel.SetActive(true);
            _occupiedModel.SetActive(false);
            GetComponent<Highlighter>().ReinitMaterials();
        }
    }

    #endregion

    #region Damage and Death

    public int GetMovementHitPenalty(Vector3 attackForward)
    {
        var facing = Mathf.Abs(Vector3.Dot(attackForward, CurrentVelocity.normalized));
        var mod = Mathf.Lerp(1, 0, facing);
        return (int)(CurrentVelocity.magnitude * (ConfigSettings.Instance.Values.MovementAccuracyPenalty * mod));
    }

    public void CleanUp()
    {
        StopAllCoroutines();
        Squad?.RemoveSquadUnit(this);
        StateMachine.ChangeState(null);
        var cols = gameObject.GetComponentsInChildren<Collider>();
        if (cols != null)
        {
            foreach (var col in cols)
            {
                col.enabled = false;
            }
        }
        SelectionManager.Instance.UnregisterUnit(this);
        if (IsMine)
        {
            ResetSelection();
            SelectionManager.Instance.RemoveUnit(this);
        }
        else
        {
            VisibilityChecker.Instance.UnregisterUnit(this);
        }
        
        if (InCover)
        {
            RemoveFromCover();
        }
        if (CurrentStructure != null)
        {
            if (CurrentFirePoint != null)
                CurrentStructure.ReturnFirePoint(CurrentFirePoint);

            CurrentStructure.Leave(this);
        }
        if (FirePointMuzzleFlash != null)
        {
            Destroy(FirePointMuzzleFlash);
        }
        if (CurrentCover != null)
        {
            CurrentCover.UnoccupyPoint(this);
        }
        if ((Data.IsSelectable || Data.IsGarrisonUnit))
        {
            StartCoroutine(DestroyOwnedUnit());
        }
        else if (FracNet.Instance.IsHost)
        {
            Destroy(gameObject, 1);
        }
    }

    private IEnumerator DestroyOwnedUnit()
    {
        yield return new WaitForSeconds(2);
        if (FracNet.Instance.IsHost)
        {
            Destroy(gameObject);
        }
    }

    public void ProcessDamageInterrupts(int damage, UnitManager attacker)
    {
        var id = attacker != null ? attacker.NetMsg.NetworkId : null;
        if (_abilityManager == null)
        {
            NetMsg.CmdTakeDamage(damage, id, id != null ? attacker.ContextualWeapon.Name : Weapon.DummyName);
        }
        else
        {
            var interrupt = false;
            foreach (var ab in _abilityManager.PassiveAbilities)
            {
                if (!interrupt && !string.IsNullOrEmpty(ab.Script) && _abilityManager.GetRemainingCooldown(ab.Name) < 0)
                {
                    var type = ScriptManager.ResolveScriptType(ab.Script);
                    if (!type.IsSubclassOf(typeof(PassiveAttackInterrupt))) continue;
                    
                    var args = new object[] { this, attacker };
                    var ability = ScriptManager.CreateAbilityScriptInstance(ab.Script, args) as PassiveAttackInterrupt;
                    if (ability != null && !ability.Proc()) continue;
                    interrupt = true;
                    _microState = new MicroUseAbilityState(this, ab.Name, ability, attacker);
                    PropagateMicroState();
                }
            }
            if (!interrupt)
            {
                NetMsg.CmdTakeDamage(damage, id, attacker != null ? attacker.ContextualWeapon.Name : Weapon.DummyName);
            }
        }
    }

    public UnitManager DetermineTarget(UnitManager originalTarget)
    {
        if (Data.CustomBehaviours?.TargetBehaviour != null)
        {
            return Data.CustomBehaviours.TargetBehaviour.FindTarget(this);
        }

        var target = originalTarget;
        if (target != null && !target.IsAlive)
        {
            target = null;
        }
        var nearby = Physics.OverlapSphere(transform.position, Data.VisionRange, GetEnemyLayerMask());
        var availableTargets = new List<UnitManager>();
        foreach (var u in nearby)
        {
            var unit = u.GetComponent<UnitManager>();
            if (unit != null && unit.IsAlive &&
                ((IsMine && VisibilityChecker.Instance.IsVisible(unit) && VisibilityChecker.Instance.HasSight(this, unit)) || (AIControlled && unit.OwnerTeam != OwnerTeam)))
            {
                // manually check side here instead of using property because from an AI perspective we end up comparing
                // against ourself in the property
                if (unit.OwnerTeam.Side == OwnerTeam.Side) continue;
                
                // AI units need additional checks
                if (AIControlled)
                {
                    if (unit.WorldState != WorldState)
                    {
                        continue;
                    }

                    if (WorldState == State.Interior && (unit.CurrentStructure != CurrentStructure))
                    {
                        continue;
                    }
                }

                // fire point units need to check facing if target is exterior and weapon range because they can't move
                if (IsOnFirePoint)
                {
                    var toUnit = (unit.transform.position - transform.position);
                    if (toUnit.magnitude > ContextualWeapon.Range || unit.WorldState == State.Exterior &&
                        Vector3.Dot(CurrentFirePoint.forward, toUnit.normalized) < ConfigSettings.Instance.Values.FirePointVisionThreshold)
                    {
                        continue;
                    }
                }
                // garrison units need to check range because they can't move
                else if (Data.IsGarrisonUnit && (unit.transform.position - transform.position).magnitude > ContextualWeapon.Range)
                {
                    continue;
                }

                availableTargets.Add(unit);
            }
        }
        if (availableTargets.Count > 0)
        {
            target = availableTargets[Random.Range(0, availableTargets.Count)];
        }
        return target;
    }

    public int MitigateDamage(Weapon weapon, Vector3 attackPosition)
    {
        var armor = Data.GetDamageResistance(weapon.DamageType) + Stats.ArmorModifier;
        var r = Mathf.RoundToInt(weapon.Damage * (armor / 100f));
        var dir = Data.GetDirectionReduction();
        if (dir > 0 && Vector3.Dot(transform.forward, (attackPosition - transform.position).normalized) > 0)
        {
            r += Mathf.RoundToInt(weapon.Damage * (dir / 100f));
        }
        r = weapon.Damage - r;
        return r > 0 ? r : 0;
    }

    public int MitigateDamage(Weapon weapon, int damage, Vector3 attackPosition)
    {
        var armor = Data.GetDamageResistance(weapon.DamageType) + Stats.ArmorModifier;
        var r = Mathf.RoundToInt(damage * (armor / 100f));
        var dir = Data.GetDirectionReduction();
        if (dir > 0 && Vector3.Dot(transform.forward, (attackPosition - transform.position).normalized) > 0)
        {
            r += Mathf.RoundToInt(damage * (dir / 100f));
        }
        r = damage - r;
        return r > 0 ? r : 0;
    }

    #endregion

    #region Cover Management

    public void DoCoverCheck()
    {
        if (Squad == null || Squad.IsGettingCover || !Data.CanTakeCover) return;
        if (WorldState == State.Interior)
        {
            var coverObjects = new List<CoverManager>();
            if (CurrentStructure != null && CurrentStructure.ContainedProps != null)
            {
                foreach (var prop in CurrentStructure.ContainedProps)
                {
                    var cover = prop.GetComponent<CoverManager>();
                    if (cover != null && (cover.transform.position - transform.position).magnitude <= Data.VisionRange)
                    {
                        coverObjects.Add(cover);
                    }
                }
            }
            Squad.DetermineCover(coverObjects, this);
        }
        else
        {
            Squad.DetermineCover(Physics.OverlapSphere(transform.position, ConfigSettings.Instance.Values.CoverCheckDistance, GameConstants.ExteriorMask), this);
        }
    }

    public void RemoveFromCover()
    {
        if (CurrentCover != null) CurrentCover.UnoccupyPoint(this);
        InCover = false;
        CurrentCover = null;
        CurrentCoverPoint = null;
        Squad?.RemoveUnitFromCover(this);
    }

    #endregion

    #region Micro

    public Ability[] GetAbilities()
    {
        return _abilityManager?.GetUnitAbilities();
    }

    public Ability[] GetSquadAbilities()
    {
        return _abilityManager?.GetSquadAbilities();
    }

    public bool HasAbility(string ability)
    {
        return _abilityManager != null && _abilityManager.HasAbility(ability);
    }

    public void SetMicroState(MicroBaseState state)
    {
        _microState = state;
    }

    public void ExecuteMicroState()
    {
        if (_microState == null) return;
        StateMachine.ChangeState(_microState);
        _microState = null;
    }

    public void PropagateMicroState()
    {
        if (_microState == null || NetMsg == null) return;
        
        if (_microState is MicroTakeCoverState)
        {
            var state = (MicroTakeCoverState)_microState;
            var coverId = state.CoverManager.GetComponent<Identity>().UID;
            NetMsg.CmdTakeCover(coverId, state.CoverPoint.name);
            _microState = null;
        }
        else if (_microState is MicroTakeFirepointState)
        {
            var state = (MicroTakeFirepointState) _microState;
            NetMsg.CmdTakeFirePoint(state.FirePointName);
            _microState = null;
        }
        else if (_microState is MicroMoveState)
        {
            NetMsg.CmdMicroMove(((MicroMoveState)_microState).Destination);
        }
        else if (_microState is MicroUseAbilityState)
        {
            var state = (MicroUseAbilityState)_microState;
            var targetId = state.Target != null ? state.Target.NetMsg.NetworkId : null;
            NetMsg.CmdUseAbility(state.AbilityData.Name, state.Position, targetId);
        }
    }

    public void UseAbility(string ability)
    {
        _abilityManager?.UseAbility(ability);
    }

    public float GetRemainingAbilityTime(string ability)
    {
        return _abilityManager?.GetRemainingCooldown(ability) ?? -1;
    }

    #endregion

    public int GetEnemyLayerMask()
    {
        int layerMask;
        if (IsOnFirePoint)
        {
            if (IsMine)
            {
                layerMask = GameConstants.ExteriorEnemyMask | GameConstants.InteriorEnemyMask;
            }
            else
            {
                layerMask = GameConstants.ExteriorUnitAllMask | GameConstants.InteriorUnitAllMask;
            }
        }
        else
        {
            if (IsMine)
            {
                layerMask = WorldState == State.Exterior ? GameConstants.ExteriorEnemyMask : GameConstants.InteriorEnemyMask;
            }
            else
            {
                layerMask = WorldState == State.Exterior ? GameConstants.ExteriorUnitAllMask : GameConstants.InteriorUnitAllMask;
            }
        }
        return layerMask;
    }

    /// <summary>
    /// Updates this unit's weapon using the weapon data associated with the given name
    /// </summary>
    /// <param name="weaponName"></param>
    /// <exception cref="FracturedStateException"></exception>
    public void SetWeaponData(string weaponName)
    {
        // remove old effects before swapping if we already have a weapon
        if (Data.WeaponData != null)
        {
            if (!string.IsNullOrEmpty(Data.WeaponData.MuzzleFlashEffect))
            {
                var fx = transform.GetChildByName(Data.WeaponData.MuzzleFlashEffect.Replace('/', '_'));
                if (fx != null) Destroy(fx.gameObject);
            }
            PrimaryMuzzleFlash = null;
            Data.WeaponData = null;
        }
        
        if (string.IsNullOrEmpty(weaponName))
        {
            Data.WeaponData = null;
            return;
        }

        Data.WeaponData = new Weapon(XmlCacheManager.Weapons[weaponName]);
        
        // apply structure bonuses
        StructureBonusManager.ApplyWeaponBonuses(OwnerTeam, Data.WeaponData);
        
        // apply ability mutators
        if (_abilityManager?.PassiveAbilities?.Length > 0)
        {
            foreach (var ab in _abilityManager.PassiveAbilities)
            {
                if (string.IsNullOrEmpty(ab.Script)) continue;
                
                var t = ScriptManager.ResolveScriptType(ab.Script);
                if (!t.IsSubclassOf(typeof(MutatorAbility))) continue;
                
                var loaded = ScriptManager.CreateAbilityScriptInstance(ab.Script, this) as MutatorAbility;
                if (loaded != null && loaded.MutatesWeapon)
                {
                    loaded.ExecuteAbility();
                }
            }
        }
        if (string.IsNullOrEmpty(Data.ParticleBones?.PrimaryMuzzleFlash) || string.IsNullOrEmpty(Data.WeaponData.MuzzleFlashEffect)) return;
        
        var effectBone = transform.GetChildByName(Data.ParticleBones.PrimaryMuzzleFlash);
        if (effectBone == null)
            throw new FracturedStateException(Data.Name + " declares particle bone " +
                                              Data.ParticleBones.PrimaryMuzzleFlash + " that does not exist");

        var muzzleFx = DataUtil.LoadBuiltInParticleSystem(Data.WeaponData.MuzzleFlashEffect);
        if (muzzleFx == null)
            throw new FracturedStateException(Data.WeaponData.Name + " declares muzzle flash " +
                                              Data.WeaponData.MuzzleFlashEffect + " that does not exist");

        muzzleFx.transform.position = effectBone.position;
        muzzleFx.transform.rotation = effectBone.rotation;
        muzzleFx.transform.parent = effectBone;
        muzzleFx.SetLayerRecursively(GameConstants.ExteriorUnitLayer);
        PrimaryMuzzleFlash = muzzleFx.GetComponent<ParticleSystem>();
    }

    /// <summary>
    /// Sets this unit's cover weapon using the weapon data associated with the given name
    /// </summary>
    /// <param name="weaponName"></param>
    /// <exception cref="FracturedStateException"></exception>
    public void SetCoverWeaponData(string weaponName)
    {
        if (Data.CoverWeaponData != null)
        {
            if (!string.IsNullOrEmpty(Data.CoverWeaponData.MuzzleFlashEffect))
            {
                var fx = transform.GetChildByName(Data.CoverWeaponData.MuzzleFlashEffect.Replace('/', '_'));
                if (fx != null) Destroy(fx.gameObject);
            }
            CoverMuzzleFlash = null;
            Data.CoverWeaponData = null;
        }

        if (string.IsNullOrEmpty(weaponName)) return;
        
        Data.CoverWeaponData = XmlCacheManager.Weapons[weaponName];
        if (Data.ParticleBones != null && Data.ParticleBones.CoverMuzzleFlash != string.Empty && Data.CoverWeaponData.MuzzleFlashEffect != string.Empty)
        {
            var effectBone = transform.GetChildByName(Data.ParticleBones.CoverMuzzleFlash);
            if (effectBone == null)
                throw new FracturedStateException(Data.Name + " declares particle bone " + Data.ParticleBones.CoverMuzzleFlash + " that does not exist");

            var muzzleFx = DataUtil.LoadBuiltInParticleSystem(Data.CoverWeaponData.MuzzleFlashEffect);
            if (muzzleFx == null)
                throw new FracturedStateException(Data.WeaponData.Name + " declares muzzle flash " + Data.CoverWeaponData.MuzzleFlashEffect + " that does not exist");

            muzzleFx.transform.position = effectBone.position;
            muzzleFx.transform.rotation = effectBone.rotation;
            muzzleFx.transform.parent = effectBone;
            muzzleFx.SetLayerRecursively(GameConstants.ExteriorUnitLayer);
            CoverMuzzleFlash = muzzleFx.GetComponent<ParticleSystem>();
        }
    }

    /// <summary>
    /// Used by priority queues to determine which unit should take cover first
    /// </summary>
    public int CompareTo(UnitManager other)
    {
        if (Data.CoverPriority < other.Data.CoverPriority) return -1;
        if (Data.CoverPriority > other.Data.CoverPriority) return 1;
        return 0;
    }

    public override string ToString()
    {
        var sb = new StringBuilder(Data.Name);
        sb.Append("(").Append(NetMsg.netId).Append("); Mine: ");
        sb.Append(IsMine);
        sb.Append("; Nav State: ").Append(WorldState);
        sb.Append("; Current State: ");
        if (StateMachine?.CurrentState != null)
        {
            sb.Append(StateMachine.CurrentState);
        }
        else
        {
            sb.Append("null");
        }
        sb.Append("; Current Structure: ");
        if (CurrentStructure != null)
        {
            sb.Append(CurrentStructure.StructureData.Name);
            sb.Append("(").Append(CurrentStructure.GetComponent<Identity>().UID).Append(")");
        }
        else
        {
            sb.Append("null");
        }
        return sb.ToString();
    }
}