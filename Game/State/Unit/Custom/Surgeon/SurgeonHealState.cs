using FracturedState.Game.Data;
using UnityEngine;

namespace FracturedState.Game.AI
{
    public class SurgeonHealState : UnitBaseState
    {
        static string[] barks = new string[1] { "Laperia/Ability/SurgeonHeal02" };

        const float healTickTime = 1;
        const int healAmount = 12;
        const string healEffectName = "Surgeon/heal/surgeonHeal";

        protected UnitManager target;
        float lastHealTime;
        ParticleSystem healEffect;

        public SurgeonHealState(UnitManager owner, UnitManager target) : base(owner)
        {
            this.target = target;
        }

        public override void Enter()
        {
            base.Enter();
            healEffect = ParticlePool.Instance.GetSystem(healEffectName);
            healEffect.gameObject.SetLayerRecursively(target.gameObject.layer);
            Owner.AnimControl.Play("HealLoop", PlayMode.StopAll);
            if (Owner.IsMine)
                UnitBarkManager.Instance.Random2DBark(barks);
        }

        public override void Execute()
        {
            if (target != null && target.DamageProcessor != null && target.DamageProcessor.CurrentHealth < target.Data.Health)
            {
                // if we're close enough then heal
                if ((Owner.transform.position - target.transform.position).magnitude <= 3f)
                {
                    // give surgeon same cover bonus as his heal target
                    Owner.CurrentCover = target.CurrentCover;
                    Owner.CurrentCoverPoint = target.CurrentCoverPoint;
                    healEffect.transform.position = target.transform.position + Vector3.up * target.Data.StatusIconHeight;
                    if ((Owner.IsMine || Owner.AISimulate) && Time.time - lastHealTime >= healTickTime)
                    {
                        target.NetMsg.CmdHeal(healAmount);
                        lastHealTime = Time.time;
                    }
                }
                else
                {
                    // otherwise move to target
                    Owner.StateMachine.ChangeState(new SurgeonHealMoveState(Owner, target));
                }
            }
            else
            {
                // go idle if target dies or is fully healed
                Owner.StateMachine.ChangeState(new UnitIdleState(Owner));
            }
        }

        public override void Exit()
        {
            base.Exit();
            ParticlePool.Instance.ReturnSystem(healEffect);
            Owner.CurrentCoverPoint = null;
            Owner.CurrentCover = null;
        }
    }
}