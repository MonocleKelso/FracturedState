
using FracturedState.Game.Data;

namespace FracturedState.Scripting
{
    /// <summary>
    /// A base class for any unit ability that targets another unit
    /// </summary>
    public abstract class TargetAbility : IFracAbility
    {
        protected UnitManager caster;
        protected UnitManager target;
        protected Ability ability;

        protected TargetAbility(UnitManager caster, UnitManager target, Ability ability)
        {
            this.caster = caster;
            this.target = target;
            this.ability = ability;
        }

        public virtual void ExecuteAbility()
        {
            caster.UseAbility(ability.Name);
            if (caster.IsMine)
            {
                UnitBarkManager.Instance.AbilityBark(ability);
            }
        }
    }

    /// <summary>
    /// A base class for any unit ability that requires no interaction with other agents or locations in the world
    /// </summary>
    public abstract class SelfAbility : IFracAbility
    {
        protected UnitManager caster;
        protected Ability ability;

        protected SelfAbility(UnitManager caster, Ability ability)
        {
            this.caster = caster;
            this.ability = ability;
        }

        public virtual void ExecuteAbility()
        {
            caster.UseAbility(ability.Name);
            if (caster.IsMine)
            {
                UnitBarkManager.Instance.AbilityBark(ability);
            }
        }
    }

    /// <summary>
    /// A base class for any ability that requires interaction with a specific location in the world
    /// </summary>
    public abstract class LocationAbility : IFracAbility
    {
        protected UnitManager caster;
        protected UnityEngine.Vector3 location;
        protected Ability ability;

        protected LocationAbility(UnitManager caster, UnityEngine.Vector3 location, Ability ability)
        {
            this.caster = caster;
            this.location = location;
            this.ability = ability;
        }

        public virtual void ExecuteAbility()
        {
            caster.UseAbility(ability.Name);
            if (caster.IsMine)
            {
                UnitBarkManager.Instance.AbilityBark(ability);
            }
        }
    }

    /// <summary>
    /// A base class for passive abilities that have a chance to proc under certain conditions
    /// </summary>
    public abstract class PassiveAbility : IFracAbility
    {
        protected UnitManager caster;

        protected PassiveAbility(UnitManager caster)
        {
            this.caster = caster;
        }

        /// <summary>
        /// Determines if this ability should execute based on some condition(s)
        /// </summary>
        public virtual bool Proc()
        {
            return true;
        }

        public virtual void ExecuteAbility() { }
    }

    public abstract class PassiveAttackInterrupt : PassiveAbility
    {
        protected UnitManager attacker;

        protected PassiveAttackInterrupt(UnitManager caster, UnitManager attacker)
            : base(caster)
        {
            this.attacker = attacker;
        }
    }

    /// <summary>
    /// A base class for abilities that mutate units by modifying their properties
    /// </summary>
    public abstract class MutatorAbility : IFracAbility
    {
        protected UnitManager Owner;
        public bool MutatesWeapon { get; protected set; }
        
        public MutatorAbility(UnitManager owner)
        {
            Owner = owner;
        }
        
        public virtual void ExecuteAbility() { }
        public virtual void Remove() { }
    }
}