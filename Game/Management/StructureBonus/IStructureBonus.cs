using FracturedState.Game.Data;

namespace FracturedState.Game.Management.StructureBonus
{
    public interface IStructureBonus
    {
        string StructureName { get; }
        string BonusText { get; }
        string HelperText { get; }
        
        void ApplyOnUnit(UnitManager unit);
        void RemoveFromUnit(UnitManager unit);
        void ApplyOnWeapon(Weapon weapon);
        void RemoveFromWeapon(Weapon weapon);
    }
}