using FracturedState.Game;
using FracturedState.Game.AI;
using FracturedState.Game.Data;
using UnityEngine;

namespace FracturedState.Scripting
{
    public class TransportCharge : LocationAbility, IMonitorAbility
    {
        private class Charge : UnitMoveState
        {
            private TransportCharge ability;
            
            protected Charge(UnitManager owner, TransportCharge ability) : base(owner)
            {
                this.ability = ability;
            }

            public Charge(UnitManager owner, Vector3 destination, TransportCharge ability) : base(owner, destination)
            {
                this.ability = ability;
            }
            
            protected override void AttackMoveEnemySearch()
            {
                // intentionally empty so we don't stop units
            }

            protected override void OnArrival()
            {
                ability.arrived = true;
            }
        }

        private Charge internalState;
        private float speedIncrease;
        private bool arrived;
        private float timeStarted;
        
        public TransportCharge(UnitManager caster, Vector3 location, Ability ability) : base(caster, location, ability) { }

        public override void ExecuteAbility()
        {
            base.ExecuteAbility();

            timeStarted = Time.time;
            caster.AcceptInput = false;
            speedIncrease = XmlCacheManager.Units[caster.Data.Name].Physics.MaxSpeed * 0.5f;
            caster.Data.Physics.MaxSpeed += speedIncrease;
            var m = DataUtil.LoadBuiltInParticleSystem("Transport/TransportCharge");
            m.SetLayerRecursively(GameConstants.ExteriorUnitLayer);
            var monitor = m.GetComponent<TransportChargeMonitor>();
            monitor.SetOwner(caster);
            internalState = new Charge(caster, location, this);
            internalState.Enter();
        }

        public void Update()
        {
            if (!arrived && Time.time - timeStarted < 5)
            {
                internalState.Execute();
            }
            else
            {
                if (caster != null && caster.IsAlive)
                {
                    caster.StateMachine.ChangeState(new UnitIdleState(caster));
                }
            }
        }

        public void Finish()
        {
            if (caster != null && caster.IsAlive)
            {
                caster.AcceptInput = true;
                caster.Data.Physics.MaxSpeed -= speedIncrease;
            }
        }
    }
}