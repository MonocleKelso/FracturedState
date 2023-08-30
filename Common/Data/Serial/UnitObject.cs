using System.Linq;
using System.Xml.Serialization;
using FracturedState.Game.AI;
using UnityEngine;

namespace FracturedState.Game.Data
{
    [XmlRoot("unit")]
    public class UnitObject : BaseObject
    {
        public static UnitObject DeepClone(UnitObject u)
        {
            var c = new UnitObject
            {
                Name = u.Name,
                Role = u.Role,
                ExcludeFromEditor = u.ExcludeFromEditor,
                Health = u.Health,
                ArmorName = u.ArmorName,
                CapturePoints = u.CapturePoints,
                IsSelectable = u.IsSelectable,
                IsGarrisonUnit = u.IsGarrisonUnit,
                IsInfantry = u.IsInfantry,
                CanSuppress = u.CanSuppress,
                UseProximity = u.UseProximity,
                SelectionScale = u.SelectionScale,
                StatusIconHeight = u.StatusIconHeight,
                PopulationCost = u.PopulationCost,
                RecruitTime = u.RecruitTime,
                IconName = u.IconName,
                Description = u.Description,
                ShortDescription = u.ShortDescription,
                WeaponName = u.WeaponName,
                CoverWeaponName = u.CoverWeaponName,
                WeaponNeedsSight = u.WeaponNeedsSight,
                WeaponDamagesSelf = u.WeaponDamagesSelf,
                WeaponFireDelay = u.WeaponFireDelay,
                VisionRange = u.VisionRange,
                CanTakeCover = u.CanTakeCover,
                CanTakeFirePoint = u.CanTakeFirePoint,
                CanEnterBuilding = u.CanEnterBuilding,
                CanBePassenger = u.CanBePassenger,
                CoverPriority = u.CoverPriority,
                DamageModule = u.DamageModule,
                LocomotorName = u.LocomotorName
            };

            if (u.Model != null)
            {
                c.Model = new ModelInfo
                {
                    EditorModel = u.Model.EditorModel,
                    ExteriorModel = u.Model.ExteriorModel,
                    InteriorModel = u.Model.InteriorModel
                };
            }

            if (u.BoundsBox != null)
            {
                c.BoundsBox = new BoundingBox
                {
                    Bounds = u.BoundsBox.Bounds,
                    Center = u.BoundsBox.Center
                };
            }
            
            if (u.StatefulEffects != null)
            {
                c.StatefulEffects = new StateEffects
                {
                    IdleEffects = new string[u.StatefulEffects.IdleEffects?.Length ?? 0],
                    MoveEffects = new string[u.StatefulEffects.MoveEffects?.Length ?? 0]
                };
                u.StatefulEffects.IdleEffects?.CopyTo(c.StatefulEffects.IdleEffects, 0);
                u.StatefulEffects.MoveEffects?.CopyTo(c.StatefulEffects.MoveEffects, 0);
            }

            
            if (u.Physics != null)
            {
                c.Physics = new PhysicalProperties()
                {
                    MaxSpeed = u.Physics.MaxSpeed,
                    PathRadius = u.Physics.PathRadius,
                    TurnInPlace = u.Physics.TurnInPlace,
                    TurnRate = u.Physics.TurnRate
                };
            }

            
            if (u.Abilities != null)
            {
                c.Abilities = new string[u.Abilities.Length];
                u.Abilities.CopyTo(c.Abilities, 0);
            }

            if (u.ParticleBones != null)
            {
                c.ParticleBones = new ParticleEffectBones()
                {
                    CoverMuzzleFlash = u.ParticleBones.CoverMuzzleFlash,
                    InteriorMuzzleFlash = u.ParticleBones.InteriorMuzzleFlash,
                    PrimaryMuzzleFlash = u.ParticleBones.PrimaryMuzzleFlash,
                    WeaponHit = u.ParticleBones.WeaponHit
                };
            }

            if (u.OnDeath != null) c.OnDeath = DeathProperties.DeepClone(u.OnDeath);
            if (u.Voices != null) c.Voices = VoiceData.DeepClone(u.Voices);
            if (u.Animations != null) c.Animations = AnimationData.DeepClone(u.Animations);
            if (u.TransportLogic != null)
            {
                c.TransportLogic = new TransportProperties
                {
                    Capacity = u.TransportLogic.Capacity,
                    EntranceName = u.TransportLogic.EntranceName,
                    OccupiedModelName = u.TransportLogic.OccupiedModelName
                };
                c.TransportLogic.PassengerPoints = new string[u.TransportLogic.PassengerPoints?.Length ?? 0];
                u.TransportLogic.PassengerPoints?.CopyTo(c.TransportLogic.PassengerPoints, 0);
            }

            c.PrerequisiteStructures = new string[u.PrerequisiteStructures?.Length ?? 0];
            u.PrerequisiteStructures?.CopyTo(c.PrerequisiteStructures, 0);
            if (u.CustomBehaviours != null)
            {
                c.CustomBehaviours = new CustomClasses
                {
                    TargetClassName = u.CustomBehaviours.TargetClassName,
                    AttackClassName = u.CustomBehaviours.AttackClassName,
                    IdleClassName = u.CustomBehaviours.IdleClassName,
                    MoveClassName = u.CustomBehaviours.MoveClassName,
                    PassengerAttackClassName = u.CustomBehaviours.PassengerAttackClassName
                };
            }
            return c;
        }
        
