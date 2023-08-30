using FracturedState.Game;
using FracturedState.Game.Data;
using FracturedState.Game.Nav;
using UnityEngine;

namespace FracturedState.Scripting
{
    public class PenetratingShotExecute : LocationAbility
    {
        public const string AbilityName = "PenetratingShot_execute";
        private const string WeaponName = "PenetratingShotGun";
        private const float Range = 50;
        
        public PenetratingShotExecute(UnitManager caster, Vector3 location, Ability ability) : base(caster, location, ability) { }

        public override void ExecuteAbility()
        {
            var rotation = Quaternion.Euler(location);
            caster.transform.rotation = rotation;
            var weapon = XmlCacheManager.Weapons[WeaponName];
            var proj = ObjectPool.Instance.GetPooledModelAtLocation(weapon.ProjectileData.Model, caster.transform.position);
            proj.transform.position = caster.ContextualMuzzleFlash.transform.position;
            proj.transform.rotation = caster.transform.rotation;
            var layer = caster.WorldState == State.Exterior ? GameConstants.ExteriorLayer : GameConstants.InteriorLayer;
            proj.SetLayerRecursively(layer);
            var pb = proj.GetComponent<ProjectileBehaviour>();
            if (pb == null) pb = proj.AddComponent<ProjectileBehaviour>();
            var position = caster.transform.position + ((rotation * Vector3.forward).normalized * Range);
            pb.Init(weapon, caster, position);
            var audio = caster.GetComponent<AudioSource>();
            var clip = weapon.SoundEffects[Random.Range(0, weapon.SoundEffects.Length)];    
            audio.clip = DataUtil.LoadBuiltInSound(clip);
            audio.pitch = Random.Range(0.80f, 1.2f);
            audio.Play();
            caster.AnimControl.Play("Fire");
        }
    }
}