using System.Collections.Generic;

namespace FracturedState.Game.Network
{
    public class CombineSquadMessage : ILockStepMessage
    {
        public uint Id { get; set; }
        private readonly UnitManager[] units;

        public CombineSquadMessage(UnitManager[] units)
        {
            this.units = units;
        }
        
        public void Process()
        {
            var unitList = new List<UnitManager>();
            var count = 0;
            
            foreach (var unit in units)
            {
                if (unit == null || !unit.IsAlive) continue;
                
                unitList.Add(unit);
                if (unit.OwnerTeam.Squads.Contains(unit.Squad))
                {
                    count++;
                    unit.OwnerTeam.Squads.Remove(unit.Squad);
                    if (unit.IsMine)
                    {
                        CompassUI.Instance.RemoveSquad();
                    }
                }
            }

            if (unitList.Count == 0) return;
            
            unitList[0].OwnerTeam.Squads.Add(new Squad(unitList));
            if (unitList[0].IsMine)
            {
                CompassUI.Instance.AddSquad();
                SelectionManager.Instance.RecalculateSquadSelection();
                SelectionManager.Instance.OnSelectionChanged.Invoke();
                MultiplayerEventBroadcaster.Text($"Consolidated {count} squads");
            }
        }
    }
}