        [XmlElement("health")]
        public int Health { get; set; }

        [XmlElement("armor")]
        public string ArmorName { get; set; }
        
        [XmlElement("role")]
        public string Role { get; set; }

        [XmlIgnore]
        public Armor ArmorValues { get; set; }

        public int GetDamageResistance(string damageType)
        {
            var defense = ArmorValues?.Defenses?.FirstOrDefault(d => d.DamageType == damageType);
            return defense?.ArmorValue ?? 0;
        }

        public int GetDirectionReduction()
        {
            var reduce = ArmorValues != null && ArmorValues.FrontReduction;
            return reduce ? ArmorValues.FrontReductionAmount : 0;
        }
        
        [XmlElement("isSelectable")]
        public bool IsSelectable { get; set; }

        [XmlElement("isGarrisonUnit")]
        public bool IsGarrisonUnit { get; set; }

        [XmlElement("effects")]
        public StateEffects StatefulEffects { get; set; }

        [XmlElement("isInfantry")]
        public bool IsInfantry { get; set; }
        
        [XmlElement("canSuppress")]
        public bool CanSuppress { get; set; }

        [XmlElement("useProximity")]
        public bool UseProximity { get; set; }

        [XmlElement("selectionScale")]
        public float SelectionScale { get; set; }

        [XmlElement("statusHeight")]
        public float StatusIconHeight { get; set; }

        [XmlElement("capturePoints")]
        public float CapturePoints { get; set; }

        [XmlElement("population")]
        public int PopulationCost { get; set; }

        [XmlElement("recruitTime")]
        public float RecruitTime { get; set; }

        [XmlElement("icon")]
        public string IconName { get; set; }

        private Texture2D icon;
        public Texture2D Icon
        {
            get
            {
                if (icon == null)
                {
                    icon = DataUtil.LoadTexture(IconName);
                }
                return icon;
            }
        }

        public bool IsTransport => TransportLogic != null;

        [XmlElement("description")]
        public string Description { get; set; }

        [XmlElement("shortDescr")]
        public string ShortDescription { get; set; }

        [XmlElement("weapon")]
        public string WeaponName { get; set; }

        [XmlElement("coverWeapon")]
        public string CoverWeaponName { get; set; }

        [XmlIgnore]
        public Weapon WeaponData { get; set; }

        [XmlIgnore]
        public Weapon CoverWeaponData { get; set; }

        [XmlElement("weaponNeedsSight")]
        public bool WeaponNeedsSight { get; set; }

        [XmlElement("weaponDamagesSelf")]
        public bool WeaponDamagesSelf { get; set; }

        [XmlElement("weaponFireDelay")]
        public float WeaponFireDelay { get; set; }

        [XmlElement("physics")]
        public PhysicalProperties Physics { get; set; }

        [XmlElement("vision")]
        public float VisionRange { get; set; }

        [XmlElement("canTakeCover")]
        public bool CanTakeCover { get; set; }

        [XmlElement("canTakeFirePoint")]
        public bool CanTakeFirePoint { get; set; }

        [XmlElement("canEnterBuilding")]
        public bool CanEnterBuilding { get; set; }

        [XmlElement("canBePassenger")]
        public bool CanBePassenger { get; set; }

        [XmlElement("coverPriority")]
        public int CoverPriority { get; set; }

        [XmlArray("abilities")]
        [XmlArrayItem("ability")]
        public string[] Abilities { get; set; }

        [XmlElement("particleEffectBones")]
        public ParticleEffectBones ParticleBones { get; set; }

        [XmlElement("onDeath")]
        public DeathProperties OnDeath { get; set; }

        [XmlElement("voice")]
        public VoiceData Voices { get; set; }

        [XmlElement("animations")]
        public AnimationData Animations { get; set; }

