using FracturedState.Game.AI;
using FracturedState.Game.Network;
using UnityEngine;

namespace FracturedState.Scripting
{
    public class WardenBlock : PassiveAttackInterrupt
    {
        public WardenBlock(UnitManager caster, UnitManager attacker) : base(caster, attacker) { }

        public override bool Proc()
        {
            return caster.IsIdle && Random.Range(1, 101) >= 75;
        }

        public override void ExecuteAbility()
        {
            caster.AnimControl.Play("ShieldBash", PlayMode.StopAll);
            if (attacker != null)
            {
                caster.transform.LookAt(attacker.transform.position);
                if (FracNet.Instance.IsHost)
                {
                    attacker.NetMsg.CmdTakeDamage(attacker.ContextualWeapon.Damage, caster.NetMsg.NetworkId, attacker.ContextualWeapon.Name);
                }
            }
            caster.StartCoroutine(IdleWait());
        }

        private System.Collections.IEnumerator IdleWait()
        {
            yield return new WaitForSeconds(caster.AnimControl.GetClip("ShieldBash").length);
            caster.StateMachine.ChangeState(new UnitIdleState(caster));
        }
    }
}