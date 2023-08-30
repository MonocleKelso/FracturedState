using System.Collections.Generic;
using System.Linq;
using FracturedState.Game.AI;
using FracturedState.Game.Data;
using FracturedState.Game.Management;
using FracturedState.Game.Modules;
using FracturedState.Game.Nav;
using FracturedState.Game.Network;
using FracturedState.UI;
using UnityEngine;
using Vectrosity;

namespace FracturedState.Game
{
    public enum SquadStance
    {
        Standard,
        HoldGround
    }
    
    public class Squad
    {
        public readonly List<UnitManager> Members;
        public Team Owner { get; private set; }
        protected PriorityQueue<UnitManager> CoverQueue;
        private Vector3[,] offsetGrid;
        private float offsetTotal;
        protected Dictionary<UnitManager, List<UnitManager>> UnitSightTable;
        public bool IsGettingCover { get; private set; }
        private List<CoverManager> nearbyCover;

        private VectorLine moveLine;
        private int lineUpdateCount;
        private Vector3 avgMove;

        public float MoveSpeed { get; private set; }
        public Vector3 LastMovePosition { get; private set; }

        private int combatUnits;

        public Quaternion Facing { get; private set; }
        public Vector3 FacingVector { get; set; }
        public bool UseFacing { get; private set; }
        public bool AttackMove { get; set; }
        public SquadStance Stance { get; set; }

        public bool IsIdle
        {
            get
            {
                for (var i = 0; i < Members.Count; i++)
                {
                    if (!Members[i].IsIdle)
                        return false;
                }
                return true;
            }
        }

        // empty ctor is used for garrison squads that don't initially have members
        protected Squad()
        {
            Members = new List<UnitManager>();
            CoverQueue = new PriorityQueue<UnitManager>();
            UnitSightTable = new Dictionary<UnitManager, List<UnitManager>>();
            nearbyCover = new List<CoverManager>();
        }

        public Squad(IEnumerable<UnitManager> units)
        {
            Members = new List<UnitManager>();
            Members.AddRange(units);
            Init();
        }

        private void Init()
        {
            CoverQueue = new PriorityQueue<UnitManager>();
            UnitSightTable = new Dictionary<UnitManager, List<UnitManager>>();
            nearbyCover = new List<CoverManager>();
            
            ApplySquadBuffs();
            
            if (Members[0].IsMine || Members[0].IsFriendly)
            {
                VisibilityChecker.Instance.RegisterSquad(this);
            }
            
            for (var i = 0; i < Members.Count; i++)
            {
                if (Members[i] != null)
                {
                    Members[i].SetSquad(this);
                    offsetTotal += Members[i].UnitRadius;
                    if (Members[i].Data.CanTakeCover)
                    {
                        CoverQueue.Enqueue(Members[i]);
                    }
                    MoveSpeed += Members[i].Data.Physics.MaxSpeed;
                }
            }
            offsetTotal *= 0.25f;
            MoveSpeed /= Members.Count;
            
            for (var i = 0; i < Members.Count; i++)
            {
                Members[i].LocoMotor = LocomotorFactory.Create(Members[i].Data.LocomotorName, Members[i]);
            }
        }

        private void ApplySquadBuffs()
        {
            // clear previous buffs and re-apply
            foreach (var member in Members)
            {
                if (member == null || !member.IsAlive) continue;

                var ab = member.GetPassiveAbilities();
                if (ab == null) continue;
                
                foreach (var a in ab)
                {
                    member.RemoveAbility(a.Name);
                }
            }

            // enumerate again to re-apply buffs
            foreach (var member in Members)
            {
                if (member == null) continue;
                
                var abilities = XmlCacheManager.Units[member.name].Abilities;
                if (abilities == null) continue;
                
                var passives = new List<Ability>();
                foreach (var ab in abilities)
                {
                    var a = XmlCacheManager.Abilities[ab];
                    if (a.Type == AbilityType.PassivePerSquad)
                    {
                        passives.Add(a);
                    }
                    else if (a.Type == AbilityType.PassivePerUnit)
                    {
                        member.AddAbility(a.Name);
                    }
                }
                
                if (passives.Count == 0) continue;

                foreach (var passive in passives)
                {
                    foreach (var m in Members)
                    {
                        m.AddAbility(passive.Name);
                    }
                }
            }
        }

