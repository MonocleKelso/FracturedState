using UnityEngine;

namespace FracturedState.Game.AI
{
    public class UnitEnterStructureState : UnitMoveState
    {
        protected readonly StructureManager currentStructure;
        protected readonly StructureManager newStructure;
        private Transform entrance;
        private Transform compoundExit;

        public UnitEnterStructureState(UnitManager owner, StructureManager currentStructure)
            : base(owner)
        {
            this.currentStructure = currentStructure;
        }

        public UnitEnterStructureState(UnitManager owner, StructureManager currentStructure, StructureManager newStructure)
            : base(owner)
        {
            this.currentStructure = currentStructure;
            this.newStructure = newStructure;
        }

        public override void Enter()
        {
            if (newStructure != null)
            {
                compoundExit = Owner.CurrentStructure.GetClosestExitToWorldPoint(newStructure.transform.position);
                Destination = compoundExit.position;
            }
            else
            {
                entrance = currentStructure.GetClosestEntrance(Owner.transform.position);
                Destination = entrance.position;
            }
            base.Enter();
        }

        protected override void OnArrival()
        {
            Owner.CurrentVelocity = Vector3.zero;
            Owner.WorldState = Nav.State.Interior;

            if (newStructure != null)
            {
                Owner.CurrentStructure.Leave(Owner);
                Owner.transform.position = Owner.CurrentStructure.GetLinkedTransform(compoundExit).position;
                
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
                Owner.OnEnterIssued(newStructure);
            }
            else
            {
                Owner.transform.position = currentStructure.GetLinkedTransform(entrance).position;
                Owner.CurrentStructure = currentStructure;
                
                OccupyOpenGround();

                if (Owner.IsMine || Owner.AISimulate)
                {
                    Owner.NetMsg.CmdSyncStructureEnter(Owner.CurrentStructure.GetComponent<Identity>().UID, Owner.transform.position);
                }
            }
        }
    }
}