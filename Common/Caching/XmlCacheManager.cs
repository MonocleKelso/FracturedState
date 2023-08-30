using System;
using System.IO;
using System.Linq;
using FracturedState.Game.Data;

namespace FracturedState.Game
{
    /// <summary>
    /// A static container class for all XML data caches used by the game.
    /// This class facilitates data loading and data validation by resolving references
    /// between different data types.  It also provides getter access to those caches.
    /// </summary>
    public static class XmlCacheManager
    {
        private static readonly ModdableXmlCache<string, DamageType> DamageTypes = new ModdableXmlCache<string,DamageType>();
		
        public static ModdableXmlCache<string, Weapon> Weapons { get; } = new ModdableXmlCache<string, Weapon>();
	    public static ModdableXmlCache<string, Armor> Armors { get; } = new ModdableXmlCache<string, Armor>();
	    public static ModdableXmlCache<string, UnitObject> Units { get; } = new ModdableXmlCache<string, UnitObject>();
	    public static ModdableXmlCache<string, Structure> Structures { get; } = new ModdableXmlCache<string, Structure>();
	    public static ModdableXmlCache<string, Faction> Factions { get; } = new ModdableXmlCache<string, Faction>();
	    public static ModdableXmlCache<string, Prop> Props { get; } = new ModdableXmlCache<string, Prop>();
	    public static ModdableXmlCache<string, Ability> Abilities { get; } = new ModdableXmlCache<string, Ability>();
	    public static ModdableXmlCache<string, HouseColor> HouseColors { get; } = new ModdableXmlCache<string, HouseColor>();

        /// <summary>
        /// Loads, deserializes, and validates all XML-based data for the game.
        /// </summary>
        public static void PopulateAllCaches()
        {
            // house colors
            PopulateCache<string, HouseColor, HouseColorList>(HouseColors,
                DataLocationConstants.HouseColorDataFile,
                DataValidation.HouseColorValidation.PrimaryKey,
                DataValidation.HouseColorValidation.ListName);

            // damage types
            PopulateCache<string, DamageType, DamageTypeList>(DamageTypes,
                DataLocationConstants.DamageTypeDataFile,
                DataValidation.DamageValidation.PrimaryKey,
                DataValidation.DamageValidation.ListName);

            // weapons
            PopulateCache<string, Weapon, WeaponList>(Weapons, 
                DataLocationConstants.WeaponDataFile,
                DataValidation.WeaponValidation.PrimaryKey,
                DataValidation.WeaponValidation.ListName);

            // armor
            PopulateCache<string, Armor, ArmorList>(Armors,
                DataLocationConstants.ArmorDataFile,
                DataValidation.ArmorValidation.PrimaryKey,
                DataValidation.ArmorValidation.ListName);

            // make sure weapons and armors reference valid damage types before proceeding
            ValidateDamageTypeReferences();

            // special abilities
            PopulateCache<string, Ability, AbilityList>(Abilities,
                DataLocationConstants.AbilityDataFile,
                DataValidation.AbilityValidation.PrimaryKey,
                DataValidation.AbilityValidation.ListName);

            // ability scripts
            // LoadAbilityScripts();

            // units
            LoadUnitData();

            // make sure units have valid references
            ValidateUnitReferences();
			
			// structures
			LoadStructureData();

            // props
            LoadPropData();
			
			// factions
			PopulateCache<string, Faction, FactionContainer>(Factions,
				DataLocationConstants.FactionDataFile,
				DataValidation.FactionValidation.PrimaryKey,
				DataValidation.FactionValidation.ListName);
			
			// validate faction starting units point to defined unit objects
			ValidateStartingUnits();
        }

        /// <summary>
        /// Loads, deserializes, and caches a list of objects contained within a single XML file.
        /// </summary>
        /// <typeparam name="TKey">The list item's lookup key property Type</typeparam>
        /// <typeparam name="TValue">The list item's Type</typeparam>
        /// <typeparam name="TList">The containing list's Type</typeparam>
        /// <param name="cache">The cache to populate</param>
        /// <param name="relativePath">The location of the XML file relative to the fsdata or mod directory</param>
        /// <param name="keyName">The name of the property to use as a lookup key.  Its Type must match <typeparamref name="TKey"/></param>
        /// <param name="listName">The name of the property backing the list of objects.  Its type must match <typeparamref name="TList"/></param>
        private static void PopulateCache<TKey, TValue, TList>(ModdableXmlCache<TKey, TValue> cache, string relativePath, string keyName, string listName) where TValue: class
        {
            var path = DataLocationConstants.GameRootPath + relativePath;
            if (File.Exists(path))
            {
                cache.LoadFromSingleFile<TList>(path, keyName, listName);
            }
            else
            {
                throw new FracturedStateException("Cannot create cache.  Unable to locate file: " + relativePath);
            }
        }

