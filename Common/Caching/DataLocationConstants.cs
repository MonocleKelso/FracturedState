using UnityEngine;

namespace FracturedState.Game
{
    public static class DataLocationConstants
    {
        public static string GameRootPath
        {
            get
            {
#if UNITY_EDITOR
                return Application.dataPath + "/../../fsdata/";
#else
                return Application.dataPath + "/fsdata/";
#endif
            }
        }

        // ui
        public const string TipDataFile = "tips.xml";

        // file names
        public const string DamageTypeDataFile = "damageTypes.xml";
        public const string WeaponDataFile = "weapon.xml";
        public const string ArmorDataFile = "armor.xml";
		public const string FactionDataFile = "faction.xml";
        public const string TerrainDataFile = "terrain.xml";
        public const string GameSettingsFile = "game.xml";
        public const string AbilityDataFile = "abilities.xml";
        public const string HouseColorDataFile = "houseColors.xml";

        // directories
        public const string UnitsDirectory = "units";
		public const string StructureDirectory = "structures";
        public const string PropDirectory = "props";
		public const string MapDirectory = "maps";
        public const string ScriptDirectory = "scripts";
        public const string ScreenshotDirectory = "screenshots";
        public const string PlayerProfileDirectory = "profiles";
		
        // built-in assets
        public const string BuiltInModelDirectory = "Models/";
        public const string BuiltInManifestName = "Manifest.txt";
        public const string BuiltInTextureDirectory = "Textures/";
        public const string BuiltInUITexDirectory = "UI/";
	    public const string UnitIconDirectory = BuiltInTextureDirectory + BuiltInUITexDirectory + "icons/units/";
        public const string BuiltInSFXDirectory = "Audio/Sound Effects/";
        public const string BuiltInBarkDirectory = "Audio/Voice/";
        public const string BuiltInParticleDirectory = "Effects/";
        public const string BuiltInRagdollDirectory = "Ragdolls/";
        public const string BuiltInGibDirectory = BuiltInModelDirectory + "Unit/Gibs/";
	    public const string BuiltInIndicatorDirectory = "GroundIndicators/";
	    public const string BuiltInDecalDirecotry = "Decals/";

		// prefab names
		public const string InertUIGameObject = "Prefabs/inertUI";
		public const string ClickableUIGameObject = "Prefabs/clickableUI";
        public const string TextUIGameObject = "Prefabs/textUI";
		public const string ModelContainerGameObject = "Prefabs/modelContainer";
        public const string UnitContainer = "Prefabs/unitContainer";
		public const string NetworkUnitContainer = "Prefabs/networkUnitContainer";
	    public const string NetworkDamageModule = "Prefabs/networkDmgModContainer";
        public const string StartingPoint = "Prefabs/mapStartingPoint";
        public const string NetworkManager = "Prefabs/netView";
        public const string NavMeshPointHelper = "Prefabs/navMeshPointHelper";
        public const string SelectionProjectorPrefab = "Prefabs/selectionProjector";
        public const string CoverHelperPrefab = "Prefabs/coverHelper";
        public const string FirepointHelperPrefab = "Prefabs/firepointHelper";
        public const string BattleIconPrefab = "Prefabs/battleIcon";
        public const string RecruitIconPrefab = "Prefabs/recruitIcon";
        public const string CoverHelperIconPrefab = "Prefabs/coverHelperIcon";
        public const string FirePointHelperIconPrefab = "Prefabs/firepointHelperIcon";
        public const string DamageHelperPrefab = "Prefabs/damageHelper";
        public const string HealHelperPrefab = "Prefabs/healHelper";
        public const string BuffHelperPrefab = "Prefabs/buffHelper";
    }
}