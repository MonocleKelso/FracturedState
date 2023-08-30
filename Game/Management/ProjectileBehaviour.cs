using System.Collections.Generic;
using FracturedState;
using FracturedState.Game;
using FracturedState.Game.Data;
using FracturedState.Game.Management;
using FracturedState.Game.Modules;
using FracturedState.Game.Network;
using UnityEngine;
using UnityEngine.Networking;

public class ProjectileBehaviour : MonoBehaviour
{
    private const float DistanceCheck = 0.1f;

    private UnitManager caster;
    private Team casterTeam;
    private int mask;
    private Weapon parentWeapon;
    private float hitChance;
    private UnitManager targetUnit;
    private Vector3 targetPos;
    private Vector3 startPoint;
    private float startTime;
    private Quaternion explodeRotation;

    private GameObject impactParent;
    private ParticleSystem explosion;

    private bool hitCollider;
    private UnitManager collidedUnit;

    private Vector3 impactPositionLocal;

    private List<UnitManager> hitUnits;

    public bool Primed { get; set; }

    private void Setup(Weapon weapon, UnitManager shooter)
    {
        Primed = false;
        caster = shooter;
        casterTeam = caster.OwnerTeam;
        hitChance = caster.Stats.Accuracy;
        parentWeapon = weapon;
        startPoint = caster.transform.position;
        startTime = Time.time;
        hitCollider = false;
        collidedUnit = null;
        
        if (!string.IsNullOrEmpty(weapon.ProjectileData.ImpactEffect))
        {
            impactParent = transform.Find(weapon.ProjectileData.ImpactEffect).gameObject;
            impactPositionLocal = impactParent.transform.localPosition;
            explosion = impactParent.GetComponent<ParticleSystem>();
            if (parentWeapon.ArchHeight > 0 && explosion != null)
            {
                explodeRotation = explosion.transform.rotation;
            }
            var systems = GetComponentsInChildren<ParticleSystem>();
            foreach (var sys in systems)
            {
                var main = sys.main;
                if (main.simulationSpace == ParticleSystemSimulationSpace.World)
                {
                    main.startRotation = Mathf.Deg2Rad * transform.rotation.eulerAngles.y;
                }
            }
            impactParent.SetActive(false);
        }
        if (FracNet.Instance.IsHost)
        {
            if (weapon.ProjectileData.PassThroughTargets)
            {
                var sc = gameObject.AddComponent<SphereCollider>();
                sc.radius = weapon.DamageRadius;
                sc.isTrigger = true;
                hitUnits = new List<UnitManager>();
            }

            if (shooter != null && shooter.IsOnFirePoint)
            {
                mask = GameConstants.AllUnitMask;
            }
            else
            {
                mask = gameObject.layer == GameConstants.ExteriorLayer ? GameConstants.ExteriorUnitAllMask : GameConstants.InteriorUnitAllMask;
            }
        }
    }
    
    public void Init(Weapon weapon, UnitManager shooter, UnitManager target)
    {
        Setup(weapon, shooter);
        targetUnit = target;
        targetPos = targetUnit.transform.position;
    }

    public void Init(Weapon weapon, UnitManager shooter, Vector3 target)
    {
        Setup(weapon, shooter);
        targetUnit = null;
        targetPos = target;
    }

