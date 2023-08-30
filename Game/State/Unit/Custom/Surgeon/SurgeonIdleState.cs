using UnityEngine;

namespace FracturedState.Game.AI
{
    public class SurgeonIdleState : CustomState
    {
        public const string HealAbilityName = "SurgeonHeal";
        public const string AttackAbilityName = "SurgeonAttack";
        private const float searchRange = 10;

        bool isHealing;
        bool healTargetSet;
        UnitIdleState normalState;
        UnitManager healTarget;
        ITargettingBehaviour<float> targetBehaviour;

        public SurgeonIdleState(IdleStatePackage initPackage) : base(initPackage)
        {
            isHealing = owner.HasAbility(HealAbilityName);
        }

        public override void Enter()
        {
            owner.IsIdle = true;
            if (owner.AnimControl != null)
            {
                owner.AnimControl.Stop();
                owner.AnimControl.Rewind();
            }
            owner.EffectManager?.PlayIdleSystems();
            targetBehaviour = new MostDamagedFriendlyNearby();
        }

        public override void Execute()
        {
            if (isHealing)
            {
                HealSquad();
            }
            else
            {
                // if we're in attack mode then we'll just use a normal idle state to search for enemies
                if (normalState == null)
                    normalState = new UnitIdleState(owner);

                normalState.Execute();
            }
        }

        public override void Exit()
        {
            owner.IsIdle = false;
            owner.EffectManager?.StopCurrentSystem();
        }

        private void HealSquad()
        {
            if (healTarget == null || !healTarget.IsAlive)
            {
                healTarget = targetBehaviour.FindTarget(owner, searchRange);
                healTargetSet = false;
            }

            if (!healTargetSet && healTarget != null && healTarget.IsAlive)
            {
                // use SurgeonHeal ability to heal target
                healTargetSet = true;
                owner.SetMicroState(new MicroUseAbilityState(owner, "SurgeonHealPassive", healTarget));
                owner.PropagateMicroState();
                owner.StateMachine.ChangeState(new UnitPendingState(owner));
            }
            else
            {
                // if we don't have someone to heal then just play idle animation
                if (owner.AnimControl != null && !owner.AnimControl.isPlaying && owner.Data.Animations?.Idle != null && owner.Data.Animations.Idle.Length > 0)
                {
                    owner.AnimControl.Play(owner.Data.Animations.Idle[Random.Range(0, owner.Data.Animations.Idle.Length)], PlayMode.StopAll);
                }
            }
        }
    }
}