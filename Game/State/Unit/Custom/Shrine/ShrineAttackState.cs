using System.Linq;
using Code.Game.Management;
using UnityEngine;

namespace FracturedState.Game.AI
{
    public class ShrineAttackState : CustomAttackState
    {
        public ShrineAttackState(AttackStatePackage initPackage) : base(initPackage) { }

        public override void Enter()
        {
            base.Enter();
            if (!owner.AnimControl.IsPlaying("Run") && !owner.AnimControl.IsPlaying("Idle1") &&
                !owner.AnimControl.IsPlaying("Idle2") && !owner.AnimControl.IsPlaying("Idle3"))
            {
                owner.AnimControl["Fire"].speed = -1;
                owner.AnimControl["Fire"].time = owner.AnimControl["Fire"].length;
                owner.AnimControl.Play("Fire");
            }
            else
            {
                owner.AnimControl.Stop();
            }
        }

        public override void Execute()
        {
            if (owner == null || !owner.IsAlive) return;

            if (!owner.AnimControl.isPlaying)
            {
                owner.AnimControl.Play(owner.Data.Animations.Idle[Random.Range(0, owner.Data.Animations.Idle.Length)]);
            }
            
            if (!CheckTargetEligibility())
            {
                owner.StateMachine.ChangeState(new UnitIdleState(owner));
                return;
            }
            
            VotarySpawner.CheckAndSpawn(owner);
            
            if (!owner.IsMine && !owner.AISimulate) return;
            
            if ((target.transform.position - owner.transform.position).magnitude > weapon.Range)
            {
                owner.SetMicroState(new MicroUseAbilityState(owner, "ShrineRecall"));
                owner.PropagateMicroState();
                owner.StateMachine.ChangeState(new UnitPendingState(owner));
                return;
            }
            
            var votaries = VotarySpawner.GetVotaries(owner);
            if (votaries == null) return;
            
            var enemies = owner.Squad.GetVisibleForUnit(owner);
            if (enemies == null || enemies.Count == 0) return;

            enemies = enemies.Where(e => e != null).ToList();

            if (enemies.Count == 0) return;
                    
            foreach (var votary in votaries)
            {
                if (votary != null && votary.IsAlive && votary.IsIdle)
                {
                    votary.NetMsg.CmdSetTarget(enemies[Random.Range(0, enemies.Count)].NetMsg.NetworkId);
                    votary.IsIdle = false;
                }
            }
        }
    }
}