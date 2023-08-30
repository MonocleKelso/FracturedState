using FracturedState.Game;
using FracturedState.Game.AI;
using FracturedState.Game.Data;
using UnityEngine;

namespace FracturedState.Scripting
{
    public class RuhkSlam : LocationAbility, IMonitorAbility
    {
        internal class MoveToPlaceState : UnitMoveState
        {
            private readonly RuhkSlam slam;
            public bool Arrived;
            
            public MoveToPlaceState(UnitManager owner, Vector3 destination, RuhkSlam slam) : base(owner, destination)
            {
                this.slam = slam;
                Arrived = false;
            }

            protected override void AttackMoveEnemySearch()
            {
                // intentionally empty so we don't stop units
            }
            
            protected override void OnArrival()
            {
                Arrived = true;
                slam.Slam();
            }
        }
        
        const float StunDuration = 3;

        public RuhkSlam(UnitManager caster, Vector3 location, Ability ability) : base(caster, location, ability) { }

        private MoveToPlaceState internalState;
        
        public override void ExecuteAbility()
        {
            internalState = new MoveToPlaceState(caster, location, this);
            internalState.Enter();
        }

        private void Slam()
        {
            caster.transform.position = location;
            caster.AnimControl.Play("stun", PlayMode.StopAll);
            ParticleSystem slam = ParticlePool.Instance.GetSystem("Ruhk/RuhkStun/RuhkStun");
            slam.gameObject.SetLayerRecursively(caster.gameObject.layer);
            slam.transform.position = caster.transform.position + Vector3.up * 0.1f;
            int mask = caster.WorldState == Game.Nav.State.Exterior ? GameConstants.ExteriorUnitAllMask : GameConstants.InteriorUnitAllMask;
            if (caster.IsMine)
            {
                Collider[] nearby = Physics.OverlapSphere(caster.transform.position, 5, mask);
                for (int i = 0; i < nearby.Length; i++)
                {
                    var unit = nearby[i].GetComponent<UnitManager>();
                    if (unit != null && unit.IsAlive && unit.OwnerTeam != caster.OwnerTeam)
                    {
                        unit.NetMsg.CmdStun(StunDuration);
                    }
                }
                UnitBarkManager.Instance.AbilityBark(ability);
            }
            caster.UseAbility(ability.Name);
        }

        public void Update()
        {
            if (!internalState.Arrived)
            {
                internalState.Execute();
                return;
            }
            if (caster != null && !caster.AnimControl.IsPlaying("stun"))
            {
                caster.StateMachine.ChangeState(new UnitIdleState(caster));
            }
        }

        public void Finish() { }
    }
}