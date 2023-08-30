using FracturedState.Game;
using FracturedState.Game.AI;
using FracturedState.Game.Data;
using FracturedState.Game.Management;
using UnityEngine;

namespace FracturedState.Scripting
{
    public class Radiance : LocationAbility, IMonitorAbility
    {
        internal class MoveToPlaceState : UnitMoveState
        {
            private readonly Radiance radiance;
            
            public MoveToPlaceState(UnitManager owner, Vector3 destination, Radiance radiance) : base(owner, destination)
            {
                this.radiance = radiance;
            }

            protected override void AttackMoveEnemySearch()
            {
                // intentionally empty so we don't stop units
            }
            
            protected override void OnArrival()
            {
                radiance.Radiate();
            }
        }

        private const float Radius = 12.5f;
        
        public Radiance(UnitManager caster, Vector3 location, Ability ability) : base(caster, location, ability) { }

        private MoveToPlaceState internalState;
        
        public override void ExecuteAbility()
        {
            internalState = new MoveToPlaceState(caster, location, this);
            internalState.Enter();
        }

        private void Radiate()
        {
            caster.transform.position = location;
            GameObject flash = Spawner.SpawnObject("Effects/Fist/Radiance/FistRadiance", caster.WorldState);
            flash.transform.position = caster.transform.position;
            if (caster.IsMine)
            {
                int layerMask = caster.WorldState == Game.Nav.State.Exterior ? GameConstants.ExteriorEnemyMask : GameConstants.InteriorEnemyMask;
                Collider[] nearby = Physics.OverlapSphere(caster.transform.position, Radius, layerMask);
                for (int i = 0; i < nearby.Length; i++)
                {
                    UnitManager unit = nearby[i].gameObject.GetComponent<UnitManager>();
                    if (unit != null && unit.IsAlive)
                    {
                        if ((unit.WorldState == Game.Nav.State.Interior && unit.CurrentStructure == caster.CurrentStructure) || unit.WorldState == Game.Nav.State.Exterior)
                        {
                            unit.NetMsg.CmdApplyBuff((int)BuffType.Accuracy, -35f, 10f, string.Empty);
                        }
                    }
                }
                UnitBarkManager.Instance.AbilityBark(ability);
            }
            caster.UseAbility(ability.Name);
            caster.StateMachine.ChangeState(new UnitIdleState(caster));
        }

        public void Update()
        {
            internalState.Execute();
        }

        public void Finish() { }
    }
}