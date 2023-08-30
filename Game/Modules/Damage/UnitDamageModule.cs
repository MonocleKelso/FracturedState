using FracturedState.Game.Data;
using FracturedState.Game.Network;
using UnityEngine;

namespace FracturedState.Game.Modules
{
    /// <summary>
    /// The default damage module for units
    /// </summary>
    public class UnitDamageModule : DamageModule
    {
        #region Public Methods
        public override void TakeDamage(int damage, UnitManager attacker, string weapon)
        {
            if (!IsAlive) return;
            base.TakeDamage(damage, attacker, weapon);
            var w = GetWeapon(weapon);
            var deathType = DoUnitDamage(w, attacker);
            if (!string.IsNullOrEmpty(deathType?.RagdollName))
            {
                Ragdoll(attacker, w, deathType);
            }
        }

        public override void TakeProjectileDamage(int damage, UnitManager attacker, Vector3 projectilePosition, string weapon)
        {
            if (!IsAlive) return;
            base.TakeProjectileDamage(damage, attacker, projectilePosition, weapon);
            var w = GetWeapon(weapon);
            var deathType = DoUnitDamage(w, attacker);
            if (!string.IsNullOrEmpty(deathType?.RagdollName))
            {
                Ragdoll(attacker, w, deathType, projectilePosition);
            }
        }

        public override void Miss(UnitManager attacker)
        {
            if (!IsAlive) return;
            base.Miss(attacker);
        }
        #endregion

        /// <summary>
        /// Process the actions a unit takes as a result of being dealt damage and returns a death type if the unit was killed
        /// </summary>
        protected DeathTypeProperties DoUnitDamage(Weapon weapon, UnitManager attacker)
        {
            PlayDamageEffect(weapon);
            Bark();
            if (IsAlive) return null;
            
            DisableUnitRenderers();
            if (unitManager.Data.OnDeath == null) return null;
            
            var deathType = GetDeathType(weapon, unitManager.Data);
            if (deathType == null) return null;
            
            DeathEffects(deathType);
            if (string.IsNullOrEmpty(deathType.DeathWeapon)) return deathType;
            
            // host fires death weapon if necessary
            var deathWeapon = GetWeapon(deathType.DeathWeapon);
            FireDeathWeapon(deathWeapon);
            return deathType;
        }

        /// <summary>
        /// Plays barks for damage or death if the unit is owned by the player
        /// </summary>
        protected void Bark()
        {
            if (!unitManager.IsMine) return;
            
            if (IsAlive)
            {
                if (!UnitBarkManager.Instance.DamageBarkRoll()) return;
                
                var screenPos = Camera.main.WorldToScreenPoint(transform.position);
                if (screenPos.x > 0 && screenPos.x < Screen.width && screenPos.y > 0 && screenPos.y < Screen.height)
                {
                    UnitBarkManager.Instance.TakeDamageBark(unitManager.Data);
                }
            }
            else
            {
                var screenPos = Camera.main.WorldToScreenPoint(transform.position);
                if (screenPos.x < 0 || screenPos.x > Screen.width || screenPos.y < 0 || screenPos.y > Screen.height)
                {
                    UnitBarkManager.Instance.DeathBark(unitManager.Data);
                }
            }
        }

        /// <summary>
        /// Fires the given weapon as a death weapon at the unit's position. This method does nothing if called on a non-host simulation
        /// </summary>
        protected void FireDeathWeapon(Weapon weapon)
        {
            if (!FracNet.Instance.IsHost) return;

            var layerMask = unitManager.WorldState == Nav.State.Exterior ? GameConstants.ExteriorUnitAllMask : GameConstants.InteriorUnitAllMask;
            var nearby = Physics.OverlapSphere(transform.position, weapon.BlastRadius, layerMask);
            for (var i = 0; i < nearby.Length; i++)
            {
                var unit = nearby[i].GetComponent<UnitManager>();
                if (unit != null && unit.IsAlive && unit != unitManager)
                {
                    if (!weapon.DamagesFriendly && unit.OwnerTeam == unitManager.OwnerTeam) continue;

                    int damage;
                    if (weapon.PointBlankRange > 0 && (transform.position - unit.transform.position).magnitude < weapon.PointBlankRange)
                    {
                        var dist = (transform.position - unit.transform.position);
                        var radDam = Mathf.Lerp(weapon.Damage, weapon.MinDamage, dist.magnitude / weapon.BlastRadius);
                        damage = unit.MitigateDamage(weapon, Mathf.RoundToInt(radDam), unitManager.transform.position);
                    }
                    else
                    {
                        var chanceToHit = Random.Range(0, 101);
                        if (unit.InCover && !weapon.IgnoresCover)
                        {
                            for (var c = 0; c < unit.CurrentCover.CoverPoints.Length; c++)
                            {
                                if (unit.CurrentCover.CoverPoints[c].name == unit.CurrentCoverPoint.Name)
                                {
                                    chanceToHit += unit.CurrentCoverPoint.GetBonus(unit.CurrentCover.CoverPoints[c], transform.position);
                                }
                            }
                        }
                        if (chanceToHit < weapon.Accuracy)
                        {
                            var dist = (transform.position - unit.transform.position);
                            var radDam = Mathf.Lerp(weapon.Damage, weapon.MinDamage, dist.magnitude / weapon.BlastRadius);
                            damage = unit.MitigateDamage(weapon, Mathf.RoundToInt(radDam), unitManager.transform.position);
                        }
                        else
                        {
                            damage = 0;
                        }
                    }
                    if (damage > 0)
                    {
                        unit.NetMsg.CmdTakeProjectileDamage(damage, null, transform.position, weapon.Name);
                    }
                }
            }
        }

