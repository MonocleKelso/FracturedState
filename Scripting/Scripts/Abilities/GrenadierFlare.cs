using FracturedState.Game.AI;
using FracturedState.Game.Data;
using FracturedState.Game.Network;
using UnityEngine;

namespace FracturedState.Scripting
{
    public class GrenadierFlare : LocationAbility, IMonitorAbility
    {
        private class MoveToPlaceState : UnitMoveState
        {
            private readonly GrenadierFlare flare;

            public MoveToPlaceState(UnitManager owner, Vector3 destination, GrenadierFlare flare) : base(owner, destination)
            {
                this.flare = flare;
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
                flare.PlaceFlare();
            }
        }

        private const float Range = 10;
        private MoveToPlaceState internalState;
        
        public GrenadierFlare(UnitManager caster, Vector3 location, Ability ability) : base(caster, location, ability) { }

        public override void ExecuteAbility()
        {
            internalState = new MoveToPlaceState(caster, location, this);
            internalState.Enter();
        }

        private void PlaceFlare()
        {
            if (FracNet.Instance.IsHost)
            {
                Spawner.SpawnedOwnedUnit(caster, "Flare", location, Quaternion.identity, caster.OwnerTeam, caster.WorldState);
            }
            
            base.ExecuteAbility();
            caster.StateMachine.ChangeState(new UnitIdleState(caster));
        }

        public void Update()
        {
            if ((caster.transform.position - location).magnitude > Range)
            {
                internalState.Execute();
                return;
            }
            
            caster.StateMachine.ChangeState(new UnitIdleState(caster));
        }

        public void Finish() { }
    }
}