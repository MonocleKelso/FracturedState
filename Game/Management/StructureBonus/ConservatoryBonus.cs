using FracturedState.Game.Data;
using UnityEngine;

namespace FracturedState.Game.Management.StructureBonus
{
    public class ConservatoryBonus : IStructureBonus
    {
        public string StructureName => "Conservatory";
        public string BonusText => "+5% Accuracy";
        public string HelperText => "bonus.conservatory";
        
        private const int Percentage = 5;
        
        public void ApplyOnUnit(UnitManager unit)
        {
            // empty - doesn't modify unit
        }

        public void RemoveFromUnit(UnitManager unit)
        {
            // empty - doesn't modify unit
        }

        public void ApplyOnWeapon(Weapon weapon)
        {
            weapon.Accuracy += Percentage;
            if (weapon.Accuracy > 100)
            {
                weapon.Accuracy = 100;
            }
        }

        public void RemoveFromWeapon(Weapon weapon)
        {
            var origAccuracy = XmlCacheManager.Weapons[weapon.Name].Accuracy;
            weapon.Accuracy -= Percentage;
            if (weapon.Accuracy < origAccuracy)
            {
                weapon.Accuracy = origAccuracy;
            }
        }
    }
}