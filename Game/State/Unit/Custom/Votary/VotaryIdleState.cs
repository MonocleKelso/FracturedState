using FracturedState.Game.Data;
using FracturedState.Game.Network;
using UnityEngine;

namespace FracturedState.Game.AI
{
    public class VotaryIdleState : CustomState
    {
        private const float IdleHoverDistance = 5f;

        private Vector3 floatLocation;
        
        public VotaryIdleState(IdleStatePackage initPackage) : base(initPackage) { }

        public override void Enter()
        {
            owner.IsIdle = true;
            GetFloatLocation();
        }

        public override void Execute()
        {
            if (FracNet.Instance.IsHost && (owner.NetMsg.OwnerUnit == null || !owner.NetMsg.OwnerUnit.IsAlive))
            {
                owner.NetMsg.CmdTakeDamage(int.MaxValue, null, Weapon.DummyName);
                return;
            }
            
            owner.transform.LookAt(new Vector3(floatLocation.x, owner.transform.position.y, floatLocation.z));
            owner.transform.position = Vector3.MoveTowards(owner.transform.position, floatLocation,
                owner.Data.Physics.MaxSpeed * Time.deltaTime);

            if ((owner.transform.position - floatLocation).magnitude < 0.1)
            {
                GetFloatLocation();
            }
        }

        private void GetFloatLocation()
        {
            if (owner.NetMsg.OwnerUnit != null)
            {
                floatLocation = owner.NetMsg.OwnerUnit.transform.position + Vector3.up * 5 +
                                Random.insideUnitSphere * IdleHoverDistance;
            }
        }

        public override void Exit()
        {
            owner.IsIdle = false;
        }
    }
}