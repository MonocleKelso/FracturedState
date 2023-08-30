using FracturedState.Game.Data;
using FracturedState.Game.Management;
using System.Linq;

namespace FracturedState.Game.AI
{
    public class FindOwnedRecruitableTerritory : AtomicGoal<FindOwnedRecruitableTerritory>
    {
        public TerritoryData Territory { get; private set; }

        public FindOwnedRecruitableTerritory(Team ownerTeam) : base(ownerTeam) { }

        public override void Activate()
        {
            base.Activate();
            // get all territories owned by this team and filter by which ones are recruitable
            // if no recuitable territories are found then this goal fails
            var territories = TerritoryManager.Instance.GetOwnedTerritories(OwnerTeam)?.Where(t => t.Recruit).ToArray();
            if (territories == null || territories.Length <= 0) return;
            
            var recruitCount = int.MaxValue;
            // get territory with the least number of pending requests
            for (var i = 0; i < territories.Length; i++)
            {
                var reqs = OwnerTeam.RecruitManager.GetRequestsByTerritory(territories[i].Name);
                if (reqs.Length < recruitCount)
                {
                    recruitCount = reqs.Length;
                    Territory = territories[i];
                }
            }
        }

        public override GoalState Process()
        {
            Status = base.Process();
            if (Territory != null)
            {
                Status = GoalState.Completed;
                OnComplete?.Invoke(this);
            }
            else
            {
                Status = GoalState.Failed;
            }
            return Status;
        }
    }
}