using FracturedState.Game.Data;

namespace FracturedState.Game.Management.StructureBonus
{
    public class BankBonus : IStructureBonus
    {
        public string StructureName => "Bank";
        public string BonusText => "+5% Move Speed";
        public string HelperText => "bonus.bank";

        private const float Percentage = 0.05f;
        
        public void ApplyOnUnit(UnitManager unit)
        {
            if (unit.Data.Physics == null) return;
            
            var speed = XmlCacheManager.Units[unit.Data.Name].Physics?.MaxSpeed ?? 0;
            speed += speed * Percentage;
            unit.Data.Physics.MaxSpeed = speed;
            unit.Squad?.CalculateMoveSpeed();
        }

        public void RemoveFromUnit(UnitManager unit)
        {
            if (unit.Data.Physics == null) return;
            
            var speed = XmlCacheManager.Units[unit.Data.Name].Physics?.MaxSpeed ?? 0;
            speed -= speed * Percentage;
            unit.Data.Physics.MaxSpeed = speed;
            unit.Squad?.CalculateMoveSpeed();
        }

        public void ApplyOnWeapon(Weapon weapon)
        {
            // empty - doesn't modify weapon
        }

        public void RemoveFromWeapon(Weapon weapon)
        {
            // empty - doesn't modify weapon
        }
    }
}