using ExitGames.Client.Photon.LoadBalancing;
using FracturedState.Game.Data;
using FracturedState.Game.Management;
using UnityEngine;

namespace FracturedState.Game.Modules
{
    /// <summary>
    /// The base class of any handler for an entity that can take damage and be destroyed
    /// </summary>
    public abstract class DamageModule : MonoBehaviour
    {
        #region Properties
        public int CurrentHealth { get; protected set; }
        public int MaxHealth { get; protected set; }
        public bool IsAlive => CurrentHealth > 0;

        #endregion
        #region Fields
        protected UnitManager unitManager;
        #endregion

        #region Unity Methods
        protected virtual void Awake()
        {
            unitManager = GetComponent<UnitManager>();
            CurrentHealth = unitManager.Data.Health;
            MaxHealth = CurrentHealth;
        }
        #endregion

        #region Static Methods
        public static DeathTypeProperties GetDeathType(Weapon weapon, UnitObject targetData)
        {
            if (weapon != null && targetData?.OnDeath != null)
            {
                if (string.IsNullOrEmpty(weapon.DeathType))
                    return targetData.OnDeath.DefaultDeathType;

                if (targetData.OnDeath.DeathTypes != null)
                {
                    for (var i = 0; i < targetData.OnDeath.DeathTypes.Length; i++)
                    {
                        var dt = targetData.OnDeath.DeathTypes[i];
                        if (dt.DeathTypeName == weapon.DeathType)
                        {
                            return dt;
                        }
                    }
                }
            }
            return null;
        }
        #endregion

        #region Public Methods
        public virtual void TakeDamage(int damage, UnitManager attacker, string weapon)
        {
            if (!IsAlive) return;

            CurrentHealth -= damage;
            ShowDamageHelper(damage);
            var w = GetWeapon(weapon);
            w.PostHit?.DoEffect(attacker, unitManager, w);
            if (!IsAlive)
            {
                unitManager.CleanUp();
            }
        }

        public virtual void TakeProjectileDamage(int damage, UnitManager attacker, Vector3 projectilePosition, string weapon)
        {
            TakeDamage(damage, attacker, weapon);
        }

        public virtual void Heal(int amount)
        {
            CurrentHealth += amount;
            if (CurrentHealth > MaxHealth)
            {
                CurrentHealth = MaxHealth;
            }
            var helper = ObjectPool.Instance.GetHealHelper(transform.position + Vector3.up * unitManager.Data.StatusIconHeight);
            helper.GetComponent<DamageHelper>().Init(amount);
        }

        public virtual void Miss(UnitManager attacker)
        {
            if (attacker != null && attacker.Data.WeaponData != null)
            {
                var fxString = attacker.Data.WeaponData.MissEffect;
                if (fxString == null) return;
                
                var missFx = ParticlePool.Instance.GetSystem(fxString);
                var unitPos = transform.position;
                var variance = (100f - attacker.Data.WeaponData.Accuracy) * 0.1f;
                unitPos.x += Random.Range(-variance, variance);
                unitPos.z += Random.Range(-variance, variance);
                if (unitManager.CurrentCover != null)
                {
                    missFx.transform.position = unitManager.CurrentCover.GetComponent<Collider>()
                        .ClosestPointOnBounds(attacker.transform.position);
                }
                else
                {
                    missFx.transform.position = unitPos;
                }
                
                missFx.gameObject.SetLayerRecursively(gameObject.layer);
                missFx.Play();
            }
        }
        #endregion

        /// <summary>
        /// Returns the data for the weapon with the given name or the data associated with the universal dummy weapon if
        /// null or an empty string are passed
        /// </summary>
        protected Weapon GetWeapon(string weaponName)
        {
            return XmlCacheManager.Weapons[string.IsNullOrEmpty(weaponName) ? Weapon.DummyName : weaponName];
        }

        /// <summary>
        /// Creates a damage indicator at the unit's position and displays the given damage amount
        /// </summary>
        private void ShowDamageHelper(int damageAmount)
        {
            if ((unitManager.IsMine && unitManager.Data.IsSelectable) || SkirmishVictoryManager.IsSpectating || VisibilityChecker.Instance.IsVisible(unitManager))
            {
                var helper = ObjectPool.Instance.GetDamageHelper(transform.position + Vector3.up * unitManager.Data.StatusIconHeight);
                helper.GetComponent<DamageHelper>().Init(damageAmount);
                
                // flash a selection indicator with updated health color for visible non selected units (friendly and enemy)
                if (!SelectionManager.Instance.SelectedUnits.Contains(unitManager))
                {
                    var proj = ObjectPool.Instance.GetSelectionProjector();
                    proj.transform.GetChild(0).localScale = Vector3.one * unitManager.Data.SelectionScale;
                    proj.transform.position = unitManager.transform.position;
                    var follow = proj.GetComponent<SelectionProjectorFollow>();
                    follow.SetTarget(unitManager);
                    follow.DoFade();
                    proj.SetActive(true);
                }
            }
        }
        
        public virtual void SetOwner(UnitManager owner) { }
        public virtual UnitManager GetOwner()
        {
            return null;
        }
    }
}
