using UnityEngine;
using System.Collections.Generic;
using Vectrosity;

namespace FracturedState.Game.AI
{
    public class MicroTakeFirepointState : MicroMoveState
    {
        private readonly Transform firePoint;
        public string FirePointName => firePoint.name;
        private GameObject helper;
        private VectorLine helperLine;

        public MicroTakeFirepointState(UnitManager owner, Transform firePoint)
            : base(owner, firePoint.position)
        {
            this.firePoint = firePoint;
        }

        public MicroTakeFirepointState(UnitManager owner, Transform firePoint, List<Vector3> path)
            : base(owner, path)
        {
            this.firePoint = firePoint;
        }

        public override void Enter()
        {
            base.Enter();
            if (owner.IsMine)
            {
                helper = ObjectPool.Instance.GetFirePointHelper(firePoint.position);
                helperLine = LineManager.Instance.GetMoveLine();
                helperLine.points3.Clear();
                helperLine.points3.Add(owner.transform.position);
                helperLine.points3.Add(firePoint.position);
            }
        }

        public override void Execute()
        {
            base.Execute();
            if (helperLine != null)
            {
                helperLine.points3[0] = owner.transform.position;
            }
        }

        protected override void OnArrival()
        {
            owner.CurrentStructure.ForceRemoveFirePoint(firePoint);
            owner.CurrentFirePoint = firePoint;
            owner.CurrentVelocity = Vector3.zero;
            owner.transform.position = firePoint.position;
            if (owner.PrimaryMuzzleFlash != null)
            {
                Transform muzzlePos = owner.CurrentStructure.GetParticleBone(firePoint.name);
                if (muzzlePos != null)
                {
                    owner.FirePointMuzzleFlash = (ParticleSystem)GameObject.Instantiate(owner.PrimaryMuzzleFlash);
                    owner.FirePointMuzzleFlash.transform.position = muzzlePos.position;
                    owner.FirePointMuzzleFlash.gameObject.SetLayerRecursively(GameConstants.ExteriorLayer);
                    if (!owner.IsMine)
                    {
                        Renderer[] renders = owner.FirePointMuzzleFlash.GetComponentsInChildren<Renderer>();
                        for (var i = 0; i < renders.Length; i++)
                        {
                            renders[i].enabled = true;
                        }
                    }
                }
            }
            owner.StateMachine.ChangeState(new UnitFirePointIdleState(owner, firePoint));
        }

        public override void Exit()
        {
            if (helper != null)
            {
                ObjectPool.Instance.ReturnFirePointHelper(helper);
            }
            if (helperLine != null)
            {
                LineManager.Instance.ReturnLine(helperLine);
            }
            base.Exit();
        }
    }
}