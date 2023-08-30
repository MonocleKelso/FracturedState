
namespace FracturedState.Game.Data
{
    public static class DataValidation
    {
        public static ValidationObject DamageValidation = new ValidationObject("Name", "DamageTypes");
        public static ValidationObject WeaponValidation = new ValidationObject("Name", "Weapons");
        public static ValidationObject ArmorValidation = new ValidationObject("Name", "Armors");
        public static ValidationObject UnitValidation = new ValidationObject("Name", "");
		public static ValidationObject StructureValidation = new ValidationObject("Name", "");
		public static ValidationObject FactionValidation = new ValidationObject("Name", "Factions");
        public static ValidationObject PropValidation = new ValidationObject("Name", "");
        public static ValidationObject AbilityValidation = new ValidationObject("Name", "Abilities");
        public static ValidationObject HouseColorValidation = new ValidationObject("Name", "HouseColors");


        public class ValidationObject
        {
            public string PrimaryKey;
            public string ListName;

            public ValidationObject(string primaryKey, string listName)
            {
                PrimaryKey = primaryKey;
                ListName = listName;
            }
        }
    }
}