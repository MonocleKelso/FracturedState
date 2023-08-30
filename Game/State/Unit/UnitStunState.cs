using FracturedState.Game.Network;
using UnityEngine;

namespace FracturedState.Game.AI
{
    public class UnitStunState : UnitBaseState
    {
        public const string EffectName = "StatusEffects/Stun/StunStatus";

        protected float duration;
        protected bool stunned;
        bool msgSent;
        bool exited;
        ParticleSystem effect;

        public UnitStunState(UnitManager owner, float duration) : base(owner)
        {
            this.duration = duration;
        }

        public void UnStun()
        {
            duration = -1;
            stunned = false;
            Owner.StateMachine.ChangeState(new UnitIdleState(Owner));
        }

        public override void Enter()
        {
            base.Enter();
            exited = false;
            stunned = true;
            Owner.IsIdle = false;
            Owner.AcceptInput = false;
            Owner.AnimControl.Stop();
            effect = ParticlePool.Instance.GetSystem(EffectName);
            effect.gameObject.SetLayerRecursively(Owner.gameObject.layer);
        }

        public override void Execute()
        {
            effect.transform.position = Owner.transform.position + Vector3.up * Owner.Data.StatusIconHeight;
            duration -= Time.deltaTime;
            if (FracNet.Instance.IsHost && duration <= 0 && !msgSent)
            {
                Owner.NetMsg.CmdUnStun();
                msgSent = true;
            }
        }

        public override void Exit()
        {
            if (stunned && Owner.IsAlive && !exited)
            {
                exited = true;
                Owner.StateMachine.ChangeState(this);
            }
            else
            {
                ParticlePool.Instance.ReturnSystem(effect);
                Owner.AcceptInput = true;
            }
        }
    }
}