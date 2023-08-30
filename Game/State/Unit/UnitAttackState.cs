using FracturedState.Game.Data;
using FracturedState.Game.Management;
using FracturedState.Game.Modules;
using FracturedState.Game.Network;
using UnityEngine;
using UnityEngine.Networking;

namespace FracturedState.Game.AI
{
    public class UnitAttackState : UnitBaseState
    {
        public bool IsActive { get; protected set; }

        protected UnitManager target;
        protected Transform targetCoverPoint;
        protected Weapon weapon;
        protected AudioClip[] shootSounds;
        protected bool aimed;
        protected bool hasSight;

        public UnitManager Target => target;

        public UnitAttackState(UnitManager owner, UnitManager target)
            : base(owner)
        {
            this.target = target;
            weapon = this.Owner.ContextualWeapon;
        }

        public override void Enter()
        {
            // switch to custom attack state if one is declared
            if (Owner.Transport == null && !string.IsNullOrEmpty(Owner.Data.CustomBehaviours?.AttackClassName) && Owner.StateMachine.CurrentState == this)
            {
                var state = CustomStateFactory<AttackStatePackage>.Create(Owner.Data.CustomBehaviours.AttackClassName, new AttackStatePackage(Owner, target));
                Owner.StateMachine.ChangeState(state);
                return;
            }

            IsActive = true;
            if (weapon == null)
            {
                Owner.StateMachine.ChangeState(new UnitIdleState(Owner));
                return;
            }

            if (Owner.IsMine && Owner.Data.IsSelectable)
            {
                MusicManager.Instance.AddCombatUnit();
                Owner.Squad?.RegisterCombatUnit();
            }
            if (Owner.AnimControl != null)
            {
                Owner.AnimControl.Stop();
                Owner.AnimControl.Rewind();
            }
            if (weapon.SoundEffects != null && weapon.SoundEffects.Length > 0)
            {
                shootSounds = new AudioClip[weapon.SoundEffects.Length];
                for (int i = 0; i < weapon.SoundEffects.Length; i++)
                {
                    shootSounds[i] = DataUtil.LoadBuiltInSound(weapon.SoundEffects[i]);
                }
            }
            aimed = Owner.InCover;
            Owner.IsIdle = false;
            hasSight = true;
        }

        public override void Execute()
        {
            if (target != null && target.IsAlive)
            {
                // if the target has changed world states then become idle
                if (Owner.WorldState != target.WorldState)
                {
                    GoIdle();
                    return;
                }

                // if the unit loses sight of their target then go idle
                hasSight = VisibilityChecker.Instance.HasSight(Owner, target);
                if (!hasSight && Owner.Data.WeaponNeedsSight)
                {
                    GoIdle();
                }

                var inRange = (target.transform.position - Owner.transform.position).sqrMagnitude <= weapon.Range * weapon.Range;
                var canFire = Owner.LastFiredTime + weapon.FireRate < Time.time;

                if (inRange)
                {
                    if (!aimed && Owner.AnimControl != null && Owner.Data.Animations?.CrouchAim != null && Owner.Data.Animations.CrouchAim.Length > 0)
                    {
                        Owner.AnimControl.Play(Owner.Data.Animations.CrouchAim[Random.Range(0, Owner.Data.Animations.CrouchAim.Length)], PlayMode.StopAll);
                        aimed = true;
                    }

                    if (!Owner.Data.IsTransport)
                    {
                        Owner.transform.LookAt(new Vector3(target.transform.position.x, 0, target.transform.position.z));
                    }
                    
                    InfantrySpace();

                    if (canFire)
                    {
                        Shoot();
                    }
                }
                else if (!Owner.InCover && !Owner.Data.IsGarrisonUnit)
                {
                    if (Owner.Squad.Stance == SquadStance.Standard)
                    {
                        Owner.StateMachine.ChangeState(new UnitMoveToAttackState(Owner, target));
                    }
                    else
                    {
                        Owner.StateMachine.ChangeState(new UnitIdleState(Owner));
                    }
                    
                }
            }
            else
            {
                GoIdle();
            }
        }

        private void GoIdle()
        {
            if (Owner.InCover)
            {
                Owner.StateMachine.ChangeState(new UnitIdleCoverState(Owner));
            }
            else if (Owner.Data.IsGarrisonUnit)
            {
                Owner.StateMachine.ChangeState(new UnitGarrisonIdleState(Owner));
            }
            else
            {
                Owner.StateMachine.ChangeState(new UnitIdleState(Owner));
            }
        }
        
        protected virtual void Shoot()
        {   
            if (Owner.AnimControl != null)
            {
                var clip = "";
                if (Owner.InCover)
                {
                    var clips = (Owner.CurrentCoverPoint.Stance == CoverPointStance.Stand) ? Owner.Data.Animations.StandFire : Owner.Data.Animations.CrouchFire;
                    if (clips != null && clips.Length > 0)
                    {
                        clip = clips[Random.Range(0, clips.Length)];
                    }
                }
                else if (Owner.Data.Animations?.CrouchFire != null && Owner.Data.Animations.CrouchFire.Length > 0)
                {
                    clip = Owner.Data.Animations.CrouchFire[Random.Range(0, Owner.Data.Animations.CrouchFire.Length)];
                }

                if (clip != "")
                {
                    Owner.AnimControl.Play(clip, PlayMode.StopAll);
                }
            }
            
            if (Owner.Data.WeaponFireDelay > 0)
            {
                Owner.LastFiredTime = Time.time;
                Owner.StartCoroutine(Owner.RunDelayedAction(ExecuteShoot, this, Owner.Data.WeaponFireDelay));
            }
            else
            {
                ExecuteShoot();
            }
        }

