using FracturedState.Game.Management;

namespace FracturedState.Game.AI
{
    public class MaximizePopulation : CompositeGoal<MaximizePopulation>
    {
        public MaximizePopulation(Team ownerTeam) : base(ownerTeam) { }

        public override void Activate()
        {
            base.Activate();
            AddSubGoal(new RecruitNewSquad(OwnerTeam));
        }

        public override GoalState Process()
        {
            Status = base.Process();
            if (Status == GoalState.Completed)
            {
                if (OwnerTeam.SquadPopulation + OwnerTeam.RecruitManager.PendingRequests.Count < OwnerTeam.TeamPopulation)
                {
                    Activate();
                }
                if (OnComplete != null)
                {
                    OnComplete(this);
                }
            }

            return Status;
        }
    }
}