    private void Update()
    {
        if (parentWeapon.ProjectileData.SeeksTarget && targetUnit != null)
        {
            targetPos = targetUnit.transform.position;
        }
            
        var toTarget = (transform.position - targetPos);
        if (toTarget.magnitude > DistanceCheck && !hitCollider)
        {
            RaycastHit hit;
            if (parentWeapon.ArchHeight > 0)
            {
                var dist = (Time.time - startTime) * parentWeapon.ProjectileData.Speed;
                var t = Mathf.Clamp(dist / toTarget.magnitude, 0, 1);
                Vector3 result;
                if (Mathf.Abs(startPoint.y - targetPos.y) < 0.1f)
                {
                    var travelDirection = targetPos - startPoint;
                    result = startPoint + t * travelDirection;
                    result.y += Mathf.Sin(t * Mathf.PI) * parentWeapon.ArchHeight;
                }
                else
                {
                    var travelDirection = targetPos - startPoint;
                    var levelDirection = targetPos - new Vector3(startPoint.x, targetPos.y, startPoint.z);
                    var right = Vector3.Cross(travelDirection, levelDirection);
                    var up = Vector3.Cross(right, travelDirection);
                    if (targetPos.y > startPoint.y)
                        up = -up;
                    result = startPoint + t * travelDirection;
                    result += (Mathf.Sin(t * Mathf.PI) * parentWeapon.ArchHeight) * up.normalized;
                }

                if (CheckWeaponBlock(result)) return;
                    
                
                if (!parentWeapon.ProjectileData.PassThroughTargets && CollisionCheck(result, out hit))
                {
                    hitCollider = true;
                    transform.LookAt(hit.point);
                    transform.position = hit.point;
                    collidedUnit = hit.collider.transform.GetAbsoluteParent().GetComponent<UnitManager>();
                }
                else
                {
                    transform.LookAt(result);
                    transform.position = result;
                }
            }
            else
            {
                transform.LookAt(targetPos);
                var move = transform.forward * parentWeapon.ProjectileData.Speed * Time.deltaTime;
                move = Vector3.ClampMagnitude(move, toTarget.magnitude);

                if (CheckWeaponBlock(transform.position + move)) return;
                
                if (!parentWeapon.ProjectileData.PassThroughTargets && CollisionCheck(transform.position + move, out hit))
                {
                    hitCollider = true;
                    transform.LookAt(hit.point);
                    transform.position = hit.point;
                    collidedUnit = hit.collider.transform.GetAbsoluteParent().GetComponent<UnitManager>();
                }
                else
                {
                    transform.position += move;
                }
            }
        }
        else
        {
            if (FracNet.Instance.IsHost && !string.IsNullOrEmpty(parentWeapon.ProjectileData.DeathWeapon))
            {
                var w = XmlCacheManager.Weapons[parentWeapon.ProjectileData.DeathWeapon];
                hitChance = w.Accuracy;
                if (w.BlastRadius > 0)
                {
                    var nearby = Physics.OverlapSphere(transform.position, w.BlastRadius, mask);
                    var casterHit = false;
                    foreach (var n in nearby)
                    {
                        var unit = n.GetComponent<UnitManager>();
                        if (unit != null && unit.IsAlive && unit.Transport == null && (unit.OwnerTeam != casterTeam || parentWeapon.DamagesFriendly))
                        {
                            if (unit == caster)
                            {
                                casterHit = true;
                                if (!caster.Data.WeaponDamagesSelf)
                                {
                                    continue;
                                }
                            }

                            int damage;
                            var hit = CalcDeathWeaponDamage(unit, w, out damage);
                            if (hit)
                            {
                                NetworkIdentity id = null;
                                if (caster != null && caster.IsAlive && (caster.Data.IsSelectable || caster.Data.IsGarrisonUnit))
                                {
                                    id = caster.NetMsg.NetworkId;
                                }
                                unit.NetMsg.CmdTakeProjectileDamage(damage, id, transform.position, w.Name);
                            }
                        }
                    }
                    if (!casterHit && caster != null && caster.Data.WeaponDamagesSelf)
                    {
                        if ((transform.position - caster.transform.position).magnitude <= w.BlastRadius)
                        {
                            int damage;
                            var hit = CalcDeathWeaponDamage(caster, w, out damage);
                            if (hit)
                            {
                                caster.NetMsg.CmdTakeProjectileDamage(damage, null, transform.position, w.Name);
                            }
                        }
                    }
                }
            }
            Impact();
        }
    }

    private void Impact()
    {
        if (impactParent != null)
        {
            impactParent.SetActive(true);
            impactParent.transform.parent = null;
            if (parentWeapon.ArchHeight > 0 && explosion != null)
            {
                explosion.transform.rotation = explodeRotation;
            }
            if (explosion != null)
            {
                var life = impactParent.GetComponent<Lifetime>();
                if (life == null)
                    life = impactParent.AddComponent<Lifetime>();
                life.SetLifetime(explosion.main.startLifetime.constant, this);
                life.LocalOffset = impactPositionLocal;
                explosion.Play();
            }
            else
            {
                var life = impactParent.AddComponent<Lifetime>();
                life.SetLifetime(parentWeapon.ProjectileData.ImpactDuration, this);
            }
            var impAudio = impactParent.GetComponent<AudioSource>();
            if (impAudio != null)
            {
                impAudio.volume = ProfileManager.GetEffectsVolumeFromProfile();
                impAudio.pitch = Random.Range(0.8f, 1.2f);
                impAudio.Play();
            }
        }
        var systems = GetComponentsInChildren<ParticleSystem>();
        foreach (var sys in systems)
        {
            sys.Stop();
            sys.Clear();
        }
        var sc = gameObject.GetComponent<SphereCollider>();
        if (sc != null)
        {
            Destroy(sc);
        }
        ObjectPool.Instance.ReturnPooledObject(gameObject);
    }
    
