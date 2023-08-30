using FracturedState.Game;
using FracturedState.Game.AI;
using FracturedState.Game.Data;
using FracturedState.Game.Network;
using UnityEngine;

namespace FracturedState.Scripting
{
    public class FlamethrowerCanister : LocationAbility, IMonitorAbility
    {
        private class MoveToPlaceState : UnitMoveState
        {
            private readonly FlamethrowerCanister canister;
            
            public MoveToPlaceState(UnitManager owner, Vector3 destination, FlamethrowerCanister canister) 
                : base(owner, destination)
            {
                this.canister = canister;
            }

            public override void Execute()
            {
                base.Execute();
                if ((Owner.transform.position - Destination).magnitude < Range)
                {
                    OnArrival();
                }
            }

            protected override void AttackMoveEnemySearch()
            {
                // intentionally empty so we don't stop units
            }
            
            protected override void OnArrival()
            {
                canister.Toss();
            }
        }

        private const string EffectPath = "Flamethrower/Canister/Canister";
        private const string AnimName = "SmokeGrenade";
        private const float Range = 10;
        private const float Radius = 7.5f;

        private MoveToPlaceState internalState;
        
        public FlamethrowerCanister(UnitManager caster, Vector3 location, Ability ability) : base(caster, location, ability) { }

        public override void ExecuteAbility()
        {
            if ((caster.transform.position - location).magnitude > Range)
            {
                internalState = new MoveToPlaceState(caster, location, this);
                internalState.Enter();
            }
            else
            {
                Toss();
            }
        }

        private void Toss()
        {
            base.ExecuteAbility();
            caster.AnimControl.Play(AnimName);
            var effect = DataUtil.LoadBuiltInParticleSystem(EffectPath);
            effect.SetLayerRecursively(caster.gameObject.layer);
            if (FracNet.Instance.IsHost)
            {
                var check = new GameObject("Collider");
                check.transform.parent = effect.transform;
                var col = check.AddComponent<SphereCollider>();
                col.radius = Radius;
                col.isTrigger = true;
                var tick = check.AddComponent<CanisterDamageTick>();
                tick.Caster = caster;
            }
            var life = effect.AddComponent<Lifetime>();
            life.SetLifetime(9);
            effect.transform.position = location;
        }

        public void Update()
        {
            if ((caster.transform.position - location).magnitude > Range)
            {
                internalState.Execute();
                return;
            }
            if (caster != null && !caster.AnimControl.IsPlaying(AnimName))
            {
                caster.StateMachine.ChangeState(new UnitIdleState(caster));
            }
        }

        public void Finish() { }
    }
}