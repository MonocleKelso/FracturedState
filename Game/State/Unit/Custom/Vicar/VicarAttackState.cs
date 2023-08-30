using FracturedState.Game.Data;
using FracturedState.Game.Management;
using Monocle.Threading;
using System.Collections.Generic;
using UnityEngine;

namespace FracturedState.Game.AI
{
    public class VicarAttackState : CustomState, IPathableState
    {
        private const string AttackSysName = "vicarAttackTotemAbsorb";
        private const string HealSysName = "vicarHealTotemGlow";
        private const string SiphonSysName = "Vicar/vicarAttack/vicarAttackSiphon";

        protected readonly UnitManager Target;
        protected bool HasSight;
        protected Weapon Weapon;

        protected bool PathRequestSent;
        protected List<Vector3> Path;
        protected int CurrentPoint;

        public bool IsActive { get; private set; }

        private ITargettingBehaviour targeter;
        private ParticleSystem attackSys;
        private ParticleSystem healSys;
        private ParticleSystem siphon;
        private bool deployed;
        private Transform siphonTarget;

        public VicarAttackState(AttackStatePackage initPackage) : base(initPackage)
        {
            Target = initPackage.Target;
        }

        public override void Enter()
        {
            base.Enter();
            if (owner.IsMine)
            {
                MusicManager.Instance.AddCombatUnit();
                owner.Squad?.RegisterCombatUnit();
            }
            owner.AnimControl.Stop();
            owner.AnimControl.Rewind();
            owner.IsIdle = false;
            Weapon = owner.ContextualWeapon;
            targeter = new MostDamagedSquadMate();
            attackSys = owner.transform.GetChildByName(AttackSysName).GetComponent<ParticleSystem>();
            healSys = owner.transform.GetChildByName(HealSysName).GetComponent<ParticleSystem>();
            siphonTarget = owner.transform.GetChildByName("totem_shaft002");
            IsActive = true;
        }

        public override void Execute()
        {
            if (Target != null && Target.IsAlive)
            {
                // if the target has changed world states then become idle
                if (owner.WorldState != Target.WorldState)
                {
                    owner.StateMachine.ChangeState(new UnitIdleState(owner));
                    return;
                }

                // if the unit loses sight of their target then find another target
                HasSight = VisibilityChecker.Instance.HasSight(owner, Target);
                if (!HasSight)
                {
                    owner.StateMachine.ChangeState(new UnitIdleState(owner));
                    return;
                }

                // determine if we're in range
                var inRange = (Target.transform.position - owner.transform.position).sqrMagnitude <= Weapon.Range * Weapon.Range;
                var canFire = owner.LastFiredTime + Weapon.FireRate < Time.time;

                if (inRange)
                {
                    owner.transform.LookAt(new Vector3(Target.transform.position.x, 0, Target.transform.position.z));
                    if (!deployed)
                    {
                        owner.AnimControl.Play("Deploy", PlayMode.StopAll);
                        deployed = true;
                        var targetTransform = Target.WeaponHitBone != null ? Target.WeaponHitBone : Target.transform;
                        siphon = DataUtil.LoadBuiltInParticleSystem(SiphonSysName).GetComponent<ParticleSystem>();
                        siphon.gameObject.SetLayerRecursively(Target.gameObject.layer);
                        siphon.transform.position = targetTransform.position;
                        siphon.GetComponent<particleAttractorMove>().target = siphonTarget;
                    }
                    
                    if (!attackSys.isPlaying)
                    {
                        attackSys.Play();
                    }
                    
                    if (siphon != null)
                    {
                        if (Target != null)
                        {
                            var point = Target.WeaponHitBone != null ? Target.WeaponHitBone : Target.transform;
                            siphon.transform.position = point.position;
                        }
                        else
                        {
                            Object.Destroy(siphon.gameObject, 2);
                        }
                    }
                    
                    if (canFire)
                    {
                        Shoot();
                    }
                }
                else if (owner.Squad.Stance == SquadStance.Standard)
                {
                    if (Path != null)
                    {
                        deployed = false;
                        if (CurrentPoint < Path.Count)
                        {
                            MoveOnPath();
                        }
                        else
                        {
                            Path = null;
                        }
                    }
                    else
                    {
                        if (!PathRequestSent)
                        {
                            CalcPathToTarget();
                        }
                    }
                }
                else
                {
                    owner.StateMachine.ChangeState(new UnitIdleState(owner));
                }
            }
            else
            {
                owner.StateMachine.ChangeState(new UnitIdleState(owner));
            }
        }

        protected void Shoot()
        {
            owner.LastFiredTime = Time.time;
            if (Network.FracNet.Instance.IsHost)
            {
                // deal damage and find injured squad mate to heal
                Target.NetMsg.CmdTakeDamage(Weapon.Damage, owner.NetMsg.NetworkId, Weapon.Name);
                var healTarget = targeter.FindTarget(owner);
                if (healTarget != null)
                {
                    healTarget.NetMsg.CmdApplyBuff((int)BuffType.None, 0, 1, "Vicar/vicarHeal/VicarHealShort");
                    healTarget.NetMsg.CmdHeal(Mathf.RoundToInt(Weapon.Damage * 0.5f));
                }
            }
        }

        protected void CalcPathToTarget()
        {
            PathRequestSent = true;
            PathRequest pr;
            if (owner.WorldState == Nav.State.Exterior)
            {
                pr = new PathRequest(this, owner.transform.position, Target.transform.position, owner.Data.Physics.PathRadius);
            }
            else
            {
                pr = new PathRequest(this, owner.CurrentStructure.NavigationGrid, owner.transform.position, Target.transform.position, owner.Data.Physics.PathRadius);
            }
            PathRequestManager.Instance.RequestPath(pr);
        }

        protected void MoveOnPath()
        {
            if (owner.AnimControl != null && !owner.AnimControl.isPlaying && owner.Data.Animations?.Move != null && owner.Data.Animations.Move.Length > 0)
            {
                owner.AnimControl.Play(owner.Data.Animations.Move[Random.Range(0, owner.Data.Animations.Move.Length)], PlayMode.StopAll);
            }

            if (siphon != null)
            {
                Object.Destroy(siphon.gameObject, 2);
                siphon = null;
            }

            var position = Path[CurrentPoint];
            var speed = owner.Squad?.MoveSpeed ?? owner.Data.Physics.MaxSpeed;

            var steeringForce = SteeringBehaviors.CalculateUnitSteering(owner, position);
            owner.CurrentVelocity = Vector3.ClampMagnitude(owner.CurrentVelocity + steeringForce, speed);
            owner.transform.LookAt(owner.transform.position + owner.CurrentVelocity);
            owner.transform.position += owner.CurrentVelocity * Time.deltaTime;

            if (owner.IsMine)
            {
                owner.Squad?.UpdateUnitPosition(owner);
            }

            if ((owner.transform.position - Path[CurrentPoint]).sqrMagnitude <= ConfigSettings.Instance.Values.CloseEnoughThreshold)
            {
                CurrentPoint++;
            }
        }

        public void SetPath(List<Vector3> path)
        {
            this.Path = path;
        }

        public override void Exit()
        {
            base.Exit();
            if (attackSys.isPlaying)
            {
                attackSys.Stop();
            }
            if (healSys.isPlaying)
            {
                healSys.Stop();
            }
            if (siphon != null)
            {
                siphon.Stop();
                Object.Destroy(siphon.gameObject, 2);
            }
            IsActive = false;
            if (owner.IsMine)
            {
                MusicManager.Instance.RemoveCombatUnit();
                owner.Squad.UnregisterCombatUnit();
            }
        }
    }
}