using FracturedState.Game.Data;
using FracturedState.Game.Management;
using System.Collections.Generic;

namespace FracturedState.Game.AI
{
    public class DefendTerritory : CompositeGoal<DefendTerritory>
    {
        private Squad squad;

        public DefendTerritory(Team ownerTeam, Squad squad) : base(ownerTeam)
        {
            this.squad = squad;
        }

        public override void Activate()
        {
            base.Activate();
            // get all our owned territories
            List<TerritoryData> territories = TerritoryManager.Instance.GetOwnedTerritories(OwnerTeam);
            // if we somehow don't own territory then don't add any sub goals
            if (territories != null && territories.Count > 0)
            {
                // pick a random territory we own
                var territory = territories[UnityEngine.Random.Range(0, territories.Count)];
                // get all the terrain tiles associated with this territory
                var terrains = TerritoryManager.Instance.GetTerrainsInTerritory(territory);
                // get a random terrain tile and add goal for squad to defend it
                AddSubGoal(new DefendTile(OwnerTeam, squad, terrains[UnityEngine.Random.Range(0, terrains.Count)]));
            }
        }

        public override GoalState Process()
        {
            if (Status == GoalState.Inactive)
                Activate();

            return Status;
        }
    }
}