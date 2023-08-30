using FracturedState.Game.AI;
using FracturedState.Game.Data;
using FracturedState.Game.Network;
using UnityEngine;

namespace FracturedState.Scripting
{
    public class PlaceMine : LocationAbility, IMonitorAbility
    {
        internal class MoveToPlaceState : UnitMoveState
        {
            private readonly PlaceMine placeMine;

            public MoveToPlaceState(UnitManager owner, Vector3 destination, PlaceMine placeMine) : base(owner, destination)
            {
                this.placeMine = placeMine;
            }

            protected override void AttackMoveEnemySearch()
            {
                // intentionally empty so we don't stop units
            }
            
            protected override void OnArrival()
            {
                placeMine.Place();
            }
        }
        
        public PlaceMine(UnitManager caster, Vector3 location, Ability ability) : base(caster, location, ability) { }

        private MoveToPlaceState internalState;
        
        public override void ExecuteAbility()
        {
            internalState = new MoveToPlaceState(caster, location, this);
            internalState.Enter();
        }

        private void Place()
        {
            if (FracNet.Instance.IsHost)
            {
                Spawner.SpawnUnit("Landmine", location, caster.transform.rotation, caster.OwnerTeam, caster.WorldState);
            }
            
            base.ExecuteAbility();
            caster.StateMachine.ChangeState(new UnitIdleState(caster));
        }

        public void Update()
        {
            internalState.Execute();
        }

        public void Finish() { }
    }
}