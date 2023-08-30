using FracturedState.Game.AI;
using FracturedState.Scripting;
using UnityEngine;

namespace FracturedState.Game.Modules
{
    public class WardenShieldDamageModule : DamageModule
    {
        [SerializeField] private GameObject hitEffect;
        [SerializeField] private float hitEffectLife;
        [SerializeField] private GameObject dropEffect;
        [SerializeField] private float dropEffectLifetime;

        private UnitManager owner;
        
        protected override void Awake()
        {
            MaxHealth = 1500;
            CurrentHealth = 1500;
        }

        public override void TakeDamage(int damage, UnitManager attacker, string weapon)
        {
            Damage(damage, attacker != null ? attacker.transform.position + Vector3.up * Random.Range(2f, 4f) : transform.position + Random.onUnitSphere * 5.5f);
        }

        public override void TakeProjectileDamage(int damage, UnitManager attacker, Vector3 projectilePosition, string weapon)
        {
            Damage(damage, projectilePosition);
        }

        private void Damage(int damage, Vector3 pos)
        {
            if (!IsAlive) return;

            CurrentHealth -= damage;
            var fx = Instantiate(hitEffect, transform.position, Quaternion.identity);
            fx.transform.LookAt(pos);
            fx.SetLayerRecursively(GameConstants.ExteriorLayer);
            fx.AddComponent<Lifetime>().SetLifetime(hitEffectLife);
        }

        public override void Heal(int amount)
        {
            // do nothing, shields can't be healed
        }

        public override void Miss(UnitManager attacker)
        {
            // do nothing
        }

        public override void SetOwner(UnitManager owner)
        {
            this.owner = owner;
            var state = owner.StateMachine.CurrentState as MicroUseAbilityState;
            if (state?.ChannelAbility == null) return;
            var s = state.ChannelAbility as WardenShield;
            s?.SetShield(GetComponentInParent<NetworkDamageModule>().gameObject);
        }

        public override UnitManager GetOwner()
        {
            return owner;
        }

        private void OnDestroy()
        {
            var fx = Instantiate(dropEffect, transform.position, dropEffect.transform.rotation);
            fx.SetLayerRecursively(GameConstants.ExteriorLayer);
            fx.AddComponent<Lifetime>().SetLifetime(dropEffectLifetime);
        }
    }
}