    private bool CheckWeaponBlock(Vector3 newPos)
    {
        var hit = RaycastUtil.RayCheckWeaponBlock(transform.position, newPos);
        if (hit.collider == null) return false;
        var module = hit.collider.GetComponentInParent<NetworkDamageModule>();
        if (module == null) return false;
        var modOwner = module.GetOwner();
        // let units on the same team shoot through blockers
        if (modOwner != null && modOwner.OwnerTeam == caster.OwnerTeam) return false;
        hitCollider = true;
        if (FracNet.Instance.IsHost)
        {
            var weapon = parentWeapon;
            if (!string.IsNullOrEmpty(weapon.ProjectileData.DeathWeapon))
            {
                weapon = XmlCacheManager.Weapons[weapon.ProjectileData.DeathWeapon];
            }
            var damage = weapon.Damage;
            if (weapon.MinDamage > 0)
            {
                damage = Mathf.RoundToInt((damage + weapon.MinDamage) / 2f);
            }
            NetworkIdentity id = null;
            if (caster != null && caster.IsAlive && (caster.Data.IsSelectable || caster.Data.IsGarrisonUnit))
            {
                id = caster.NetMsg.NetworkId;
            }
            module.TakeDamage(damage, id, weapon.Name);
        }
        Impact();
        return true;
    }
    
    private bool CollisionCheck(Vector3 destination, out RaycastHit hit)
    {
        var dir = destination - transform.position;
        var ray = new Ray(transform.position, dir);
        var msk = caster.IsMine ? GameConstants.EnemyUnitMask : GameConstants.FriendlyUnitMask;
        return Physics.Raycast(ray, out hit, dir.magnitude, msk);
    }

    private bool CalcDeathWeaponDamage(UnitManager target, Weapon weapon, out int damage)
    {
        // if we collided with a unit then deal damage as if that unit were in the epicenter
        if (target == collidedUnit)
        {
            damage = target.MitigateDamage(weapon, weapon.Damage, transform.position);
            return true;
        }

        if (weapon.PointBlankRange > 0 && (target.transform.position - transform.position).magnitude < weapon.PointBlankRange)
        {
            var dist = (transform.position - target.transform.position);
            var radDam = Mathf.Lerp(weapon.Damage, weapon.MinDamage, dist.magnitude / weapon.BlastRadius);
            damage = target.MitigateDamage(weapon, Mathf.RoundToInt(radDam), transform.position);
            return true;
        }
        var chanceToHit = Random.Range(0, 101);
        if (chanceToHit > hitChance)
        {
            damage = 0;
            return false;
        }
        if (target.InCover && !weapon.IgnoresCover)
        {
            for (var i = 0; i < target.CurrentCover.CoverPoints.Length; i++)
            {
                if (target.CurrentCover.CoverPoints[i].name == target.CurrentCoverPoint.Name)
                {
                    chanceToHit += target.CurrentCoverPoint.GetBonus(target.CurrentCover.CoverPoints[i], transform.position);
                }
            }
        }
        if (chanceToHit < hitChance)
        {
            var isHit = true;
            if (weapon.NeedsSight)
            {
                var ray = new Ray(transform.position, (target.transform.position - transform.position).normalized);
                isHit = !Physics.Raycast(ray, (target.transform.position - transform.position).magnitude, GameConstants.WorldMask);
            }
            if (isHit)
            {
                var dist = (transform.position - target.transform.position);
                var radDam = Mathf.Lerp(weapon.Damage, weapon.MinDamage, dist.magnitude / weapon.BlastRadius);
                damage = target.MitigateDamage(weapon, Mathf.RoundToInt(radDam), transform.position);
            }
            else
            {
                damage = 0;
            }
            return isHit;
        }

        damage = 0;
        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        var unit = other.GetComponent<UnitManager>();
        if (unit != null && unit.IsAlive && unit.Transport == null)
        {
            if (unit.OwnerTeam == caster.OwnerTeam && !parentWeapon.DamagesFriendly || hitUnits.Contains(unit))
                return;

            var mDamage = unit.MitigateDamage(parentWeapon, transform.position);
            if (caster != null && caster.IsAlive)
            {
                unit.NetMsg.CmdTakeProjectileDamage(mDamage, caster.NetMsg.NetworkId, transform.position, parentWeapon.Name);
            }
            else
            {
                unit.NetMsg.CmdTakeProjectileDamage(mDamage, null, transform.position, parentWeapon.Name);

            }
            hitUnits.Add(unit);
        }
    }
}