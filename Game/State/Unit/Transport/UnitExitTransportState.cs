using UnityEngine;

namespace FracturedState.Game.AI
{
    public class UnitExitTransportState : UnitMoveState
    {
        public UnitExitTransportState(UnitManager owner)
            : base(owner)
        {
            Transform exit = owner.Transport.transform.GetChildByName(owner.Transport.Data.TransportLogic.EntranceName);
            Destination = exit.position;
        }

        protected override void OnArrival()
        {
            Owner.Transport.ReturnTransportSlot(Owner.PassengerSlot);
            Owner.Transport.Passengers.Remove(Owner);
            Owner.Transport = null;
            Owner.PassengerSlot = null;
            base.OnArrival();
        }
    }
}