        private static void LoadUnitData()
        {
            try
            {
				LoadXmlDirectory(Units, DataLocationConstants.GameRootPath + DataLocationConstants.UnitsDirectory, DataValidation.UnitValidation.PrimaryKey);
            }
            catch (Exception e)
            {
                throw new FracturedStateException("An error occurred when loading unit XML data.", e);
            }
        }
		
		private static void LoadStructureData()
		{
			try
			{
				LoadXmlDirectory(Structures, DataLocationConstants.GameRootPath + DataLocationConstants.StructureDirectory, DataValidation.StructureValidation.PrimaryKey);
			}
			catch (Exception e)
			{
				throw new FracturedStateException("An error occurred when loading structure XML data.", e);
			}
		}

        private static void LoadPropData()
        {
            try
            {
                LoadXmlDirectory(Props, DataLocationConstants.GameRootPath + DataLocationConstants.PropDirectory, DataValidation.PropValidation.PrimaryKey);
            }
            catch (Exception e)
            {
                throw new FracturedStateException("An error occured when loading prop XML data.", e);
            }
        }
		
		// does the actual directory search and populates a cache
		// split off into its own method because it gets called a few different places
		private static void LoadXmlDirectory<TKey, TValue>(ModdableXmlCache<TKey, TValue> cache, string directory, string key) where TValue : class
		{
			var files = Directory.GetFiles(directory, "*.xml", SearchOption.AllDirectories);
			cache.LoadFromDirectory(files, key);
		}

        // this method ensures that all unit data references only valid armors and weapons
        private static void ValidateUnitReferences()
        {
            var allUnits = Units.Values;
            var armorNames = Armors.Values.Select(a => a.Name).ToArray();
            var weapNames = Weapons.Values.Select(w => w.Name).ToArray();

            foreach (var unit in allUnits)
            {
	            if (unit.ArmorName != null && !armorNames.Contains(unit.ArmorName))
	            {
		            throw new FracturedStateException("Unit \"" + unit.Name + "\" references Armor \"" + unit.ArmorName + "\" that does not exist.");
	            }

	            if (unit.WeaponName != null && !weapNames.Contains(unit.WeaponName))
	            {
		            throw new FracturedStateException("Unit \"" + unit.Name + "\" references Weapon \"" + unit.WeaponName + "\" that does not exist.");
	            }
            }
        }

        // this method ensures that all weapon and armor data reference only valid damage types
        private static void ValidateDamageTypeReferences()
        {
            var dTypes = DamageTypes.Values.Select(d => d.Name).ToArray();

            var allWeapons = Weapons.Values;
            foreach (var weapon in allWeapons)
            {
	            if (!dTypes.Contains(weapon.DamageType))
		            throw new FracturedStateException("Weapon \"" + weapon.Name +
		                                              "\" references damage type \"" + weapon.DamageType + "\" that does not exist.");
            }

            var allArmors = Armors.Values;
            foreach (var armor in allArmors)
            {
	            if (armor.Defenses == null) continue;
	            foreach (var defense in armor.Defenses)
	            {
		            if (!dTypes.Contains(defense.DamageType))
			            throw new FracturedStateException("Armor \"" + armor.Name +
			                                              "\" references damage type \"" + defense.DamageType + "\" that does not exists");
	            }
            }
        }
		
		private static void ValidateStartingUnits()
		{
			var allFactions = Factions.Values;
			var unitNames = Units.Values.Select(u => u.Name).ToArray();
			
			foreach (var fac in allFactions)
			{
			    if (fac.StartingUnits == null) continue;
			    foreach (var sUnit in fac.StartingUnits)
			    {
			        if (!unitNames.Contains(sUnit.Name))
			            throw new FracturedStateException("Faction \"" + fac.Name + "\" references starting unit \"" +
			                                              sUnit.Name + "\" that does not exist.");
			    }
			}
		}
    }
}