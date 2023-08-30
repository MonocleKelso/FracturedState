using UnityEngine;
using System.Collections.Generic;
using FracturedState.Game.Nav;
using FracturedState.Game.Data;
using Monocle.Threading;

namespace FracturedState.Game.AI
{
    public class MicroMoveState : MicroBaseState, IPathableState
    {
        public bool IsActive { get; protected set; }

        public Vector3 Destination { get; private set; }

        protected List<Vector3> path;
        protected int currentPathNode;
        protected bool pathSet;
        protected bool movePrepped;

        public MicroMoveState(UnitManager owner, Vector3 destination)
            : base(owner)
        {
            Destination = destination;
        }

        public MicroMoveState(UnitManager owner, List<Vector3> path)
            : base(owner)
        {
            this.path = AStarPather.SmoothPath(path, owner.Data.Physics.PathRadius, owner.WorldState);
            pathSet = true;
            movePrepped = true;
        }

        public virtual void SetPath(List<Vector3> path)
        {
            this.path = path;
            pathSet = true;
        }

        public override void Enter()
        {
            base.Enter();
            IsActive = true;
            PathRequest pr;
            if (owner.WorldState == Nav.State.Exterior)
            {
                pr = new PathRequest(this, owner.transform.position, Destination, owner.Data.Physics.PathRadius);
            }
            else
            {
                pr = new PathRequest(this, owner.CurrentStructure.NavigationGrid, owner.transform.position, Destination, owner.Data.Physics.PathRadius);
            }
            PathRequestManager.Instance.RequestPath(pr);
            currentPathNode = 0;
            owner.IsIdle = false;
            if (owner.InCover)
            {
                owner.RemoveFromCover();
            }
            if (owner.WorldState == Nav.State.Interior)
            {
                owner.ReturnFirePoint();
            }
            if (owner.AnimControl != null && owner.Data.Animations != null && owner.Data.Animations.Move != null && owner.Data.Animations.Move.Length > 0)
            {
                owner.AnimControl.Play(owner.Data.Animations.Move[Random.Range(0, owner.Data.Animations.Move.Length)], PlayMode.StopAll);
            }
        }

        public override void Execute()
        {
            if (!pathSet)
                return;

            if (!movePrepped)
            {
                path = AStarPather.SmoothPath(path, owner.Data.Physics.PathRadius, owner.WorldState);
                movePrepped = true;
            }

            if (path != null && currentPathNode < path.Count)
            {
                Vector3 steeringForce = SteeringBehaviors.CalculateUnitSteering(owner, path[currentPathNode]);
                owner.CurrentVelocity = Vector3.ClampMagnitude(owner.CurrentVelocity, owner.Squad.MoveSpeed);
                owner.transform.position += steeringForce * Time.deltaTime;
                owner.transform.LookAt(owner.transform.position + steeringForce);

                if ((owner.transform.position - path[currentPathNode]).sqrMagnitude <= ConfigSettings.Instance.Values.CloseEnoughThreshold)
                {
                    currentPathNode++;
                }
            }
            else
            {
                OnArrival();
            }
        }

        protected virtual void OnArrival()
        {
            owner.StateMachine.ChangeState(new MicroIdleState(owner));
        }

        public override void Exit()
        {
            base.Exit();
            IsActive = false;
            owner.CurrentVelocity = Vector3.zero;
        }
    }
}