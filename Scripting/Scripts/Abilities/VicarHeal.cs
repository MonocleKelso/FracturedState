using FracturedState.Game.AI;
using FracturedState.Game.Data;
using FracturedState.Game.Management;
using UnityEngine;

namespace FracturedState.Scripting
{
    public class VicarHeal : SelfAbility, IMonitorAbility
    {
        private const float TickTime = 0.5f;
        private float lastTickTime;
        public VicarHeal(UnitManager caster, Ability ability) : base(caster, ability) { }

        public override void ExecuteAbility()
        {
            base.ExecuteAbility();
            if (caster != null && caster.AnimControl != null && caster.IsAlive)
            {
                caster.AnimControl.Play("Deploy", PlayMode.StopAll);
            }
        }

        public void Update()
        {
            var run = false;
            for (var i = 0; i < caster.Squad.Members.Count; i++)
            {
                var mem = caster.Squad.Members[i];
                if (mem != null && mem.DamageProcessor != null && mem.DamageProcessor.MaxHealth > mem.DamageProcessor.CurrentHealth)
                {
                    run = true;
                }
            }
            if (run && Time.time - lastTickTime > TickTime)
            {
                lastTickTime = Time.time;
                if (caster != null && caster.IsAlive && caster.Squad != null)
                {
                    for (var i = 0; i < caster.Squad.Members.Count; i++)
                    {
                        var m = caster.Squad.Members[i];
                        if (m != null && m.IsAlive)
                        {
                            if (m.DamageProcessor.MaxHealth > m.DamageProcessor.CurrentHealth)
                            {
                                m.NetMsg.CmdHeal(caster.ContextualWeapon.Damage);
                                m.NetMsg.CmdApplyBuff((int)BuffType.Root, 0, 1, "Vicar/vicarHeal/VicarHealShort");
                            }
                        }
                    }
                }
                else if (caster != null && caster.IsAlive)
                {
                    caster.StateMachine.ChangeState(new UnitIdleState(caster));
                }
            }
            else if (!run)
            {
                caster.StateMachine.ChangeState(new UnitIdleState(caster));
            }
        }

        void IMonitorAbility.Finish() { }
    }
}