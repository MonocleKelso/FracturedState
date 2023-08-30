using Code.Game.Management;
using FracturedState.Game.Management;
using UnityEngine;

namespace FracturedState.Game.AI
{
    public class ShrineIdleState : CustomState
    {
        private const float SpawnWaitTime = 5;
        private const float VotaryRecallWait = 1;

        private bool recalledVotaries;
        private float wentIdleTime;
        private ParticleSystem left;
        private ParticleSystem right;
        
        public ShrineIdleState(IdleStatePackage initPackage) : base(initPackage) { }

        public override void Enter()
        {
            owner.IsIdle = true;
            wentIdleTime = Time.time;
            left = owner.transform.GetChildByName("shrineOpenL").GetComponent<ParticleSystem>();
            right = owner.transform.GetChildByName("shrineOpenR").GetComponent<ParticleSystem>();
            owner.AnimControl["Fire"].speed = 1;
            owner.AnimControl.Play("Fire");
        }

        public override void Execute()
        {
            if (owner == null || !owner.IsAlive) return;

            if (!recalledVotaries)
            {
                if (wentIdleTime + VotaryRecallWait < Time.time)
                {
                    var votaries = VotarySpawner.GetVotaries(owner);
                    if (votaries != null)
                    {
                        foreach (var votary in votaries)
                        {
                            if (votary != null && votary.IsAlive)
                            {
                                votary.StateMachine.ChangeState(new UnitIdleState(votary));
                            }
                        }
                    }
                    recalledVotaries = true;
                }
            }
            
            if (!owner.AnimControl.IsPlaying("Fire") && !left.isPlaying && !right.isPlaying)
            {
                left.Play();
                right.Play();
            }
            
            if (wentIdleTime + SpawnWaitTime < Time.time)
            {
                VotarySpawner.CheckAndSpawn(owner);
            }

            if (!owner.IsMine && !owner.AISimulate) return;
            
            UnitManager target = null;
            var dist = float.MaxValue;
            var visible = owner.Squad.GetVisibleForUnit(owner);
            if (visible == null) return;
                
            foreach (var vi in visible)
            {
                if (vi != null && vi.IsAlive && !(owner.WorldState == Nav.State.Interior && vi.Data.IsGarrisonUnit)
                    && VisibilityChecker.Instance.HasSight(owner, vi))
                {
                    var toUnit = (vi.transform.position - owner.transform.position);
                    var mag = toUnit.magnitude;
                    if (mag > owner.ContextualWeapon.Range) continue;
                    if (mag > dist) continue;
                            
                    target = vi;
                    dist = toUnit.sqrMagnitude;
                }
            }
            if (target != null && target.NetMsg != null)
            {
                owner.NetMsg.CmdSetTarget(target.NetMsg.NetworkId);
                owner.StateMachine.ChangeState(new UnitPendingState(owner));
            }
        }

        public override void Exit()
        {
            left.Stop();
            right.Stop();
            owner.IsIdle = false;
        }
    }
}