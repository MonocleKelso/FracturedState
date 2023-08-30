using FracturedState.Game.Management;
using FracturedState.UI;

namespace FracturedState.Game
{
    public class GarrisonSquad : Squad
    {
        private readonly StructureManager owner;

        public GarrisonSquad(StructureManager owner)
        {
            this.owner = owner;
        }

        public override void RemoveSquadUnit(UnitManager unit)
        {
            Members.Remove(unit);
            CoverQueue.RemoveReorder(unit);
            var los = unit.GetComponentInChildren<UnitManager>();
            UnitSightTable.Remove(los);
            if (Members.Count == 0)
            {
                if (unit.IsMine)
                {
                    VisibilityChecker.Instance.UnregisterSquad(this);
                }
            }
            owner.RespawnGarrisonUnit(unit);
        }
    }
}