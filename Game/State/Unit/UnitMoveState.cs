using System.Collections.Generic;
using FracturedState.Game.Management;
using FracturedState.Game.Nav;
using Monocle.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace FracturedState.Game.AI
{
    public class UnitMoveState : UnitBaseState, IPathableState
    {
        public bool IsActive { get; protected set; }

        protected Vector3 Destination;
        protected List<Vector3> MovePath;
        protected int CurrentPathNode;
        protected bool PathSet;
        protected bool MovePrepped;

        private int? endId;

        protected UnitMoveState(UnitManager owner) : base(owner) { }

        public UnitMoveState(UnitManager owner, Vector3 destination)
            : base(owner)
        {
            Destination = destination;
            CurrentPathNode = 0;
        }

        public virtual void SetPath(List<Vector3> path)
        {
            MovePath = path;
            PathSet = true;
        }

        public override void Enter()
        {
            // if unit can't move because it has no locomotor then just exit
            if (Owner.LocoMotor == null)
            {
                Owner.StateMachine.ChangeState(new UnitIdleState(Owner));
                return;
            }

            if (Owner.WorldState == Nav.State.Exterior)
            {
                var endHit = RaycastUtil.RaycastTerrain(Destination + Vector3.up);
                if (endHit.transform != null)
                {
                    endId = endHit.transform.gameObject.GetInstanceID();
                }
            }

            // if the unit has a custom ove state then use it instead
            if (!string.IsNullOrEmpty(Owner.Data.CustomBehaviours?.MoveClassName))
            {
                var customState = CustomStateFactory<MoveStatePackage>.Create(Owner.Data.CustomBehaviours.MoveClassName,
                    new MoveStatePackage(Owner, Destination));
                Owner.StateMachine.ChangeState(customState);
                return;
            }
            
            IsActive = true;
            var grid = (Owner.CurrentStructure != null) ? Owner.CurrentStructure.UnitPlacementGrid : AStarPather.Instance.ExteriorGrid;
            grid.OpenPoint(Owner);

            PathRequest pathRequest = null;
            if (Owner.WorldState == Nav.State.Exterior)
            {
                pathRequest = new PathRequest(this, Owner.transform.position, Destination, Owner.Data.Physics.PathRadius);
            }
            else if (Owner.CurrentStructure != null)
            {
                pathRequest = new PathRequest(this, Owner.CurrentStructure.NavigationGrid, Owner.transform.position, Destination, Owner.Data.Physics.PathRadius);
            }
            
            if (pathRequest != null) PathRequestManager.Instance.RequestPath(pathRequest);
        }

        public override void Execute()
        {
            if (!PathSet)
                return;

            if (!MovePrepped)
            {
                if (MovePath != null)
                {
                    if (Owner.WorldState == Nav.State.Exterior)
                    {
                        MovePath = AStarPather.SmoothPath(MovePath, Owner.Data.Physics.PathRadius, Owner.WorldState);
                    }
                    Owner.IsIdle = false;
                    if (Owner.InCover)
                    {
                        Owner.RemoveFromCover();
                    }
                    Owner.EffectManager?.PlayMoveSystems();
                    if (Owner.AnimControl != null && Owner.Data.Animations?.Move != null && Owner.Data.Animations.Move.Length > 0)
                    {
                        var anim = Owner.Data.Animations.Move[Random.Range(0, Owner.Data.Animations.Move.Length)];
                        var len = Owner.AnimControl[anim].length;
                        Owner.AnimControl[anim].time = Random.Range(0, len);
                        Owner.AnimControl.Play(anim);
                    }
                }
                else
                {
                    // hacky fix for units getting stuck on the edge of the map when recruited
                    if (!RaycastUtil.IsUnderTerrain(Owner.transform.position))
                    {
                        MovePath = new List<Vector3> {Destination};
                    }
                    else
                    {
                        Owner.StateMachine.ChangeState(new UnitIdleState(Owner));
                    }
                }
                MovePrepped = true;
            }
            
            if (MovePath != null && CurrentPathNode < MovePath.Count)
            {
                // if our destination has another unit standing on it then pick a new destination
                if ((Owner.IsMine || Owner.AISimulate) && Owner.WorldState == Nav.State.Exterior)
                {
                    var c = AStarPather.Instance.ExteriorGrid.GetClosestPoint(Destination,
                        Owner.Data.Physics.PathRadius, endId);
                    if (c.IsOccupied)
                    {
                        var newDest = AStarPather.Instance.ExteriorGrid.GetClosestFreePoint(c);
                        Owner.StateMachine.ChangeState(new UnitMoveState(Owner, newDest.WorldPosition));
                    }
                }
                
                InfantrySpace();
                CurrentPathNode = Owner.LocoMotor.MoveOnPath(MovePath, CurrentPathNode);
                
                if (Owner.IsMine && Owner.Squad != null)
                {
                    Owner.Squad.UpdateUnitPosition(Owner);
                    if (Owner.Squad.AttackMove)
                    {
                        AttackMoveEnemySearch();
                    }
                }
            }
            else if (MovePath != null)
            {
                OnArrival();
            }
        }

        protected virtual void AttackMoveEnemySearch()
        {
            if (Owner.ContextualWeapon == null) return;
            
            UnitManager target = null;
            var dist = float.MaxValue;
            var visible = Owner.Squad.GetVisibleForUnit(Owner);
            if (visible == null) return;
                
            for (var i = 0; i < visible.Count; i++)
            {
                if (visible[i] != null && visible[i].IsAlive &&
                    VisibilityChecker.Instance.HasSight(Owner, visible[i]))
                {
                    var toUnit = (visible[i].transform.position - Owner.transform.position);
                    if (!(toUnit.sqrMagnitude < dist)) continue;
                            
                    target = visible[i];
                    dist = toUnit.sqrMagnitude;
                }
            }
            if (target != null && target.NetMsg != null)
            {
                Owner.NetMsg.CmdSetTarget(target.NetMsg.NetworkId);
                Owner.StateMachine.ChangeState(new UnitPendingState(Owner));
            }
        }

        /// <summary>
        /// Moves a unit to an open point on its navigation grid in order to prevent unit overlapping
        /// This fires a single move delegate which triggers a new MoveState. Ultimately the unit goes
        /// idle once a free point has been reached
        /// </summary>
        protected virtual void OccupyOpenGround()
        {
            var grid = (Owner.CurrentStructure != null) ? Owner.CurrentStructure.UnitPlacementGrid : AStarPather.Instance.ExteriorGrid;
            var closestPoint = grid.GetClosestPoint(Owner.transform.position, Owner.Data.Physics.PathRadius);

            if (!closestPoint.IsOccupied)
            {
                grid.OccupyPoint(Owner, closestPoint);
                if (Owner.Squad.UseFacing)
                {
                    Owner.StateMachine.ChangeState(new UnitApplyFacingState(Owner));
                }
                else
                {
                    Owner.StateMachine.ChangeState(new UnitIdleState(Owner));
                }
            }
            else
            {
                closestPoint = grid.GetClosestFreePoint(closestPoint);
                if (Owner.WorldState == Nav.State.Exterior)
                {
                    Owner.StateMachine.ChangeState(new UnitMoveState(Owner, closestPoint.WorldPosition));
                }
                else
                {
                    Owner.transform.position = closestPoint.WorldPosition;
                    grid.OccupyPoint(Owner, closestPoint);
                    if (Owner.Squad.UseFacing)
                    {
                        Owner.StateMachine.ChangeState(new UnitApplyFacingState(Owner));
                    }
                    else
                    {
                        Owner.StateMachine.ChangeState(new UnitIdleState(Owner));
                    }
                }
            }
        }

        protected virtual void OnArrival()
        {
            OccupyOpenGround();
        }

        public override void Exit()
        {
            IsActive = false;
            Owner.LocoMotor?.ZeroVelocity();
            if (Owner.IsMine)
            {
                Owner.Squad?.StopUpdatingPositions();
            }
            Owner.EffectManager?.StopCurrentSystem();
            base.Exit();
        }
    }
}