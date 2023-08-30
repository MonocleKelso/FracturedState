using FracturedState.Game.Data;
using FracturedState.Game.Management;
using System.Collections.Generic;
using UnityEngine;

namespace FracturedState.Game.AI
{
    public class TakeClosestTerritory : SimultCompositeGoal<TakeClosestTerritory>
    {
        private Squad squad;

        public TakeClosestTerritory(Team ownerTeam, Squad squad)
            : base(ownerTeam)
        {
            this.squad = squad;
        }

        public override void Activate()
        {
            base.Activate();
            List<TerritoryData> ownedTerritories = TerritoryManager.Instance.GetOwnedTerritories(OwnerTeam);
            // determine the average positions of every unowned building in every unowned territory
            var distanceLookup = new Dictionary<TerritoryData, Vector3>();
            for (int i = 0; i < TerritoryManager.Instance.AllTerritories.Count; i++)
            {
                TerritoryData territory = TerritoryManager.Instance.AllTerritories[i];
                if (ownedTerritories != null && !ownedTerritories.Contains(territory))
                {
                    Vector3 avgPos = Vector3.zero;
                    int structCount = 0;
                    List<StructureManager> structures = TerritoryManager.Instance.GetStructuresInTerritory(territory);
                    if (structures != null)
                    {
                        for (int s = 0; s < structures.Count; s++)
                        {
                            StructureManager structure = structures[s];
                            if (structure.OwnerTeam != OwnerTeam)
                            {
                                avgPos += structure.transform.position;
                                structCount++;
                            }
                        }
                        if (structCount > 0)
                        {
                            distanceLookup[territory] = (avgPos / structCount);
                        }
                    }
                }
            }

            var keys = distanceLookup.Keys;
            // closest avg structure to this squad
            Vector3 squadPos = squad.GetAveragePosition();
            float dist = float.MaxValue;
            TerritoryData t = null;
            foreach (TerritoryData key in keys)
            {
                float d = (squadPos - distanceLookup[key]).sqrMagnitude;
                if (d < dist)
                {
                    dist = d;
                    t = key;
                }
            }
            if (t != null)
            {
                AddSubGoal(new CaptureStructure(OwnerTeam, t, squad));
            }
        }
    }
}