using FracturedState.Game.Data;
using FracturedState.Game.Management;
using System.Collections.Generic;
using UnityEngine;

namespace FracturedState.Game.AI
{
    class PassengerIdleState : UnitBaseState
    {
        protected UnitManager transport;
        protected Transform point;
        bool foundTarget;

        public PassengerIdleState(UnitManager owner, UnitManager transport, Transform point)
            : base(owner)
        {
            this.transport = transport;
            this.point = point;
            owner.PassengerSlot = point;
        }

        public override void Enter()
        {
            if (SelectionManager.Instance.SelectedUnits.Contains(Owner))
            {
                Owner.OnDeSelected(true);
            }
            transport.Passengers.Add(Owner);
            Owner.Transport = transport;
            if (Owner.AnimControl != null)
            {
                Owner.AnimControl.Stop();
                Owner.AnimControl.Rewind();
                if (Owner.Data.Animations.StandAim != null && Owner.Data.Animations.StandAim.Length > 0)
                {
                    Owner.AnimControl.Play(Owner.Data.Animations.StandAim[Random.Range(0, Owner.Data.Animations.StandAim.Length)], PlayMode.StopAll);
                }
            }
            foundTarget = false;
        }

        public override void Execute()
        {
            if (point != null)
            {
                Owner.transform.position = point.position;
                Owner.transform.rotation = point.rotation;
            }
            if ((Owner.IsMine || Owner.AISimulate) && !foundTarget && Owner.ContextualWeapon != null)
            {
                UnitManager target = null;
                float dist = float.MaxValue;
                List<UnitManager> visible = Owner.Squad.GetVisibleForUnit(Owner);
                if (visible != null)
                {
                    for (var i = 0; i < visible.Count; i++)
                    {
                        if (visible[i] != null && visible[i].IsAlive && VisibilityChecker.Instance.HasSight(Owner, visible[i]))
                        {
                            Vector3 toUnit = (visible[i].transform.position - Owner.transform.position);
                            bool inRange = toUnit.magnitude < Owner.ContextualWeapon.Range;
                            if (!inRange || visible[i].WorldState != Owner.WorldState || Vector3.Dot(point.forward, toUnit.normalized) < ConfigSettings.Instance.Values.FirePointVisionThreshold)
                            {
                                continue;
                            }
                            if (toUnit.sqrMagnitude < dist)
                            {
                                target = visible[i];
                                dist = toUnit.sqrMagnitude;
                            }
                        }
                    }
                }
                if (target != null)
                {
                    foundTarget = true;
                    Owner.NetMsg.CmdSetTarget(target.NetMsg.NetworkId);
                }
            }
        }
    }
}
