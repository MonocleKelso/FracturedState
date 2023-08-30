using UnityEngine;
using FracturedState.Game.Data;

namespace FracturedState.Game.AI
{
    public class UnitEnterTransportState : UnitMoveState
    {
        protected UnitManager transport;
        protected Transform entrance;

        public UnitEnterTransportState(UnitManager owner, UnitManager transport)
            : base(owner)
        {
            this.transport = transport;
            entrance = transport.transform.GetChildByName(transport.Data.TransportLogic.EntranceName);
        }

        public override void Enter()
        {
            if (entrance == null)
            {
                throw new FracturedStateException("Cannot enter transport " + transport.Data.Name + " because entrance " +
                    transport.Data.TransportLogic.EntranceName + " does not exist");
            }
            Destination = entrance.position;
            base.Enter();
        }

        protected override void OnArrival()
        {
            if ((Owner.transform.position - entrance.position).sqrMagnitude > ConfigSettings.Instance.Values.TransportEnterDistance)
            {
                // re-calc path if the transport moved before we got to it
                Owner.StateMachine.ChangeState(new UnitEnterTransportState(Owner, transport));
            }
            else
            {
                Transform point = transport.GetNextTransportSlot();
                if (point != null)
                {
                    Owner.StateMachine.ChangeState(new PassengerIdleState(Owner, transport, point));
                }
                else
                {
                    OccupyOpenGround();
                }
            }
        }
    }
}