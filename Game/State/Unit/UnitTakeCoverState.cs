using UnityEngine;
using Vectrosity;

namespace FracturedState.Game.AI
{
    public class UnitTakeCoverState : UnitMoveState
    {
        private Transform coverPoint;
        private GameObject helper;
        private VectorLine helperLine;
        private CoverManager _coverManager;

        public UnitTakeCoverState(UnitManager owner, CoverManager cover, Transform coverPoint)
            : base(owner)
        {
            _coverManager = cover;
            this.coverPoint = coverPoint;
        }

        public override void Enter()
        {
            Destination = coverPoint.position;
            base.Enter();
            if (!Owner.IsMine) return;
            
            helper = ObjectPool.Instance.GetCoverHelper(Destination);
            helperLine = LineManager.Instance.GetMoveLine();
            helperLine.points3.Clear();
            helperLine.points3.Add(Owner.transform.position);
            helperLine.points3.Add(Destination);
        }

        public override void Execute()
        {
            base.Execute();
            if (helperLine != null)
            {
                helperLine.points3[0] = Owner.transform.position;
            }
        }

        protected override void OnArrival()
        {
            Owner.transform.position = coverPoint.position;
            Owner.transform.rotation = coverPoint.rotation;
            Owner.transform.Rotate(new Vector3(0, 180, 0));
            Owner.CurrentCover = _coverManager;
            Owner.CurrentCoverPoint = Owner.CurrentCover.GetPointInfo(coverPoint);
            Owner.CurrentCover.OccupyPoint(Owner, coverPoint);
            Owner.InCover = true;
            Owner.StateMachine.ChangeState(new UnitIdleCoverState(Owner));
        }

        protected override void AttackMoveEnemySearch()
        {
            // intentionally empty so we don't stop units
        }
        
        public override void Exit()
        {
            base.Exit();
            // if we issued another command before OnArrival was called then return the cover point
            if (!Owner.InCover)
            {
                Owner.RemoveFromCover();
                _coverManager.ReturnReservedPoint(coverPoint);
            }
            if (helper != null)
            {
                ObjectPool.Instance.ReturnCoverHelper(helper);
            }
            if (helperLine != null)
            {
                LineManager.Instance.ReturnLine(helperLine);
            }
        }
    }
}