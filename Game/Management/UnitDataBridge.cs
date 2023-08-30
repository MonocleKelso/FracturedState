using FracturedState.Game.Data;
using System.Collections.Generic;
using UnityEngine;

namespace FracturedState.Game.Management
{
    public enum BuffType
    {
        None,
        Root,
        Accuracy,
        Heal,
        Bleed,
        Defense
    }

    public struct Buff
    {
        public static readonly string[] BuffTypeNames = { "", "Root", "Acc", "Heal", "Bleed", "Def" };

        public readonly BuffType Type;
        public readonly float Amount;
        public readonly float Duration;
        public float TimeStarted { get; }
        public string EffectName { get; }

        public Buff(BuffType type, float amount) : this()
        {
            Type = type;
            Amount = amount;
            Duration = 0;
            TimeStarted = Time.time;
            EffectName = string.Empty;
        }

        public Buff (BuffType type, float amount, float duration) : this()
        {
            Type = type;
            Amount = amount;
            Duration = duration;
            TimeStarted = Time.time;
            EffectName = string.Empty;
        }

        public Buff(BuffType type, float amount, float duration, string effect) : this()
        {
            Type = type;
            Amount = amount;
            Duration = duration;
            TimeStarted = Time.time;
            EffectName = effect;
        }
    }

	public class UnitDataBridge
	{
		private UnitObject unitData;
        private readonly UnitManager owner;

        private readonly List<Buff> buffs = new List<Buff>();

		public int MaxHealth { get; private set; }
        public bool Rooted { get; private set; }

	    public float Accuracy
        {
            get
            {
                var weapon = owner.ContextualWeapon;
                if (weapon == null)
                    return 0;

                float accuracy = weapon.Accuracy;
                for (var i = 0; i < buffs.Count; i++)
                {
                    if (buffs[i].Type == BuffType.Accuracy)
                    {
                        accuracy += (float)weapon.Accuracy * (buffs[i].Amount / 100f);
                    }
                }
                return accuracy;
            }
        }

        public float HealModifier
        {
            get
            {
                float amount = 0;
                for (var i = 0; i < buffs.Count; i++)
                {
                    if (buffs[i].Type == BuffType.Heal)
                    {
                        amount += buffs[i].Amount;
                    }
                }
                return amount;
            }
        }

        public float ArmorModifier
        {
            get
            {
                float amount = 0;
                foreach (var buff in buffs)
                {
                    if (buff.Type != BuffType.Defense) continue;

                    amount += buff.Amount;
                }

                return amount;
            }
        }

		public UnitDataBridge(string unitName, UnitManager owner)
		{
            this.owner = owner;
			SetData(unitName);
		}

        public void Update()
        {
            for (var i = buffs.Count - 1; i >= 0; i--)
            {
                if (Time.time - buffs[i].Duration > buffs[i].TimeStarted)
                {
                    if (!string.IsNullOrEmpty(buffs[i].EffectName))
                    {
                        var fx = owner.transform.Find(buffs[i].EffectName);
                        if (fx != null)
                        {
                            fx.parent = null;
                            Object.Destroy(fx.gameObject);
                        }
                    }
                    if (buffs[i].Type == BuffType.Root)
                    {
                        Rooted = false;
                    }
                    buffs.RemoveAt(i);
                }
            }
        }
		
        public void AddBuff(Buff buff)
        {
            if (buff.Type == BuffType.Bleed)
            {
                var bleed = owner.gameObject.GetComponent<BleedTicker>();
                if (bleed == null)
                {
                    bleed = owner.gameObject.AddComponent<BleedTicker>();
                    bleed.Init(buff.Duration, (int)buff.Amount);
                    Helper(buff);
                }
                return;
            }
            
            var found = false;
            for (var i = 0; i < buffs.Count; i++)
            {
                if (buffs[i].Type != buff.Type) continue;
                
                found = true;
                var timeElapsed = Time.time - buffs[i].TimeStarted;
                if (buffs[i].Amount < buff.Amount || (buffs[i].Duration - timeElapsed) < buff.Duration)
                {
                    if (!string.IsNullOrEmpty(buffs[i].EffectName))
                    {
                        var fx = owner.transform.Find(buffs[i].EffectName);
                        if (fx != null)
                        {
                            fx.parent = null;
                            Object.Destroy(fx.gameObject);
                        }
                    }
                    buffs[i] = buff;
                }
            }

            if (!found)
            {
                Helper(buff);
                if (buff.Type == BuffType.Root)
                {
                    Rooted = true;
                }
                if (!string.IsNullOrEmpty(buff.EffectName))
                {
                    var fx = DataUtil.LoadBuiltInParticleSystem(buff.EffectName);
                    if (fx != null)
                    {
                        fx.gameObject.SetLayerRecursively(owner.gameObject.layer);
                        fx.transform.position = owner.transform.position;
                        fx.transform.parent = owner.transform;
                    }
                }
                buffs.Add(buff);
            }
        }
        
	    private void Helper(Buff buff)
	    {
	        if (buff.Type == BuffType.None) return;
	        
	        var helper = ObjectPool.Instance.GetBuffHelper(owner.transform.position);
	        helper.GetComponent<BuffIndicator>().ApplyBuff(Buff.BuffTypeNames[(int)buff.Type], buff.Amount);
	    }

		public void SetData(string unitName)
		{
			unitData = XmlCacheManager.Units[unitName];
			MaxHealth = unitData.Health;
		}
	}
}