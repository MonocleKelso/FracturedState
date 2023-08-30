using System.Collections.Generic;
using FracturedState.Game.Management;
using FracturedState.Game.Nav;
using Monocle.Threading;
using UnityEngine;

namespace FracturedState.Game.AI
{
    public class ShrineMoveState : CustomState, IPathableState
    {
        public bool IsActive { get; private set; }
        private Vector3 destination;
        private List<Vector3> movePath;
        private int currentPathNode;
        private bool pathSet;
        private bool prepped;
        
        public ShrineMoveState(MoveStatePackage initPackage) : base(initPackage)
        {
            destination = initPackage.Destination;
        }

        public override void Enter()
        {
            IsActive = true;
            
            AStarPather.Instance.ExteriorGrid.OpenPoint(owner);
            var pathRequest = new PathRequest(this, owner.transform.position, destination, owner.Data.Physics.PathRadius);
            PathRequestManager.Instance.RequestPath(pathRequest);
            
            owner.transform.GetChildByName("shrineOpenL").GetComponent<ParticleSystem>().Stop();
            owner.transform.GetChildByName("shrineOpenR").GetComponent<ParticleSystem>().Stop();

            if (!owner.AnimControl.IsPlaying("Fire") && !owner.AnimControl.IsPlaying("Run"))
            {
                owner.AnimControl["Fire"].speed = -1;
                owner.AnimControl["Fire"].time = owner.AnimControl["Fire"].length;
                owner.AnimControl.Play("Fire");
            }
        }

        public override void Execute()
        {
            if (owner.AnimControl.IsPlaying("Fire"))
            {
                return;
            }
            else
            {
                owner.AnimControl["Fire"].speed = 1;
            }

            if (!pathSet) return;

            if (!prepped)
            {
                movePath = AStarPather.SmoothPath(movePath, owner.Data.Physics.PathRadius, owner.WorldState);
                var anim = owner.Data.Animations.Move[Random.Range(0, owner.Data.Animations.Move.Length)];
                var len = owner.AnimControl[anim].length;
                owner.AnimControl[anim].time = Random.Range(0, len);
                owner.AnimControl.Play(anim);
                prepped = true;
            }
            
            if (movePath != null && currentPathNode < movePath.Count)
            {
                currentPathNode = owner.LocoMotor.MoveOnPath(movePath, currentPathNode);
                
                if (owner.IsMine && owner.Squad != null)
                {
                    owner.Squad.UpdateUnitPosition(owner);
                    if (owner.Squad.AttackMove)
                    {
                        AttackMoveEnemySearch();
                    }
                }
            }
            else if (movePath != null)
            {
                OnArrival();
            }
        }

        private void OnArrival()
        {
            var closestPoint = AStarPather.Instance.ExteriorGrid.GetClosestPoint(owner.transform.position, owner.Data.Physics.PathRadius);
            
            if (!closestPoint.IsOccupied)
            {
                AStarPather.Instance.ExteriorGrid.OccupyPoint(owner, closestPoint);
                if (owner.Squad.UseFacing)
                {
                    owner.StateMachine.ChangeState(new UnitApplyFacingState(owner));
                }
                else
                {
                    owner.StateMachine.ChangeState(new UnitIdleState(owner));
                }
            }
            else
            {
                closestPoint = AStarPather.Instance.ExteriorGrid.GetClosestFreePoint(closestPoint);
                if (owner.WorldState == Nav.State.Exterior)
                {
                    owner.StateMachine.ChangeState(new UnitMoveState(owner, closestPoint.WorldPosition));
                }
                else
                {
                    owner.transform.position = closestPoint.WorldPosition;
                    AStarPather.Instance.ExteriorGrid.OccupyPoint(owner, closestPoint);
                    if (owner.Squad.UseFacing)
                    {
                        owner.StateMachine.ChangeState(new UnitApplyFacingState(owner));
                    }
                    else
                    {
                        owner.StateMachine.ChangeState(new UnitIdleState(owner));
                    }
                }
            }
        }

        private void AttackMoveEnemySearch()
        {
            if (owner.ContextualWeapon == null) return;
            
            UnitManager target = null;
            var dist = float.MaxValue;
            var visible = owner.Squad.GetVisibleForUnit(owner);
            if (visible == null) return;
                
            for (var i = 0; i < visible.Count; i++)
            {
                if (visible[i] != null && visible[i].IsAlive &&
                    VisibilityChecker.Instance.HasSight(owner, visible[i]))
                {
                    var toUnit = (visible[i].transform.position - owner.transform.position);
                    if (!(toUnit.sqrMagnitude < dist)) continue;
                            
                    target = visible[i];
                    dist = toUnit.sqrMagnitude;
                }
            }
            if (target != null && target.NetMsg != null)
            {
                owner.NetMsg.CmdSetTarget(target.NetMsg.NetworkId);
                owner.StateMachine.ChangeState(new UnitPendingState(owner));
            }
        }
        
        public void SetPath(List<Vector3> path)
        {
            movePath = path;
            pathSet = true;
        }
    }
}