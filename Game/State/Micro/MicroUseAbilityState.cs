using FracturedState.Game.Data;
using UnityEngine;
using FracturedState.Scripting;

namespace FracturedState.Game.AI
{
    public class MicroUseAbilityState : MicroBaseState
    {
        public Ability AbilityData { get; protected set; }
        public Vector3 Position { get; protected set; }
        public UnitManager Target { get; protected set; }
        protected IFracAbility ability = null;
        protected IMonitorAbility monitor = null;
        public IMonitorAbility ChannelAbility => monitor;

        public MicroUseAbilityState(UnitManager owner, string abilityName)
            : base(owner)
        {
            AbilityData = XmlCacheManager.Abilities[abilityName];
        }

        public MicroUseAbilityState(UnitManager owner, string abilityName, Vector3 position)
            : base(owner)
        {
            AbilityData = XmlCacheManager.Abilities[abilityName];
            Position = position;
        }

        public MicroUseAbilityState(UnitManager owner, string abilityName, UnitManager target)
            : base(owner)
        {
            AbilityData = XmlCacheManager.Abilities[abilityName];
            Target = target;
        }

        public MicroUseAbilityState(UnitManager owner, string abilityName, IFracAbility ability)
            : base(owner)
        {
            AbilityData = XmlCacheManager.Abilities[abilityName];
            this.ability = ability;
        }

        public MicroUseAbilityState(UnitManager owner, string abilityName, IFracAbility ability, UnitManager target)
            : base(owner)
        {
            AbilityData = XmlCacheManager.Abilities[abilityName];
            this.ability = ability;
            Target = target;
        }

        public override void Enter()
        {
            base.Enter();
            owner.IsIdle = false;
            if (ability == null)
            {
                if (AbilityData.Targetting == TargetType.None)
                {
                    ability = ScriptManager.CreateAbilityScriptInstance(AbilityData.Script, owner, AbilityData);
                }
                else if (AbilityData.Targetting == TargetType.Ground || AbilityData.Targetting == TargetType.Structure)
                {
                    ability = ScriptManager.CreateAbilityScriptInstance(AbilityData.Script, owner, Position, AbilityData);
                }
                else
                {
                    ability = ScriptManager.CreateAbilityScriptInstance(AbilityData.Script, owner, Target, AbilityData);
                }
            }
            ability.ExecuteAbility();
            monitor = ability as IMonitorAbility;
        }

        public override void Execute()
        {
            if (monitor != null)
                monitor.Update();
            else if (owner != null && owner.StateMachine.CurrentState == this) // bump to idle so we don't get stuck
                owner.StateMachine.ChangeState(new UnitIdleState(owner));
        }

        public override void Exit()
        {
            base.Exit();
            monitor?.Finish();
        }
    }
}