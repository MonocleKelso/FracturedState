using FracturedState.Game.Data;
using FracturedState.Game.Management;
using FracturedState.Game.Nav;
using UnityEngine;

namespace FracturedState.Game.AI
{
    public class UnitMoveToAttackState : UnitMoveState
    {
        private UnitManager _target;
        private Weapon _weapon;
        
        public UnitMoveToAttackState(UnitManager owner, UnitManager target) : base(owner)
        {
            _target = target;
            Destination = _target.transform.position;
            _weapon = owner.ContextualWeapon;
        }

        public UnitMoveToAttackState(UnitManager owner, UnitManager target, Vector3 moveTo) : base(owner, moveTo)
        {
            _target = target;
            _weapon = owner.ContextualWeapon;
        }

        public override void Execute()
        {
            if (_target == null || !_target.IsAlive || !VisibilityChecker.Instance.HasSight(Owner, _target))
            {
                Owner.StateMachine.ChangeState(new UnitIdleState(Owner));
                return;
            }
            if ((_target.transform.position - Owner.transform.position).sqrMagnitude <= _weapon.Range * _weapon.Range)
            {
                Owner.StateMachine.ChangeState(new UnitAttackState(Owner, _target));
                return;
            }
            base.Execute();
        }

        protected override void AttackMoveEnemySearch()
        {
            // do nothing in here because we're already attacking someone
        }

        protected override void OccupyOpenGround()
        {
            var grid = (Owner.CurrentStructure != null) ? Owner.CurrentStructure.NavigationGrid : AStarPather.Instance.ExteriorGrid;
            var closestPoint = grid.GetClosestFreePoint(Owner.transform.position, Owner.Data.Physics.PathRadius, _target.transform.position);

            if (closestPoint != null)
            {
                if ((closestPoint.WorldPosition - Owner.transform.position).sqrMagnitude <=
                    ConfigSettings.Instance.Values.CloseEnoughThreshold)
                {
                    grid.OccupyPoint(Owner, closestPoint);
                    Owner.StateMachine.ChangeState(new UnitAttackState(Owner, _target));
                }
                else
                {
                    Owner.StateMachine.ChangeState(new UnitMoveToAttackState(Owner, _target, closestPoint.WorldPosition));
                }
            }
            else
            {
                Owner.StateMachine.ChangeState(new UnitIdleState(Owner));
            }
        }
    }
}