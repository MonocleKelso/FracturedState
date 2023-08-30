using System.Collections.Generic;
using FracturedState.Game.Data;
using FracturedState.Game.Nav;
using FracturedState.Game.Network;
using FracturedState.Scripting;
using Monocle.Threading;
using UnityEngine;

namespace FracturedState.Game.AI
{
    public class UnitSuppressState : UnitBaseState, IPathableState
    {
        public bool IsActive { get; private set; }
        private bool aimed;
        private int currentNode;
        private List<Vector3> movePath;
        private bool moveReqSent;
        private bool moveSmooth;
        private readonly Transform firePoint;
        private AudioClip[] shootSounds;
        private StructureManager structure;
        
        public UnitSuppressState(UnitManager owner, StructureManager structure, string pointName) : base(owner)
        {
            this.structure = structure;
            foreach (var p in structure.AllFirePoints)
            {
                if (p.name == pointName)
                {
                    firePoint = p;
                    break;
                }
            }
        }
        
        public void SetPath(List<Vector3> path)
        {
            movePath = path;
        }

        public override void Enter()
        {
            IsActive = true;
            Owner.IsIdle = false;
            Owner.AddAbility(StopSuppress.AbilityName);
            if (SelectionManager.Instance.SelectedUnits.Contains(Owner))
            {
                SelectionManager.Instance.OnSelectionChanged.Invoke();
            }
        }

        public override void Execute()
        {
            if (Owner == null || !Owner.IsAlive) return;

            var weapon = Owner.ContextualWeapon;
            if (weapon == null)
            {
                Owner.StateMachine.ChangeState(new UnitIdleState(Owner));
                return;
            }
            
            if (weapon.SoundEffects != null && weapon.SoundEffects.Length > 0)
            {
                shootSounds = new AudioClip[weapon.SoundEffects.Length];
                for (var i = 0; i < weapon.SoundEffects.Length; i++)
                {
                    shootSounds[i] = DataUtil.LoadBuiltInSound(weapon.SoundEffects[i]);
                }
            }

            // if we're out of range then move into range
            if ((Owner.transform.position - firePoint.position).magnitude > weapon.Range)
            {
                // unless we're in cover then go idle
                if (Owner.CurrentCover != null)
                {
                    Owner.StateMachine.ChangeState(new UnitIdleCoverState(Owner));
                    return;
                }
                
                if (movePath == null)
                {
                    if (moveReqSent) return;
                    
                    PathRequestManager.Instance.RequestPath(new PathRequest(this, Owner.transform.position, firePoint.position, Owner.Data.Physics.PathRadius));
                    moveReqSent = true;

                    return;
                }

                if (!moveSmooth)
                {
                    movePath = AStarPather.SmoothPath(movePath, Owner.Data.Physics.PathRadius, Nav.State.Exterior);
                    moveSmooth = true;
                    var anim = Owner.Data.Animations.Move[Random.Range(0, Owner.Data.Animations.Move.Length)];
                    var len = Owner.AnimControl[anim].length;
                    Owner.AnimControl[anim].time = Random.Range(0, len);
                    Owner.AnimControl.Play(anim);
                }
                    
                InfantrySpace();
                currentNode = Owner.LocoMotor.MoveOnPath(movePath, currentNode);
                    
                if (Owner.IsMine)
                {
                    Owner.Squad?.UpdateUnitPosition(Owner);
                }

                return;
            }

            Owner.transform.LookAt(new Vector3(firePoint.position.x, 0, firePoint.position.z));
            if (!aimed && Owner.Data.Animations?.StandAim?.Length > 0)
            {
                Owner.AnimControl.Play(Owner.Data.Animations.StandAim[Random.Range(0, Owner.Data.Animations.StandAim.Length)], PlayMode.StopAll);
                aimed = true;
            }

            if (Owner.LastFiredTime + weapon.FireRate > Time.time) return;

            // process effects and damage
            if (Owner.ContextualMuzzleFlash != null)
            {
                Owner.ContextualMuzzleFlash.Play();
            }

            if (Owner.Data.Animations?.StandFire?.Length > 0)
            {
                Owner.AnimControl.Play(Owner.Data.Animations.StandFire[Random.Range(0, Owner.Data.Animations.StandFire.Length)], PlayMode.StopAll);
            }
            
            if (shootSounds != null)
            {
                var audio = Owner.GetComponent<AudioSource>();
                audio.clip = shootSounds[Random.Range(0, shootSounds.Length)];
                audio.pitch = Random.Range(0.80f, 1.2f);
                audio.Play();
            }
            
            // do effect on structure closest to unit
            if (!string.IsNullOrEmpty(weapon.MissEffect))
            {
                RaycastHit hit;
                if (Physics.Raycast(Owner.transform.position + Vector3.up * 2, Owner.transform.forward, out hit, weapon.Range, GameConstants.ExteriorMask))
                {
                    var missFx = ParticlePool.Instance.GetSystem(weapon.MissEffect);
                    missFx.transform.position = hit.point;
                    missFx.gameObject.layer = GameConstants.ExteriorLayer;
                    missFx.Play();
                }
            }

            Owner.LastFiredTime = Time.time;
            
            var chanceToHit = Random.Range(0, 101);
            if (chanceToHit > Owner.Stats.Accuracy) return;

            if (!FracNet.Instance.IsHost) return;
            
            var unit = structure.GetUnitForFirePoint(firePoint);
            if (unit == null) return;

            var damage = unit.MitigateDamage(weapon, Owner.transform.position);
            unit.ProcessDamageInterrupts(damage, Owner);

            if (Random.Range(0, 100) <= 50) return;
            
            var fp = structure.AllFirePoints;
                
            var points = new List<Transform>(); 
            foreach (var p in fp)
            {
                // don't attack firepoints facing away from us
                if (Vector3.Dot((p.position - Owner.transform.position).normalized, p.forward) > 0) continue;
                
                points.Add(p);
            }

            if (points.Count == 0) return;

            var po = points[Random.Range(0, points.Count)];
            Owner.NetMsg.CmdSuppressPoint(structure.GetComponent<Identity>().UID, po.name);
            Owner.StateMachine.ChangeState(new UnitPendingState(Owner));
        }

        public override void Exit()
        {
            IsActive = false;
        }
    }
}