        /// <summary>
        /// Creates a particle system attached to the 'hit bone' of the unit based on the hit effect property of the attacking weapon
        /// </summary>
        protected void PlayDamageEffect(Weapon weapon)
        {
            if (unitManager.WeaponHitBone != null && !string.IsNullOrEmpty(weapon.HitEffect))
            {
                var hit = ParticlePool.Instance.GetSystem(weapon.HitEffect);
                hit.transform.position = unitManager.WeaponHitBone.position;
                hit.transform.rotation = unitManager.WeaponHitBone.rotation;
                hit.gameObject.SetLayerRecursively(gameObject.layer);
                hit.Play();
            }
        }

        /// <summary>
        /// Disables all the renderers on the unit so that it can be visually swapped with any effects
        /// </summary>
        protected void DisableUnitRenderers()
        {
            var renders = GetComponentsInChildren<Renderer>();
            for (var i = 0; i < renders.Length; i++)
            {
                renders[i].enabled = false;
            }
        }

        /// <summary>
        /// Creates any particle system effects or gibs when the unit dies based on what death type was triggered
        /// </summary>
        protected void DeathEffects(DeathTypeProperties deathType)
        {
            // fire particle effect if applicable
            if (!string.IsNullOrEmpty(deathType.ParticleEffect))
            {
                var sys = ObjectPool.Instance.GetPooledObject(deathType.ParticleEffect);
                sys.transform.position = transform.position;
                sys.transform.rotation = transform.rotation;
                sys.SetLayerRecursively(gameObject.layer);
                var life = sys.GetComponent<Lifetime>();
                if (life == null)
                {
                    life = sys.AddComponent<Lifetime>();
                }
                life.SetLifetime(deathType.ParticleLifeTime);
                life.SetPool(true);
                sys.SetLayerRecursively(gameObject.layer);
                if (sys.GetComponent<AudioSource>() != null)
                {
                    sys.GetComponent<AudioSource>().Play();
                }
            }
            // create gibs if applicable
            if (deathType.GibList != null)
            {
                for (var i = 0; i < deathType.GibList.Length; i++)
                {
                    for (var c = 0; c < deathType.GibList[i].Count; c++)
                    {
                        var gibName = DataLocationConstants.BuiltInGibDirectory + deathType.GibList[i].Name;
                        var gib = ObjectPool.Instance.GetPooledObjectAtLocation(gibName, transform.position + Random.insideUnitSphere);
                        gib.SetLayerRecursively(GameConstants.RagdollLayer);
                        var r = gib.GetComponent<Rigidbody>();
                        if (r != null)
                        {
                            r.AddForce(Vector3.up * deathType.GibForce, ForceMode.Impulse);
                            r.AddExplosionForce(deathType.GibForce, transform.position, 5);
                        }
                        var life = gib.GetComponent<Lifetime>();
                        if (life == null)
                        {
                            life = gib.AddComponent<Lifetime>();
                        }
                        life.SetPool(true);
                        life.SetLifetime(ConfigSettings.Instance.Values.GibLifetime);
                    }
                }
            }
        }

        #region Ragdoll
        /// <summary>
        /// Creates a new ragdoll, places it at the unit's position, and sets its lifetime for pooling
        /// </summary>
        protected GameObject PrepRagdoll(string ragdollName, float ragdollLifetime)
        {
            var ragdoll = DataUtil.LoadBuiltInRagdoll(ragdollName);
            ragdoll.transform.position = transform.position;
            ragdoll.transform.rotation = transform.rotation;
            var lf = ragdoll.AddComponent<Lifetime>();
            lf.SetLifetime(ragdollLifetime);
            var audio = ragdoll.GetComponent<AudioSource>();
            if (audio != null)
            {
                audio.volume *= ProfileManager.GetEffectsVolumeFromProfile();
                audio.pitch = Random.Range(0.80f, 1.2f);
            }
            return ragdoll;
        }

        /// <summary>
        /// Creates a ragdoll and applies force to it based on the attacker's position and the impact force of the killing weapon
        /// </summary>
        protected void Ragdoll(UnitManager attacker, Weapon weapon, DeathTypeProperties deathType)
        {
            var ragdoll = PrepRagdoll(deathType.RagdollName, deathType.RagdollLifetime);
            if (attacker != null)
            {
                var rbs = ragdoll.GetComponentsInChildren<Rigidbody>();
                for (var i = 0; i < rbs.Length; i++)
                {
                    if (rbs[i].name == deathType.RagdollRootBone)
                    {
                        var force = (transform.position - attacker.transform.position).normalized * weapon.ImpactForce;
                        rbs[i].AddForceAtPosition(force, rbs[i].position, ForceMode.Acceleration);
                    }
                }
            }
        }

        /// <summary>
        /// Creates a ragdoll and applies force to it based on where the projectile that killed the unit landed
        /// </summary>
        protected void Ragdoll(UnitManager attacker, Weapon weapon, DeathTypeProperties deathType, Vector3 projectilePosition)
        {
            var ragdoll = PrepRagdoll(deathType.RagdollName, deathType.RagdollLifetime);
            if (attacker != null)
            {
                var rbs = ragdoll.GetComponentsInChildren<Rigidbody>();
                for (var i = 0; i < rbs.Length; i++)
                {
                    if (rbs[i].name == deathType.RagdollRootBone)
                    {
                        rbs[i].AddExplosionForce(weapon.ImpactForce, projectilePosition, weapon.BlastRadius);
                    }
                }
            }
        }
        #endregion
    }
}