        protected void ExecuteShoot()
        {
            if (Owner.ContextualMuzzleFlash != null)
            {
                Owner.ContextualMuzzleFlash.Play();
            }
            if (shootSounds != null)
            {
                var audio = Owner.GetComponent<AudioSource>();
                audio.clip = shootSounds[Random.Range(0, shootSounds.Length)];
                audio.pitch = Random.Range(0.80f, 1.2f);
                audio.Play();
            }
            Owner.LastFiredTime = Time.time;

            if (weapon.ProjectileData != null)
            {
                GameObject proj;
                if (!string.IsNullOrEmpty(weapon.ProjectileData.Model))
                {
                    proj = ObjectPool.Instance.GetPooledModelAtLocation(weapon.ProjectileData.Model, Owner.transform.position);
                }
                else
                {
                    proj = new GameObject(weapon.Name);
                    proj.transform.position = Owner.transform.position;
                }
                proj.transform.position = Owner.ContextualMuzzleFlash != null ? Owner.ContextualMuzzleFlash.transform.position : Owner.transform.position;
                proj.transform.rotation = Owner.transform.rotation;
                
                var layer = Owner.WorldState == Nav.State.Exterior ? GameConstants.ExteriorLayer : GameConstants.InteriorLayer;
                proj.SetLayerRecursively(layer);
                
                var pb = proj.GetComponent<ProjectileBehaviour>();
                if (pb == null) pb = proj.AddComponent<ProjectileBehaviour>();
                
                pb.Init(weapon, Owner, target);
            }

            if (FracNet.Instance.IsHost && weapon.ProjectileData == null)
            {
                if (weapon.MuzzleRadius > 0)
                {
                    var mask = Owner.WorldState == Nav.State.Exterior ? GameConstants.ExteriorUnitAllMask : GameConstants.InteriorUnitAllMask;
                    var dir = target.transform.position - Owner.transform.position;
                    var hits = Physics.CapsuleCastAll(Owner.transform.position, target.transform.position, weapon.MuzzleRadius, dir, weapon.Range, mask);
                    if (hits == null) return;
                    
                    foreach (var hit in hits)
                    {
                        var unit = hit.transform.GetAbsoluteParent().GetComponent<UnitManager>();
                        if (unit != null && unit.IsAlive && unit.OwnerTeam != Owner.OwnerTeam)
                        {
                            EvalTarget(unit);
                        }
                    }
                }
                else
                {
                    EvalTarget(target);
                }
            }
        }

        protected void EvalTarget(UnitManager t)
        {
            if (BlockerCheck(t)) return;
            
            if (weapon.PointBlankRange > 0 && (Owner.transform.position - t.transform.position).magnitude < weapon.PointBlankRange)
            {
                DealDamage(t);
                return;
            }

            int chanceToHit = Random.Range(0, 101);
            if (chanceToHit > Owner.Stats.Accuracy)
            {
                t.NetMsg.RpcMiss(Owner.NetMsg.NetworkId);
                Owner.LastFiredTime = Time.time;
                return;
            }

            if (t.CurrentCover != null && t.CurrentCoverPoint != null)
            {
                if (targetCoverPoint == null || targetCoverPoint.name != t.CurrentCoverPoint.Name)
                {
                    for (var i = 0; i < t.CurrentCover.CoverPoints.Length; i++)
                    {
                        if (t.CurrentCover.CoverPoints[i].name == t.CurrentCoverPoint.Name)
                            targetCoverPoint = t.CurrentCover.CoverPoints[i];
                    }
                }
                int coverBonus = t.CurrentCoverPoint.GetBonus(targetCoverPoint, Owner.transform.position);
                chanceToHit += coverBonus;
            }
            else
            {
                chanceToHit += t.GetMovementHitPenalty(Owner.transform.forward);
            }

            if (chanceToHit < Owner.Stats.Accuracy)
            {
                DealDamage(t);
            }
            else
            {
                t.NetMsg.RpcMiss(Owner.NetMsg.NetworkId);
                Owner.LastFiredTime = Time.time;
            }
        }

        protected bool BlockerCheck(UnitManager target)
        {
            RaycastHit hit = RaycastUtil.RayCheckWeaponBlock(Owner.transform.position, target.transform.position);
            if (hit.collider == null) return false;
            var damageModule = hit.collider.GetComponentInParent<NetworkDamageModule>();
            if (damageModule == null) return false;
            var dOwner = damageModule.GetOwner();
            // let units on the same team shoot through blockers
            if (dOwner != null && dOwner.OwnerTeam == Owner.OwnerTeam) return false;
            damageModule.TakeDamage(weapon.Damage, Owner.NetMsg.NetworkId, weapon.Name);
            return true;
        }

        protected void DealDamage(UnitManager t)
        {
            int mDamage = t.MitigateDamage(weapon, Owner.transform.position);
            t.ProcessDamageInterrupts(mDamage, Owner);
            Owner.LastFiredTime = Time.time;
        }

        public override void Exit()
        {
            IsActive = false;
            base.Exit();
            if (Owner.IsMine && Owner.Data.IsSelectable)
            {
                MusicManager.Instance.RemoveCombatUnit();
                Owner.Squad.UnregisterCombatUnit();
            }
        }
    }
}