        [XmlElement("transportLogic")]
        public TransportProperties TransportLogic { get; set; }

        [XmlArray("prereqStructures")]
        [XmlArrayItem("structure")]
        public string[] PrerequisiteStructures { get; set; }

        [XmlElement("customBehaviour")]
        public CustomClasses CustomBehaviours { get; set; }

        [XmlElement("damageModule")]
        public string DamageModule { get; set; }
        
        [XmlElement("locomotor")]
        public string LocomotorName { get; set; }
    }

    public class CustomClasses
    {
        [XmlElement("target")]
        public string TargetClassName { get; set; }
        [XmlIgnore]
        ITargettingBehaviour targetBehaviour;
        public ITargettingBehaviour TargetBehaviour
        {
            get
            {
                if (string.IsNullOrEmpty(TargetClassName))
                    return null;

                if (targetBehaviour == null)
                {
                    targetBehaviour = DataUtil.LoadCustomBehaviour<ITargettingBehaviour>(TargetClassName);
                }
                return targetBehaviour;
            }
        }

        [XmlElement("idle")]
        public string IdleClassName { get; set; }

        [XmlElement("attack")]
        public string AttackClassName { get; set; }
        
        [XmlElement("move")]
        public string MoveClassName { get; set; }

        [XmlElement("passengerAttack")]
        public string PassengerAttackClassName { get; set; }
    }

    public class VoiceData
    {
        public static VoiceData DeepClone(VoiceData v)
        {
            var c = new VoiceData
            {
                Select = new string[v.Select?.Length ?? 0],
                Move = new string[v.Move?.Length ?? 0],
                Enter = new string[v.Enter?.Length ?? 0],
                Attack = new string[v.Attack?.Length ?? 0],
                Retreat = new string[v.Retreat?.Length ?? 0],
                TransportEnter = new string[v.TransportEnter?.Length ?? 0],
                Damage = new string[v.Damage?.Length ?? 0],
                Death = new string[v.Death?.Length ?? 0]
            };
            v.Select?.CopyTo(c.Select, 0);
            v.Move?.CopyTo(c.Move, 0);
            v.Enter?.CopyTo(c.Enter, 0);
            v.Attack?.CopyTo(c.Attack, 0);
            v.Retreat?.CopyTo(c.Retreat, 0);
            v.TransportEnter?.CopyTo(c.TransportEnter, 0);
            v.Damage?.CopyTo(c.Damage, 0);
            v.Death?.CopyTo(c.Death, 0);
            return c;
        }
        [XmlArray("select")]
        [XmlArrayItem("clip")]
        public string[] Select { get; set; }

        [XmlArray("move")]
        [XmlArrayItem("clip")]
        public string[] Move { get; set; }

        [XmlArray("enter")]
        [XmlArrayItem("clip")]
        public string[] Enter { get; set; }

        [XmlArray("attack")]
        [XmlArrayItem("clip")]
        public string[] Attack { get; set; }

        [XmlArray("retreat")]
        [XmlArrayItem("clip")]
        public string[] Retreat { get; set; }

        [XmlArray("enterTransport")]
        [XmlArrayItem("clip")]
        public string[] TransportEnter { get; set; }

        [XmlArray("damage")]
        [XmlArrayItem("clip")]
        public string[] Damage { get; set; }

        [XmlArray("death")]
        [XmlArrayItem("clip")]
        public string[] Death { get; set; }
    }

    public class AnimationData
    {
        public static AnimationData DeepClone(AnimationData a)
        {
            var c = new AnimationData
            {
                Idle = new string[a.Idle?.Length ?? 0],
                CrouchAim = new string[a.CrouchAim?.Length ?? 0],
                CrouchFire = new string[a.CrouchFire?.Length ?? 0],
                Move = new string[a.Move?.Length ?? 0],
                StandAim = new string[a.StandAim?.Length ?? 0],
                StandFire = new string[a.StandFire?.Length ?? 0]
            };
            a.Idle?.CopyTo(c.Idle, 0);
            a.CrouchAim?.CopyTo(c.CrouchAim, 0);
            a.CrouchFire?.CopyTo(c.CrouchFire, 0);
            a.Move?.CopyTo(c.Move, 0);
            a.StandAim?.CopyTo(c.StandAim, 0);
            a.StandFire?.CopyTo(c.StandFire, 0);
            return c;
        }
        
        [XmlArray("idle")]
        [XmlArrayItem("anim")]
        public string[] Idle { get; set; }

        [XmlArray("move")]
        [XmlArrayItem("anim")]
        public string[] Move { get; set; }

        [XmlArray("standAim")]
        [XmlArrayItem("anim")]
        public string[] StandAim { get; set; }

