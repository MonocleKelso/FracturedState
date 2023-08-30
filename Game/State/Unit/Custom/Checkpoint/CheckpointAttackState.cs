using FracturedState.Game.Data;
using FracturedState.Game.Management;
using UnityEngine;

namespace FracturedState.Game.AI
{
    public class CheckpointAttackState : CustomAttackState
    {
        private const float SwivelRotate = 10f;
        private const float WobbleAmount = 3f;
        
        private ParticleSystem leftBarrel;
        private ParticleSystem rightBarrel;
        private ParticleSystem barrelHeat;
        private bool leftShot;
        private Transform swivel;
        private Transform bulletSpawn;

        private AudioClip[] shootSounds;
        
        public CheckpointAttackState(AttackStatePackage initPackage) : base(initPackage) { }

        public override void Enter()
        {
            base.Enter();
            leftBarrel = owner.transform.GetChildByName("checkpointMuzzleFlash_L").GetComponent<ParticleSystem>();
            rightBarrel = owner.transform.GetChildByName("checkpointMuzzleFlash_R").GetComponent<ParticleSystem>();
            barrelHeat = owner.transform.GetChildByName("hotBarrels").GetComponent<ParticleSystem>();
            swivel = owner.transform.GetChildByName("EmplacementSwivelBone");
            bulletSpawn = owner.transform.GetChildByName("ProjectileSpawn");
            
            // un-deployed checkpoints don't have a weapon so we can't grab the sounds
            if (weapon != null)
            {
                shootSounds = new AudioClip[weapon.SoundEffects.Length];
                for (var i = 0; i < weapon.SoundEffects.Length; i++)
                {
                    shootSounds[i] = DataUtil.LoadBuiltInSound(weapon.SoundEffects[i]);
                }
            }
        }

        public override void Execute()
        {
            if (owner == null || !owner.IsAlive || weapon == null) return;
            
            if (!CheckTargetEligibility())
            {
                target = null;
                FindTarget();
                if (target == null)
                {
                    owner.StateMachine.ChangeState(new UnitIdleState(owner));
                    return;
                }
            }

            Shoot();
        }

        protected override bool CheckTargetEligibility()
        {
            if (!base.CheckTargetEligibility()) return false;
            
            var toUnit = (target.transform.position - owner.transform.position);
            return Vector3.Dot(owner.transform.forward, toUnit.normalized) > 0.25f;
        }

        private void Shoot()
        {
            var origSwivelRot = swivel.transform.rotation;
            swivel.transform.localRotation = Quaternion.Euler(Vector3.zero);
            var oRot = owner.transform.rotation;
            owner.transform.LookAt(target.transform.position);
            var sRot = swivel.rotation;
            owner.transform.rotation = oRot;
            swivel.transform.rotation = Quaternion.RotateTowards(origSwivelRot, sRot, SwivelRotate * Time.deltaTime);
            
            if (owner.LastFiredTime + weapon.FireRate > Time.time) return;

            owner.LastFiredTime = Time.time;

            var shot = leftShot ? Object.Instantiate(leftBarrel, leftBarrel.transform.position, leftBarrel.transform.rotation)
                : Object.Instantiate(rightBarrel, rightBarrel.transform.position, rightBarrel.transform.rotation);
            shot.gameObject.AddComponent<Lifetime>().SetLifetime(2);
            shot.Play();
            
            leftShot = !leftShot;

            if (!barrelHeat.isPlaying)
            {
                barrelHeat.Play();
            }
            
            AudioSource.PlayClipAtPoint(shootSounds[Random.Range(0, shootSounds.Length)], owner.transform.position);
            
            var proj = ObjectPool.Instance.GetPooledModelAtLocation(weapon.ProjectileData.Model, bulletSpawn.position);
            proj.SetLayerRecursively(GameConstants.ExteriorLayer);
            var pb = proj.GetComponent<ProjectileBehaviour>();
            
            if (pb == null) pb = proj.AddComponent<ProjectileBehaviour>();
                
            pb.Init(weapon, owner, bulletSpawn.position + ((bulletSpawn.forward * weapon.Range) + (Random.insideUnitSphere * WobbleAmount)));
        }
        
        private void FindTarget()
        {
            if (!owner.IsMine && !owner.AISimulate) return;
            
            var dist = float.MaxValue;
            var visible = owner.Squad.GetVisibleForUnit(owner);
            if (visible == null) return;
            
            for (var i = 0; i < visible.Count; i++)
            {
                var v = visible[i];
                if (v != null && v.IsAlive && !(owner.WorldState == Nav.State.Interior && v.Data.IsGarrisonUnit) && VisibilityChecker.Instance.HasSight(owner, v))
                {
                    var toUnit = (v.transform.position - owner.transform.position);
                    var mag = toUnit.magnitude;
                    
                    if (mag > weapon.Range) continue;
                    if (mag > dist) continue;
                        
                    // also check orientation because checkpoints have limited rotation to face target
                    if (Vector3.Dot(owner.transform.forward, toUnit.normalized) < 0.25f) continue;
                            
                    target = v;
                    dist = toUnit.sqrMagnitude;
                }
            }
            if (target != null && target.NetMsg != null)
            {
                owner.NetMsg.CmdSetTarget(target.NetMsg.NetworkId);
            }
        }
    }
}