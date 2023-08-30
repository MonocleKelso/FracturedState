using System.Xml.Serialization;

namespace FracturedState.Game.Data
{
    [XmlRoot("damageTypes")]
    public class DamageTypeList
    {
        [XmlElement("damageType")]
        public DamageType[] DamageTypes { get; set; }
    }

    public class DamageType
    {
        [XmlElement("name")]
        public string Name { get; set; }
    }

    [XmlRoot("weapons")]
    public class WeaponList
    {
        [XmlElement( "weapon")]
        public Weapon[] Weapons { get; set; }
    }

    public class Weapon
    {
        public const string DummyName = "Dummy";

        public Weapon() {}

        public Weapon(Weapon orig)
        {
            Name = orig.Name;
            DamageType = orig.DamageType;
            Range = orig.Range;
            PointBlankRange = orig.PointBlankRange;
            FireRate = orig.FireRate;
            Accuracy = orig.Accuracy;
            ArchHeight = orig.ArchHeight;
            MinDamage = orig.MinDamage;
            Damage = orig.Damage;
            DamagesFriendly = orig.DamagesFriendly;
            NeedsSight = orig.NeedsSight;
            IgnoresCover = orig.IgnoresCover;
            MuzzleRadius = orig.MuzzleRadius;
            BlastRadius = orig.BlastRadius;
            DamageRadius = orig.DamageRadius;
            SoundEffects = orig.SoundEffects;
            MuzzleFlashEffect = orig.MuzzleFlashEffect;
            MissEffect = orig.MissEffect;
            HitEffect = orig.HitEffect;
            ImpactForce = orig.ImpactForce;
            
            if (orig.ProjectileData != null)
            {

                ProjectileData = new Projectile
                {
                    DeathWeapon = orig.ProjectileData.DeathWeapon,
                    ImpactDuration = orig.ProjectileData.ImpactDuration,
                    ImpactEffect = orig.ProjectileData.ImpactEffect,
                    Model = orig.ProjectileData.Model,
                    PassThroughTargets = orig.ProjectileData.PassThroughTargets,
                    SeeksTarget = orig.ProjectileData.SeeksTarget,
                    Speed = orig.ProjectileData.Speed
                };
            }

            DeathType = orig.DeathType;
            
        }
        
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlElement("damageType")]
        public string DamageType { get; set; }
		
		[XmlElement("range")]
		public int Range { get; set; }

        [XmlElement("pointBlankRange")]
        public float PointBlankRange { get; set; }
		
		[XmlElement("fireRate")]
		public float FireRate { get; set; }

        [XmlElement("accuracy")]
        public int Accuracy { get; set; }

        [XmlElement("arcHeight")]
        public float ArchHeight { get; set; }

        [XmlElement("minDamage")]
        public int MinDamage { get; set; }

		[XmlElement("damage")]
		public int Damage { get; set; }

        [XmlElement("damagesFriendly")]
        public bool DamagesFriendly { get; set; }

        [XmlElement("needsSight")]
        public bool NeedsSight { get; set; }

        [XmlElement("ignoresCover")]
        public bool IgnoresCover { get; set; }

        [XmlElement("muzzleRadius")]
        public float MuzzleRadius { get; set; }

        [XmlElement("blastRadius")]
        public float BlastRadius { get; set; }

        [XmlElement("damageRadius")]
        public float DamageRadius { get; set; }

        [XmlArray("soundEffects")]
        [XmlArrayItem("soundEffect")]
        public string[] SoundEffects { get; set; }

        [XmlElement("muzzleFlash")]
        public string MuzzleFlashEffect { get; set; }

        [XmlElement("missEffect")]
        public string MissEffect { get; set; }

        [XmlElement("hitEffect")]
        public string HitEffect { get; set; }

        [XmlElement("impactForce")]
        public float ImpactForce { get; set; }

        [XmlElement("projectile")]
        public Projectile ProjectileData { get; set; }

        [XmlElement("deathType")]
        public string DeathType { get; set; }

        [XmlElement("postHit")]
        public string PostHitName { get; set; }

        [XmlIgnore]
        private AI.IWeaponPostEffect postHit;

        [XmlIgnore]
        public AI.IWeaponPostEffect PostHit
        {
            get
            {
                if (string.IsNullOrEmpty(PostHitName))
                    return null;

                if (postHit == null)
                {
                    postHit = DataUtil.LoadCustomBehaviour<AI.IWeaponPostEffect>(PostHitName);
                }

                return postHit;
            }
        }
    }

    public class Projectile
    {
        [XmlElement("model")]
        public string Model { get; set; }

        [XmlElement("speed")]
        public float Speed { get; set; }

        [XmlElement("seeksTarget")]
        public bool SeeksTarget { get; set; }

        [XmlElement("passThroughTargets")]
        public bool PassThroughTargets { get; set; }

        [XmlElement("deathWeapon")]
        public string DeathWeapon { get; set; }

        [XmlElement("impactEffect")]
        public string ImpactEffect { get; set; }

        [XmlElement("impactDuration")]
        public float ImpactDuration { get; set; }
    }

    [XmlRoot("armors")]
    public class ArmorList
    {
        [XmlElement("armor")]
        public Armor[] Armors { get; set; }
    }

    public class Armor
    {
        [XmlAttribute("name")]
        public string Name { get; set; }
        
        [XmlElement("frontReduction")]
        public bool FrontReduction { get; set; }
        
        [XmlElement("frontReductionAmount")]
        public int FrontReductionAmount { get; set; }

        [XmlArray("defs")]
        [XmlArrayItem(ElementName = "def")]
        public ArmorDefense[] Defenses { get; set; }
    }

    public class ArmorDefense
    {
        [XmlAttribute("damageType")]
        public string DamageType { get; set; }

        [XmlAttribute("armorValue")]
        public int ArmorValue { get; set; }
    }
}