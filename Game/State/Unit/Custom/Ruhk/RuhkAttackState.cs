using FracturedState.Game.Data;
using FracturedState.Game.Management;
using FracturedState.Game.Network;
using UnityEngine;

namespace FracturedState.Game.AI
{
    public class RuhkAttackState : CustomState
    {
        public const float LeapMaxDistance = 15;
        public const float LeapMinDistance = 5;
        public const string LeapAnimation = "idleCharge";
        public const string LeapAnimationMoving = "runCharge";
        public const string LeapFinishAnimation = "chargeLand";
        public const float LeapSpeed = 15;

        private Weapon weapon;
        private readonly UnitManager target;

        public RuhkAttackState(AttackStatePackage initPackage) : base(initPackage)
        {
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
            weapon = owner.ContextualWeapon;
            owner.AnimControl.Stop();
            owner.AnimControl.Rewind();
            owner.IsIdle = false;
        }

        public override void Execute()
        {
            if (target != null && target.IsAlive)
            {
                // if the target has changed world states then become idle
                if (owner.WorldState != target.WorldState)
                {
                    owner.StateMachine.ChangeState(new UnitIdleState(owner));
                    return;
                }

                // if the unit loses sight of their target then find another target
                var hasSight = VisibilityChecker.Instance.HasSight(owner, target);
                if (!hasSight)
                {
                    if (owner.IsMine)
                    {
                        var newTarget = owner.DetermineTarget(null);
                        if (newTarget != null)
                        {
                            owner.NetMsg.CmdSetTarget(newTarget.NetMsg.NetworkId);
                            owner.StateMachine.ChangeState(new UnitPendingState(owner));
                        }
                    }
                    return;
                }

                owner.transform.LookAt(target.transform.position);
                // has enough time elapsed for us to fire
                var canFire = owner.LastFiredTime + weapon.FireRate < Time.time;
                
                if (canFire)
                {
                    // determine range to target to see if we need to move and if so, how
                    var dist = (owner.transform.position - target.transform.position).magnitude;
                    // if we're out of range then use the charge state to either move or charge into position
                    if (dist > weapon.Range)
                    {
                        var toTarget = owner.transform.position - target.transform.position;
                        toTarget = target.transform.position + toTarget.normalized * (weapon.Range * 0.8f);
                        owner.StateMachine.ChangeState(new RuhkCharge(owner, toTarget, target));
                    }
                    // otherwise attack targets
                    else
                    {
                        Slam();
                    }
                }
            }
            else
            {
                if (!owner.AnimControl.isPlaying)
                {
                    owner.StateMachine.ChangeState(new UnitIdleState(owner));
                }
            }
        }

        private void Slam()
        {
            owner.LastFiredTime = Time.time;
            owner.AnimControl.Play(owner.Data.Animations.StandFire[Random.Range(0, owner.Data.Animations.StandFire.Length)], PlayMode.StopAll);
            if (FracNet.Instance.IsHost)
            {
                var mask = owner.WorldState == Nav.State.Exterior ? GameConstants.ExteriorUnitAllMask : GameConstants.InteriorUnitAllMask;
                var nearby = Physics.OverlapSphere(owner.transform.position, weapon.Range, mask);
                for (var i = 0; i < nearby.Length; i++)
                {
                    var unit = nearby[i].GetComponent<UnitManager>();
                    if (unit != null && unit.IsAlive && (weapon.DamagesFriendly || unit.OwnerTeam != owner.OwnerTeam))
                    {
                        // make sure target is in front of unit
                        var toTarget = (unit.transform.position - owner.transform.position).normalized;
                        if (Vector3.Dot(owner.transform.forward, toTarget) > 0)
                        {
                            EvalTarget(unit);
                        }
                    }
                }
            }
        }

        private void EvalTarget(UnitManager t)
        {
            if (t.Transport != null) return;

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
                // halve damage for secondary targets
                if (t != target)
                {
                    mDamage = Mathf.RoundToInt(mDamage * 0.5f);
                }
                t.ProcessDamageInterrupts(mDamage, owner);
            }
            else
            {
                t.NetMsg.RpcMiss(owner.NetMsg.NetworkId);
                owner.LastFiredTime = Time.time;
            }
        }

        public override void Exit()
        {
            base.Exit();
            if (owner.IsMine)
            {
                MusicManager.Instance.RemoveCombatUnit();
                owner.Squad.UnregisterCombatUnit();
            }
        }
    }
}