using UnityEngine;

namespace FracturedState.Game.AI
{
    public class UnitExitStructureState : UnitMoveState
    {
        protected Transform exit;
        protected Transform enter;
        protected Vector3 worldDestination;

        public UnitExitStructureState(UnitManager owner, Vector3 destination) : base(owner, destination) { }

        public override void Enter()
        {
            if (Owner.CurrentStructure != null)
            {
                if (Owner.CurrentFirePoint != null)
                {
                    Owner.CurrentStructure.ReturnFirePoint(Owner.CurrentFirePoint);
                    Owner.CurrentFirePoint = null;
                }
                if (Owner.FirePointMuzzleFlash != null)
                {
                    Object.Destroy(Owner.FirePointMuzzleFlash.gameObject);
                    Owner.FirePointMuzzleFlash = null;
                }
                
                exit = Owner.CurrentStructure.GetClosestExitToWorldPoint(Destination);
                if (exit == null) return;
                
                enter = Owner.CurrentStructure.GetLinkedTransform(exit);
                worldDestination = Destination;
                Destination = exit.position;
                base.Enter();
            }
            else
            {
                Owner.StateMachine.ChangeState(new UnitMoveState(Owner, Destination));
            }
        }

        protected override void OnArrival()
        {
            Owner.CurrentStructure.Leave(Owner);
            
            if (Owner.IsMine)
            {
                Owner.gameObject.SetLayerRecursively(GameConstants.ExteriorUnitLayer, "Vision");
                Owner.UpdateSelectionLayer();
            }
            else
            {
                Owner.gameObject.SetLayerRecursively(GameConstants.ExteriorEnemyLayer, "Vision");
            }
            Owner.WorldState = Nav.State.Exterior;
            Owner.transform.position = enter.position;
            Owner.CurrentStructure = null;
            
            if (Owner.IsMine)
            {
                // triggers won't fire because enemies are already in range when we're in the wrong layer state
                // we do a redundant check here to get an initial set of visiblke exterior units
                var nearby = Physics.OverlapSphere(Owner.transform.position, Owner.Data.VisionRange, GameConstants.ExteriorEnemyMask);
                for (var i = 0; i < nearby.Length; i++)
                {
                    var unit = nearby[i].gameObject.GetComponent<UnitManager>();
                    if (unit.IsFriendly) continue;
                    Owner.Squad.RegisterVisibleUnit(Owner, unit);
                }

            }
            Owner.StateMachine.ChangeState(new UnitMoveState(Owner, worldDestination));
        }
    }
}