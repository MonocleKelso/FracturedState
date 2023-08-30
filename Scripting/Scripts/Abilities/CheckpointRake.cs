using FracturedState.Game;
using FracturedState.Game.AI;
using FracturedState.Game.Data;
using UnityEngine;

namespace FracturedState.Scripting
{
    public class CheckpointRake : SelfAbility, IMonitorAbility
    {
        private const float Duration = 15;
        private const float FireRate = 0.1f;
        private const float RotateRate = 15;
        private const float TurnStop = 45;
        
        private float time;
        
        private ParticleSystem leftBarrel;
        private ParticleSystem rightBarrel;
        private ParticleSystem barrelHeat;
        private bool leftShot;
        private bool turnLeft;
        private Transform swivel;
        private Transform bulletSpawn;

        private Weapon weapon;
        
        private AudioClip[] shootSounds;
        
        public CheckpointRake(UnitManager caster, Ability ability) : base(caster, ability) { }

        public override void ExecuteAbility()
        {
            base.ExecuteAbility();
            weapon = caster.ContextualWeapon;
            leftBarrel = caster.transform.GetChildByName("checkpointMuzzleFlash_L").GetComponent<ParticleSystem>();
            rightBarrel = caster.transform.GetChildByName("checkpointMuzzleFlash_R").GetComponent<ParticleSystem>();
            barrelHeat = caster.transform.GetChildByName("hotBarrels").GetComponent<ParticleSystem>();
            swivel = caster.transform.GetChildByName("EmplacementSwivelBone");
            bulletSpawn = caster.transform.GetChildByName("ProjectileSpawn");
            swivel.localRotation = Quaternion.Euler(Vector3.zero);
            shootSounds = new AudioClip[weapon.SoundEffects.Length];
            for (var i = 0; i < weapon.SoundEffects.Length; i++)
            {
                shootSounds[i] = DataUtil.LoadBuiltInSound(weapon.SoundEffects[i]);
            }
        }

        public void Update()
        {
            if (caster == null || !caster.IsAlive) return;
            
            time += Time.deltaTime;

            if (time > Duration)
            {
                caster.StateMachine.ChangeState(new UnitIdleState(caster));
                return;
            }

            var sRot = swivel.localEulerAngles;
            if (turnLeft)
            {
                sRot.z -= RotateRate * Time.deltaTime;
            }
            else
            {
                sRot.z += RotateRate * Time.deltaTime;
            }
            
            swivel.localEulerAngles = sRot;

            if (turnLeft && sRot.z > 300 && sRot.z < 360 - TurnStop || !turnLeft && sRot.z < 100 && sRot.z >  TurnStop)
            {
                turnLeft = !turnLeft;
            }
            
            if (caster.LastFiredTime + FireRate > Time.time) return;

            caster.LastFiredTime = Time.time;
            
            var shot = leftShot ? Object.Instantiate(leftBarrel, leftBarrel.transform.position, leftBarrel.transform.rotation)
                : Object.Instantiate(rightBarrel, rightBarrel.transform.position, rightBarrel.transform.rotation);
            shot.gameObject.AddComponent<Lifetime>().SetLifetime(2);
            shot.Play();
            
            leftShot = !leftShot;

            if (!barrelHeat.isPlaying)
            {
                barrelHeat.Play();
            }
            
            AudioSource.PlayClipAtPoint(shootSounds[Random.Range(0, shootSounds.Length)], caster.transform.position);
            
            var proj = ObjectPool.Instance.GetPooledModelAtLocation(weapon.ProjectileData.Model, bulletSpawn.position);
            
            var layer = caster.WorldState == Game.Nav.State.Exterior ? GameConstants.ExteriorLayer : GameConstants.InteriorLayer;
            proj.SetLayerRecursively(layer);
                
            var pb = proj.GetComponent<ProjectileBehaviour>();
            if (pb == null) pb = proj.AddComponent<ProjectileBehaviour>();
                
            pb.Init(weapon, caster, bulletSpawn.position + bulletSpawn.forward * weapon.Range);
        }

        public void Finish() { }
    }
}