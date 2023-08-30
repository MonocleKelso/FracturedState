using FracturedState.Game.Data;
using FracturedState.Game.Management;
using Monocle.Threading;
using System.Collections.Generic;
using UnityEngine;

namespace FracturedState.Game.AI
{
    /// <summary>
    /// An abstract base class that implements most of the necessary/common functionality for custom unit attack states. You can inherit from this in order
    /// to write less boilerplate code for combat music management and IPathableState implementation
    /// </summary>
    public abstract class CustomAttackState : CustomState, IPathableState
    {
        public bool IsActive { get; protected set; }

        protected UnitManager target;
        protected Weapon weapon;
        protected bool pathRequestSent;
        protected List<Vector3> path;
        protected int currentPoint;

        public CustomAttackState(AttackStatePackage initPackage) : base(initPackage)
        {
            weapon = owner.ContextualWeapon;
            target = initPackage.Target;
        }

        public override void Enter()
        {
            base.Enter();
            if (owner.IsMine)
            {
                MusicManager.Instance.AddCombatUnit();
                owner.Squad?.RegisterCombatUnit();
            }
            IsActive = true;
        }

        public override void Exit()
        {
            base.Exit();
            if (owner.IsMine)
            {
                MusicManager.Instance.RemoveCombatUnit();
                owner.Squad.UnregisterCombatUnit();
            }
            IsActive = false;
        }

        /// <summary>
        /// IPathableState implementation - sets this state's path to the one returned by the navigation thread
        /// </summary>
        /// <param name="path"></param>
        public void SetPath(List<Vector3> path)
        {
            this.path = path;
        }

        /// <summary>
        /// Moves the owner along it's navigation path that has been calculated to the target's position
        /// </summary>
        protected virtual void MoveOnPath()
        {
            if (path == null || currentPoint >= path.Count)
            {
                path = null;
                pathRequestSent = false;
                currentPoint = 0;
                return;
            }

            if (!owner.AnimControl.isPlaying)
            {
                owner.AnimControl.Play(owner.Data.Animations.Move[Random.Range(0, owner.Data.Animations.Move.Length)], PlayMode.StopAll);
            }

            var position = path[currentPoint];
            var speed = owner.Squad?.MoveSpeed ?? owner.Data.Physics.MaxSpeed;

            var steeringForce = SteeringBehaviors.CalculateUnitSteering(owner, position);
            owner.CurrentVelocity = Vector3.ClampMagnitude(owner.CurrentVelocity + steeringForce, speed);
            owner.transform.LookAt(owner.transform.position + owner.CurrentVelocity);
            owner.transform.position += owner.CurrentVelocity * Time.deltaTime;

            if (owner.IsMine)
            {
                owner.Squad?.UpdateUnitPosition(owner);
            }

            if ((owner.transform.position - path[currentPoint]).sqrMagnitude <= ConfigSettings.Instance.Values.CloseEnoughThreshold)
            {
                currentPoint++;
            }
        }

        /// <summary>
        /// Requests a navigable path to the owner's target
        /// </summary>
        protected virtual void CalcPathToTarget()
        {
            pathRequestSent = true;
            PathRequest pr;
            if (owner.WorldState == Nav.State.Exterior)
            {
                pr = new PathRequest(this, owner.transform.position, target.transform.position, owner.Data.Physics.PathRadius);
            }
            else
            {
                pr = new PathRequest(this, owner.CurrentStructure.NavigationGrid, owner.transform.position, target.transform.position, owner.Data.Physics.PathRadius);
            }
            PathRequestManager.Instance.RequestPath(pr);
        }

        /// <summary>
        /// Calculates the chance to hit against the given target for the owner's contextual weapon and
        /// either deals damage or tells the target to generate a miss effect
        /// </summary>
        /// <param name="t"></param>
        protected virtual void EvalTarget(UnitManager t)
        {
            var chanceToHit = Random.Range(0, 101);
            if (chanceToHit > owner.Stats.Accuracy)
            {
                t.NetMsg.RpcMiss(owner.NetMsg.NetworkId);
                owner.LastFiredTime = Time.time;
                return;
            }
            if (t.CurrentCover != null && t.CurrentCoverPoint != null)
            {
                var coverBonus = t.CurrentCoverPoint.GetBonus(t.transform, owner.transform.position);
                chanceToHit += coverBonus;
            }
            else
            {
                chanceToHit += t.GetMovementHitPenalty(owner.transform.forward);
            }

            if (chanceToHit < owner.Stats.Accuracy)
            {
                var mDamage = t.MitigateDamage(weapon, owner.transform.position);
                t.ProcessDamageInterrupts(mDamage, owner);
            }
            else
            {
                t.NetMsg.RpcMiss(owner.NetMsg.NetworkId);
                owner.LastFiredTime = Time.time;
            }
        }

        /// <summary>
        /// Checks if the target is still alive and still matches the owner's world state
        /// </summary>
        protected virtual bool CheckTargetEligibility()
        {
            // if the target has been destroyed or is dead
            if (target == null || !target.IsAlive)
                return false;

            // if the target changed world state
            if (owner.WorldState != target.WorldState)
                return false;

            return true;
        }

        /// <summary>
        /// Checks if the owner can see the target. If the owner cannot then determines a new target, changes states, and returns false. Otherwise returns true so
        /// normal state execution can continue
        /// </summary>
        protected virtual bool CheckTargetSight()
        {
            var hasSight = VisibilityChecker.Instance.HasSight(owner, target);
            if (!hasSight)
            {
                if (owner.IsMine)
                {
                    var newTarget = owner.DetermineTarget(null);
                    if (newTarget != null)
                    {
                        owner.NetMsg.CmdSetTarget(newTarget.GetComponent<UnityEngine.Networking.NetworkIdentity>());
                        owner.StateMachine.ChangeState(new UnitPendingState(owner));
                    }
                }
            }
            return hasSight;
        }
    }
}