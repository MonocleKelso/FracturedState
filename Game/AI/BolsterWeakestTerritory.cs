using FracturedState.Game.Data;
using FracturedState.Game.Management;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FracturedState.Game.AI
{
    public class BolsterWeakestTerritory : SimultCompositeGoal<BolsterWeakestTerritory>
    {
        private Squad squad;
        private TerritoryData territory;

        public BolsterWeakestTerritory(Team ownerTeam, Squad squad)
            : base(ownerTeam)
        {
            this.squad = squad;
        }

        public override void Activate()
        {
            base.Activate();
            territory = null;
            List<StructureManager> unownedStructures = null;
            List<TerritoryData> territories = TerritoryManager.Instance.GetOwnedTerritories(OwnerTeam);
            // if we own no territories then grab the closest territory to the squad
            if (territories == null || territories.Count == 0)
            {
                // average position of squad to compare territory distance
                Vector3 squadPos = squad.GetAveragePosition();
                territories = TerritoryManager.Instance.AllTerritories;
                float dist = float.MaxValue;
                for (int i = 0; i < territories.Count; i++)
                {
                    var tiles = TerritoryManager.Instance.GetTerrainsInTerritory(territories[i]);
                    Vector3 tilePos = Vector3.zero;
                    // calculate average position of tiles in each territory to get a rough guess as to which one is closest
                    for (int t = 0; t < tiles.Count; t++)
                    {
                        tilePos += tiles[t].transform.position;
                    }
                    tilePos /= tiles.Count;
                    if ((tilePos - squadPos).sqrMagnitude < dist)
                    {
                        dist = (tilePos - squadPos).sqrMagnitude;
                        territory = territories[i];
                    }
                }
            }
            else if (territories != null)
            {
                // find the territory we own that has the least amount of structures in it that we own
                // ie - the territory we're most in danger of losing
                double currentPercent = 0;
                for (int i = 0; i < territories.Count; i++)
                {
                    List<StructureManager> structures = TerritoryManager.Instance.GetStructuresInTerritory(territories[i]).Where(s => s.StructureData.CanBeCaptured).ToList();
                    unownedStructures = structures.Where(s => s.OwnerTeam != OwnerTeam).ToList();
                    double percent = unownedStructures.Count / (double)structures.Count;
                    if (percent > currentPercent)
                    {
                        currentPercent = percent;
                        territory = territories[i];
                    }
                }
            }

            if (territory != null)
            {
                AddSubGoal(new CaptureStructure(OwnerTeam, territory, squad));
            }
        }
    }
}