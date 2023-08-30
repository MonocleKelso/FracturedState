using FracturedState.Game.Data;
using FracturedState.Game.Management;

namespace FracturedState.Game.AI
{
    public class RecruitNewSquad : CompositeGoal<RecruitNewSquad>
    {
        public RecruitNewSquad(Team ownerTeam) : base(ownerTeam) { }
        private TerritoryData territory;

        public override void Activate()
        {
            base.Activate();
            RemoveAllSubGoals();
            territory = null;
            var territoryGoal = new FindOwnedRecruitableTerritory(OwnerTeam);
            territoryGoal.OnComplete = g => territory = g.Territory;
            AddSubGoal(territoryGoal);
        }

        public override GoalState Process()
        {
            base.Process();
            if (Status == GoalState.Completed)
            {
                if (territory != null)
                {
                    // OwnerTeam.RecruitManager.CreateRequest();
                    OwnerTeam.RecruitManager.GenerateMixedRequest();
                    OwnerTeam.RecruitManager.SetTerritory(territory.Name);
                    OwnerTeam.RecruitManager.QueueRequest();
                    if (OnComplete != null)
                    {
                        OnComplete(this);
                    }
                    return GoalState.Completed;
                }
                else
                {
                    return GoalState.Inactive;
                }
            }
            return GoalState.Failed;
        }
    }
}