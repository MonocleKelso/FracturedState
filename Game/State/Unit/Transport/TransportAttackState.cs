using UnityEngine;

namespace FracturedState.Game.AI
{
    public class TransportAttackState : UnitAttackState
    {
        private bool ownsWeapon;

        public TransportAttackState(UnitManager owner, UnitManager target) : base(owner, target) { }

        public override void Enter()
        {
            if (weapon == null)
            {
                ownsWeapon = false;
                if (Owner.Passengers.Count == 0)
                {
                    if (Owner.IsMine || Owner.AISimulate)
                        Owner.OnMoveIssued(target.transform.position);
                }
                else
                {
                    float range = 0;
                    for (int i = 0; i < Owner.Passengers.Count; i++)
                    {
                        if (Owner.Passengers[i] != null)
                        {
                            if (Owner.Passengers[i].ContextualWeapon != null && Owner.Passengers[i].ContextualWeapon.Range > range)
                            {
                                range = Owner.Passengers[i].ContextualWeapon.Range;
                                weapon = Owner.Passengers[i].ContextualWeapon;
                            }
                        }
                    }
                }
            }
            else
            {
                ownsWeapon = true;
            }
            base.Enter();
        }

        public override void Execute()
        {
            if (ownsWeapon)
                base.Execute();
        }

        protected override void Shoot()
        {
            if (ownsWeapon)
            {
                Owner.transform.LookAt(target.transform.position);
                base.Shoot();
            }
        }
    }
}