        public void CalculateMoveSpeed()
        {
            MoveSpeed = 0;
            foreach (var member in Members)
            {
                if (member == null) continue;

                MoveSpeed += member.Data.Physics.MaxSpeed;
            }

            MoveSpeed /= Members.Count;
        }
        
        public Vector3 GetAveragePosition()
        {
            var pos = Vector3.zero;
            for (var i = 0; i < Members.Count; i++)
            {
                if (Members[i] != null)
                {
                    pos += Members[i].transform.position;
                }
            }
            pos /= Members.Count;
            return pos;
        }

        public void SetFacing(Quaternion facing)
        {
            UseFacing = true;
            Facing = facing;
        }

        public void SquadFacingMove(Vector3 destination, Quaternion facing)
        {
            SquadMove(destination);
            foreach (var member in Members)
            {
                if (member != null && member.NetMsg != null)
                {
                    member.NetMsg.CmdSetFacing(facing);
                    break;
                }
            }
        }

        public void SquadMove(Vector3 destination, bool bark = true)
        {
            UseFacing = false;
            LastMovePosition = destination;
            avgMove = Vector3.zero;
            var dest = destination;
            var targets = new List<UnitManager>();
            for (var i = 0; i < Members.Count; i++)
            {
                if (Members[i] != null && !Members[i].Stats.Rooted && Members[i].AcceptInput)
                {
                    if (Members[i].WorldState == Nav.State.Exterior)
                    {
                        var r = Random.insideUnitSphere;
                        r *= offsetTotal;
                        r.y = 0;
                        destination += r;
                    }

                    var t = Members[i].CurrentTarget;
                    if (t != null)
                    {
                        targets.Add(t);
                    }
                    Members[i].NetMsg.CmdMove(destination);
                    avgMove += Members[i].transform.position;
                }
            }

            if (Owner == FracNet.Instance.LocalTeam)
            {
                avgMove /= Members.Count;

                var retreat = false;
                if (targets.Count > Members.Count * 0.5)
                {
                    var targetPos = Vector3.zero;
                    for (var p = 0; p < targets.Count; p++)
                    {
                        targetPos += targets[p].transform.position;
                    }
                    targetPos /= targets.Count;
                    var toTarget = (targetPos - avgMove).normalized;
                    var toDest = (dest - avgMove).normalized;
                    if (Vector3.Dot(toDest, toTarget) < 0)
                    {
                        retreat = true;
                    }
                }
                if (bark)
                {
                    if (retreat)
                    {
                        UnitBarkManager.Instance.RetreatBark(Members[0].Data);
                    }
                    else
                    {
                        UnitBarkManager.Instance.MoveBark(Members[0].Data);
                    }
                }


                lineUpdateCount = 0;
            }
        }

        public void UpdateUnitPosition(UnitManager unit)
        {
            if (moveLine != null)
            {
                lineUpdateCount++;
                if (lineUpdateCount <= Members.Count)
                {
                    avgMove += unit.transform.position;
                }
                else
                {
                    avgMove /= Members.Count;
                    avgMove += Vector3.up;
                    moveLine.points3[0] = avgMove;
                    avgMove = Vector3.zero;
                    lineUpdateCount = 0;
                }
            }
        }

        public void StopUpdatingPositions()
        {
            if (moveLine != null)
            {
                LineManager.Instance.ReturnLine(moveLine);
                moveLine = null;
            }
        }

        public void SingleMove(UnitManager member, Vector3 destination)
        {
            member.NetMsg.CmdMove(destination);
        }

        public void SetOwner(Team owner)
        {
            Owner = owner;
            owner.Squads.Add(this);
        }

        public void InformSquadSelection(UnitManager clickedUnit, bool selected)
        {
            for (var i = 0; i < Members.Count; i++)
            {
                if (Members[i] != null && Members[i] != clickedUnit && Members[i].Data.IsSelectable)
                {
                    if (selected)
                    {
                        Members[i].OnSelected(false);
                    }
                    else
                    {
                        Members[i].OnDeSelected(false);
                    }
                }
            }
        }

        public void RemoveUnitFromCover(UnitManager unit)
        {
            if (unit != null && unit.IsAlive)
            {
                CoverQueue.Enqueue(unit);
            }
        }

        public void DetermineCover(List<CoverManager> covers, UnitManager caller, bool ignoreIdle = false)
        {
            if (covers.Count > 0)
            {
                for (var i = covers.Count - 1; i >= 0; i--)
                {
                    if (!covers[i].CanOccupy(caller))
                    {
                        covers.RemoveAt(i);
                    }
                }
                DetermineCover(covers, ignoreIdle);
            }
        }

