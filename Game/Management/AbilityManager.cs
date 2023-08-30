using System.Collections.Generic;
using System.Linq;
using FracturedState.Game.Data;
using UnityEngine;

namespace FracturedState.Game
{
    public class AbilityManager
    {
        private Dictionary<string, Ability> abilities;
        private Dictionary<string, float> cooldowns;
        private string[] keys;

        public Ability[] PassiveAbilities { get; private set; }

        public AbilityManager()
        {
            cooldowns = new Dictionary<string, float>();
            abilities = new Dictionary<string, Ability>();
        }

        public AbilityManager(string[] abilityNames) : this()
        {
            for (var i = 0; i < abilityNames.Length; i++)
            {
                abilities[abilityNames[i]] = new Ability(XmlCacheManager.Abilities[abilityNames[i]]);
            }
            PassiveAbilities = abilities.Values.Where(a => a.Type == AbilityType.PassivePerUnit || a.Type == AbilityType.PassivePerSquad).OrderByDescending(a => a.Priority).ToArray();
        }

        public bool AddAbility(string ability)
        {
            if (!abilities.ContainsKey(ability))
            {
                abilities[ability] = new Ability(XmlCacheManager.Abilities[ability]);
                PassiveAbilities = abilities.Values.Where(a => a.Type == AbilityType.PassivePerUnit || a.Type == AbilityType.PassivePerSquad).OrderByDescending(a => a.Priority).ToArray();
                return true;
            }

            return false;
        }

        public void RemoveAbility(string ability)
        {
            if (abilities.ContainsKey(ability))
            {
                abilities.Remove(ability);
                PassiveAbilities = abilities.Values.Where(a => a.Type == AbilityType.PassivePerUnit || a.Type == AbilityType.PassivePerSquad).OrderByDescending(a => a.Priority).ToArray();
            }
        }

        public bool HasAbility(string ability)
        {
            return abilities.ContainsKey(ability);
        }

        public void UseAbility(string abilityName)
        {
            Ability a;
            if (abilities.TryGetValue(abilityName, out a))
            {
                if (a.CooldownTime > 0)
                {
                    cooldowns[a.Name] = a.CooldownTime;
                    keys = cooldowns.Keys.ToArray();
                }
            }
        }

        public Ability[] GetUnitAbilities()
        {
            return abilities.Values.Where(a => a.Type == AbilityType.PerUnit).ToArray();
        }

        public Ability[] GetSquadAbilities()
        {
            return abilities.Values.Where(a => a.Type == AbilityType.PerSquad).ToArray();
        }

        public float GetRemainingCooldown(string abilityName)
        {
            if (cooldowns.ContainsKey(abilityName))
            {
                return cooldowns[abilityName];
            }
            return 0;
        }

        public Ability GetAbilityData(string abilityName)
        {
            if (!HasAbility(abilityName))
                return null;

            return abilities[abilityName];
        }

        public void Update()
        {
            if (keys == null || keys.Length == 0)
                return;

            for (var i = 0; i < keys.Length; i++)
            {
                float t = cooldowns[keys[i]] - Time.deltaTime;
                if (t < 0)
                {
                    cooldowns.Remove(keys[i]);
                }
                else
                {
                    cooldowns[keys[i]] = t;
                }
            }
            keys = cooldowns.Keys.ToArray();
        }
    }
}