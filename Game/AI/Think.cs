using FracturedState.Game.Management;
using System.Collections.Generic;
using System.Linq;

namespace FracturedState.Game.AI
{
    public class Think : SimultCompositeGoal<Think>
    {
        private Dictionary<Squad, IGoal> goalTable;

        public Think(Team ownerTeam) : base(ownerTeam)
        {
            goalTable = new Dictionary<Squad, IGoal>();
        }

        public override void Activate()
        {
            base.Activate();
            Arbitrate();
        }

        public override GoalState Process()
        {
            Status = base.Process();
            if (Status == GoalState.Completed || Status == GoalState.Failed)
            {
                Status = GoalState.Inactive;
            }
            else
            {
                Arbitrate();
            }
            return Status;
        }

        private void Arbitrate()
        {
            if (OwnerTeam.SquadPopulation + OwnerTeam.RecruitManager.PendingRequests.Count < OwnerTeam.TeamPopulation)
            {
                AddSubGoal(new RecruitNewSquad(OwnerTeam));
            }

            for (int i = 0; i < OwnerTeam.Squads.Count; i++)
            {
                Squad squad = OwnerTeam.Squads[i];
                IGoal squadGoal;
                if (goalTable.TryGetValue(squad, out squadGoal))
                {
                    if (squadGoal.Status == GoalState.Completed || squadGoal.Status == GoalState.Failed)
                    {
                        goalTable.Remove(squad);
                        FindJobForSquad(squad);
                    }
                }
                else
                {
                    FindJobForSquad(squad);
                }
            }
        }

        public void AddSubGoal(IGoal subGoal, Squad squad)
        {
            AddSubGoal(subGoal);
            goalTable[squad] = subGoal;
        }

        private void FindJobForSquad(Squad squad)
        {
            var territories = TerritoryManager.Instance.GetOwnedTerritories(OwnerTeam);
            if (territories == null || territories.Count == 0)
            {
                AddSubGoal(new BolsterWeakestTerritory(OwnerTeam, squad), squad);
            }
            else
            {
                // take all structures in the territories we own first before moving onto other territories
                bool foundGoal = false;
                for (int i = 0; i < territories.Count; i++)
                {
                    var territory = territories[i];
                    var structures = TerritoryManager.Instance.GetStructuresInTerritory(territory).Where(s => s.StructureData.CanBeCaptured);
                    if (structures.Count(s => s.OwnerTeam == OwnerTeam) != structures.Count())
                    {
                        AddSubGoal(new BolsterWeakestTerritory(OwnerTeam, squad), squad);
                        foundGoal = true;
                        break;
                    }
                }

                if (!foundGoal)
                {
                    AddSubGoal(new TakeClosestTerritory(OwnerTeam, squad), squad);
                }

                // coin flip to determine if we defend or attack
                //if (UnityEngine.Random.Range(0, 100) % 2 == 0)
                //{
                //    AddSubGoal(new DefendTerritory(OwnerTeam, squad));
                //}
                //else
                //{
                    
                //}
            }
        }
    }
}