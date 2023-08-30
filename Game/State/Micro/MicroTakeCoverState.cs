using UnityEngine;
using System.Collections.Generic;
using Vectrosity;

namespace FracturedState.Game.AI
{
    public class MicroTakeCoverState : MicroMoveState
    {
        public CoverManager CoverManager { get; protected set; }
        public Transform CoverPoint { get; protected set; }

        private GameObject helper;
        private VectorLine helperLine;

        public MicroTakeCoverState(UnitManager owner, CoverManager coverManager, Transform coverPoint)
            : base(owner, coverPoint.position)
        {
            CoverManager = coverManager;
            CoverPoint = coverPoint;
        }

        public MicroTakeCoverState(UnitManager owner, CoverManager coverManager, Transform coverPoint, List<Vector3> path)
            : base(owner, path)
        {
            CoverManager = coverManager;
            CoverPoint = coverPoint;
        }
        public override void Enter()
        {
            base.Enter();
            if (!owner.IsMine) return;
            
            helper = ObjectPool.Instance.GetCoverHelper(CoverPoint.position);
            helperLine = LineManager.Instance.GetMoveLine();
            helperLine.points3.Clear();
            helperLine.points3.Add(owner.transform.position);
            helperLine.points3.Add(CoverPoint.position);
        }

        public override void SetPath(List<Vector3> path)
        {
            base.SetPath(path);
            if (path == null)
            {
                path = new List<Vector3>() { CoverPoint.position };
            }
        }

        protected override void OnArrival()
        {
            owner.transform.position = CoverPoint.position;
            owner.transform.rotation = CoverPoint.rotation;
            owner.transform.Rotate(new Vector3(0, 180, 0));
            CoverManager.OccupyPoint(owner, CoverPoint);
            owner.CurrentCover = CoverManager;
            owner.CurrentCoverPoint = owner.CurrentCover.GetPointInfo(CoverPoint);
            owner.InCover = true;
            owner.StateMachine.ChangeState(new UnitIdleCoverState(owner));
        }

        public override void Exit()
        {
            base.Exit();
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