        public void DetermineCover(Collider[] nearbyObjects, UnitManager caller, bool ignoreIdle = false)
        {
            var coverObjects = new List<CoverManager>();
            for (var i = 0; i < nearbyObjects.Length; i++)
            {
                var cm = nearbyObjects[i].gameObject.GetComponent<CoverManager>();
                if (cm != null && cm.CanOccupy(caller))
                {
                    coverObjects.Add(cm);
                }
            }
            DetermineCover(coverObjects, ignoreIdle);
        }

        private void DetermineCover(IReadOnlyList<CoverManager> coverObjects, bool ignoreIdle)
        {
            IsGettingCover = true;
            if (coverObjects.Count > 0)
            {
                var units = new List<UnitManager>();
                for (var i = 0; i < Members.Count; i++)
                {
                    var member = Members[i];
                    if (member != null && member.IsAlive && member.Data.CanTakeCover && !member.InCover && !member.StateMachine.IsCoverPrepped && (ignoreIdle || (!ignoreIdle && member.IsIdle)))
                    {
                        units.Add(member);
                    }
                }
                units.Sort();
                for (var i = 0; i < units.Count; i++)
                {
                    var bonus = 0;
                    float faceAngle = 2;
                    CoverManager cover = null;
                    Transform coverPoint = null;
                    for (var c = 0; c < coverObjects.Count; c++)
                    {
                        int b;
                        var t = coverObjects[c].ProcessOpenPoints(units[i], out b);
                        if (t != null)
                        {
                            if (b > bonus)
                            {
                                bonus = b;
                                cover = coverObjects[c];
                                coverPoint = t;
                            }
                            // grab closest point or point with closer facing if bonuses are equal
                            else if (b == bonus && coverPoint != null)
                            {
                                if (UseFacing)
                                {
                                    var a = Vector3.Dot(FacingVector, t.forward);
                                    if (1 - a < faceAngle)
                                    {
                                        cover = coverObjects[c];
                                        coverPoint = t;
                                        faceAngle = a;
                                    }
                                }
                                else
                                {
                                    var distToCurrent = (coverPoint.position - units[i].transform.position).sqrMagnitude;
                                    var distToNew = (t.position - units[i].transform.position).sqrMagnitude;
                                    if (distToNew < distToCurrent)
                                    {
                                        cover = coverObjects[c];
                                        coverPoint = t;
                                    }
                                }
                            }
                        }
                    }
                    if (cover != null)
                    {
                        var id = cover.gameObject.GetComponent<Identity>();
                        if (id != null)
                        {
                            cover.ReservePoint(coverPoint);
                            units[i].NetMsg.CmdTakeCover(id.UID, coverPoint.name);
                            units[i].StateMachine.ChangeState(new UnitPendingState(units[i]));
                        }
                    }
                }
            }
            IsGettingCover = false;
        }

        public void AddSquadUnit(UnitManager unit)
        {
            Members.Add(unit);
            unit.SetSquad(this);
            ApplySquadBuffs();
        }

        public virtual void RemoveSquadUnit(UnitManager unit)
        {
            Members.Remove(unit);
            CoverQueue.RemoveReorder(unit);
            var los = unit.GetComponentInChildren<UnitManager>();
            UnitSightTable.Remove(los);
            ApplySquadBuffs();
            if (Members.Count > 0) return;
            
            if (unit.IsMine)
            {
                VisibilityChecker.Instance.UnregisterSquad(this);
                CompassUI.Instance.RemoveSquad();
                ScreenEdgeNotificationManager.Instance.RemoveBattleNotification(this);
                UnitHotKeyManager.Instance.RemoveSquad(this);
            }
            else if (Owner != null)
            {
                Owner.Squads.Remove(this);
                if (unit.AISimulate)
                {
                    Owner.SquadPopulation--;
                }
            }
        }

        public void RegisterVisibleUnit(UnitManager unit, UnitManager enemy)
        {
            if (!UnitSightTable.ContainsKey(unit))
            {
                UnitSightTable[unit] = new List<UnitManager>();
            }

            UnitSightTable[unit].Add(enemy);
        }

        public void UnregisterVisibleUnit(UnitManager unit, UnitManager enemy)
        {
            if (UnitSightTable.ContainsKey(unit))
            {
                UnitSightTable[unit].Remove(enemy);
            }
        }

