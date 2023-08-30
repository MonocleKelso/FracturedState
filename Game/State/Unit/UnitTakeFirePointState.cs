using UnityEngine;
using Vectrosity;

namespace FracturedState.Game.AI
{
    public class UnitTakeFirePointState : UnitMoveState
    {
        protected Transform firePoint;
        private GameObject helper;
        private VectorLine helperLine;

        public UnitTakeFirePointState(UnitManager owner, Transform firePoint) : base(owner)
        {
            this.firePoint = firePoint;
        }

        public override void Enter()
        {
            Destination = firePoint.position;
            base.Enter();
            if (Owner.IsMine)
            {
                helper = ObjectPool.Instance.GetFirePointHelper(Destination);
                helperLine = LineManager.Instance.GetMoveLine();
                helperLine.points3.Clear();
                helperLine.points3.Add(Owner.transform.position);
                helperLine.points3.Add(Destination);
            }
        }

        public override void Execute()
        {
            base.Execute();
            if (helperLine != null)
            {
                helperLine.points3[0] = Owner.transform.position;
            }
        }

        protected override void AttackMoveEnemySearch()
        {
            // intentionally empty so we don't stop units
        }
        
        protected override void OnArrival()
        {
            Owner.CurrentVelocity = Vector3.zero;
            Owner.transform.position = firePoint.position;
            if (Owner.PrimaryMuzzleFlash != null)
            {
                Transform muzzlePos = Owner.CurrentStructure.GetParticleBone(firePoint.name);
                if (muzzlePos != null)
                {
                    Owner.FirePointMuzzleFlash = (ParticleSystem)GameObject.Instantiate(Owner.ContextualMuzzleFlash);
                    Owner.FirePointMuzzleFlash.transform.position = muzzlePos.position;
                    Owner.FirePointMuzzleFlash.gameObject.SetLayerRecursively(GameConstants.ExteriorLayer);
                    if (!Owner.IsMine)
                    {
                        Renderer[] renders = Owner.FirePointMuzzleFlash.GetComponentsInChildren<Renderer>();
                        for (var i = 0; i < renders.Length; i++)
                        {
                            renders[i].enabled = true;
                        }
                    }
                }
            }
            Owner.StateMachine.ChangeState(new UnitFirePointIdleState(Owner, firePoint));
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