        [XmlArray("crouchAim")]
        [XmlArrayItem("anim")]
        public string[] CrouchAim { get; set; }

        [XmlArray("standFire")]
        [XmlArrayItem("anim")]
        public string[] StandFire { get; set; }

        [XmlArray("crouchFire")]
        [XmlArrayItem("anim")]
        public string[] CrouchFire { get; set; }
    }

    public class DeathProperties
    {
        public static DeathProperties DeepClone(DeathProperties d)
        {
            var c = new DeathProperties();
            if (d.DefaultDeathType != null) c.DefaultDeathType = DeathTypeProperties.DeepClone(d.DefaultDeathType);
            if (d.DeathTypes != null)
            {
                c.DeathTypes = new DeathTypeProperties[d.DeathTypes.Length];
                for (int i = 0; i < d.DeathTypes.Length; i++)
                {
                    c.DeathTypes[i] = DeathTypeProperties.DeepClone(d.DeathTypes[i]);
                }
            }

            return c;
        }
        
        [XmlElement("defaultDeath")]
        public DeathTypeProperties DefaultDeathType { get; set; }

        [XmlArray("deathTypes")]
        [XmlArrayItem("deathType")]
        public DeathTypeProperties[] DeathTypes { get; set; }
    }

    public class DeathTypeProperties
    {
        public static DeathTypeProperties DeepClone(DeathTypeProperties d)
        {
            var c = new DeathTypeProperties();
            c.DeathTypeName = d.DeathTypeName;
            c.RagdollName = d.RagdollName;
            c.RagdollRootBone = d.RagdollRootBone;
            c.RagdollLifetime = d.RagdollLifetime;
            c.DeathWeapon = d.DeathWeapon;
            c.ParticleEffect = d.ParticleEffect;
            c.ParticleLifeTime = d.ParticleLifeTime;
            c.GibForce = d.GibForce;
            if (d.GibList != null)
            {
                c.GibList = new GibProperty[d.GibList.Length];
                for (int i = 0; i < d.GibList.Length; i++)
                {
                    c.GibList[i] = new GibProperty()
                    {
                        Count = d.GibList[i].Count,
                        Name = d.GibList[i].Name
                    };
                }
            }
            return c;
        }
        
        [XmlElement("name")]
        public string DeathTypeName { get; set; }

        [XmlElement("ragdoll")]
        public string RagdollName { get; set; }

        [XmlElement("ragdollRootBone")]
        public string RagdollRootBone { get; set; }

        [XmlElement("ragdollLifetime")]
        public float RagdollLifetime { get; set; }

        [XmlElement("deathWeapon")]
        public string DeathWeapon { get; set; }

        [XmlElement("particleEffect")]
        public string ParticleEffect { get; set; }

        [XmlElement("particleLifetime")]
        public float ParticleLifeTime { get; set; }

        [XmlElement("gibForce")]
        public float GibForce { get; set; }

        [XmlArray("gibs")]
        [XmlArrayItem("gib")]
        public GibProperty[] GibList { get; set; }
    }

    public class GibProperty
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("count")]
        public int Count { get; set; }
    }

    public class PhysicalProperties
    {
        [XmlElement("maxSpeed")]
        public float MaxSpeed { get; set; }

        [XmlElement("turnRate")]
        public float TurnRate { get; set; }

        [XmlElement("radius")]
        public float PathRadius { get; set; }

        [XmlElement("turnInPlace")]
        public bool TurnInPlace { get; set; }
    }

    public class ParticleEffectBones
    {
        [XmlElement("primaryMuzzleFlash")]
        public string PrimaryMuzzleFlash { get; set; }

        [XmlElement("interiorMuzzleFlash")]
        public string InteriorMuzzleFlash { get; set; }

        [XmlElement("coverMuzzleFlash")]
        public string CoverMuzzleFlash { get; set; }

        [XmlElement("weaponHitFX")]
        public string WeaponHit { get; set; }
    }

    public class StateEffects
    {
        [XmlArray("idle")]
        [XmlArrayItem("effect")]
        public string[] IdleEffects { get; set; }

        [XmlArray("move")]
        [XmlArrayItem("effect")]
        public string[] MoveEffects { get; set; }
    }

    public class TransportProperties
    {
        [XmlElement("occupiedModel")]
        public string OccupiedModelName { get; set; }

        [XmlElement("capacity")]
        public int Capacity { get; set; }

        [XmlArray("passengerPoints")]
        [XmlArrayItem("point")]
        public string[] PassengerPoints { get; set; }

        [XmlElement("enterPoint")]
        public string EntranceName { get; set; }
    }
}