        public void UnregisterAllVisibleUnits(UnitManager unit)
        {
            if (UnitSightTable.ContainsKey(unit))
            {
                UnitSightTable[unit].Clear();
            }
        }

        public List<UnitManager> GetVisibleForUnit(UnitManager unit)
        {
            List<UnitManager> units;
            UnitSightTable.TryGetValue(unit, out units);
            return units;
        }

        public List<UnitManager> GetVisibleUnits()
        {
            var visibleUnits = new List<UnitManager>();
            var keys = new UnitManager[UnitSightTable.Keys.Count];
            UnitSightTable.Keys.CopyTo(keys, 0);
            foreach (var key in keys)
            {
                if (key == null) continue;

                List<UnitManager> units;
                if (UnitSightTable.TryGetValue(key, out units))
                {
                    // remove nulls and re-assign
                    for (var u = units.Count - 1; u >= 0; u--)
                    {
                        if (units[u] == null)
                        {
                            units.RemoveAt(u);
                        }
                    }

                    UnitSightTable[key] = units;
                    
                    foreach (var unit in units)
                    {
                        if (!visibleUnits.Contains(unit))
                        {
                            visibleUnits.Add(unit);
                        }
                    }
                }
            }
            return visibleUnits;
        }

        public void RegisterCoverObject(CoverManager cover)
        {
            nearbyCover.Add(cover);
        }

        public void UnregisterCoverObject(CoverManager cover)
        {
            nearbyCover.Remove(cover);
        }

        public CoverManager[] GetNearbyCover()
        {
            return nearbyCover.Distinct().ToArray();
        }

        // ensures everyone in this squad is either inside or outside
        // also ensures that at least one member of this squad has the ability
        private bool CheckSquadState(Ability ability)
        {
            var hasAbility = false;
            var state = Members[0].WorldState;
            for (var i = 1; i < Members.Count; i++)
            {
                if (Members[i].WorldState != state)
                    return false;

                if (ability.ConstrainType == Constraint.Interior && Members[i].WorldState != Nav.State.Interior)
                    return false;

                if (ability.ConstrainType == Constraint.Exterior && Members[i].WorldState != Nav.State.Exterior)
                    return false;
                
                if (Members[i].HasAbility(ability.Name))
                {
                    hasAbility = true;
                }
            }
            return hasAbility;
        }

        public void ExecuteSquadAbility(Ability ability)
        {
            if (CheckSquadState(ability))
            {
                var hit = default(RaycastHit);
                UnitManager target = null;
                if (ability.Targetting == TargetType.Ground)
                {
                    hit = RaycastUtil.RaycastTerrainAtMouse();
                }
                else if (ability.Targetting == TargetType.Enemy)
                {
                    target = RaycastUtil.RaycastEnemyAtMouse();
                }
                else if (ability.Targetting == TargetType.Friendly)
                {
                    target = RaycastUtil.RaycastFriendlyAtMouse();
                }

                for (var i = 0; i < Members.Count; i++)
                {
                    if (Members[i] != null && Members[i].IsAlive && !Members[i].IsMicroPrepped && !Members[i].IsMicroing)
                    {
                        if (ability.Targetting == TargetType.Ground)
                        {
                            Members[i].SetMicroState(new MicroUseAbilityState(Members[i], ability.Name, hit.point));
                        }
                        else if (ability.Targetting == TargetType.Enemy || ability.Targetting == TargetType.Friendly)
                        {
                            Members[i].SetMicroState(new MicroUseAbilityState(Members[i], ability.Name, target));
                        }
                        else if (ability.Targetting == TargetType.Structure)
                        {
                            var structure = RaycastUtil.RaycastStructureAtMouse();
                            Members[i].SetMicroState(new MicroUseAbilityState(Members[i], ability.Name, structure.transform.position));
                        }
                        else
                        {
                            Members[i].SetMicroState(new MicroUseAbilityState(Members[i], ability.Name));
                        }
                        Members[i].PropagateMicroState();
                    }
                }
            }
        }

        public void RegisterCombatUnit()
        {
            combatUnits++;
            if (combatUnits == 1)
            {
                ScreenEdgeNotificationManager.Instance.RequestBattleNotification(this);
            }
        }

        public void UnregisterCombatUnit()
        {
            combatUnits--;
            if (combatUnits < 0)
                combatUnits = 0;
            
            if (combatUnits == 0)
            {
                ScreenEdgeNotificationManager.Instance.RemoveBattleNotification(this);
            }
        }
    }
}