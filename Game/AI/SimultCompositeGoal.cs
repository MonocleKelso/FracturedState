using FracturedState.Game.Management;
using System.Collections.Generic;

namespace FracturedState.Game.AI
{
    public abstract class SimultCompositeGoal<T> : CompositeGoal<T> where T : IGoal
    {
        protected List<IGoal> simultGoals;

        public SimultCompositeGoal(Team ownerTeam) : base(ownerTeam) { }

        public override void Activate()
        {
            base.Activate();
            if (simultGoals != null)
            {
                for (int i = 0; i < simultGoals.Count; i++)
                {
                    simultGoals[i].Terminate();
                }
                simultGoals.Clear();
            }
            else
            {
                simultGoals = new List<IGoal>();
            }
        }

        public override void AddSubGoal(IGoal subGoal)
        {
            simultGoals.Add(subGoal);
        }

        public override GoalState Process()
        {
            if (Status == GoalState.Inactive)
            {
                Activate();
            }
            for (int i = simultGoals.Count - 1; i >= 0; i--)
            {
                if (simultGoals[i].Status == GoalState.Completed || simultGoals[i].Status == GoalState.Failed)
                {
                    simultGoals.RemoveAt(i);
                }
                else
                {
                    simultGoals[i].Process();
                }
            }
            if (simultGoals.Count > 0)
            {
                Status = GoalState.Active;
            }
            else
            {
                Status = GoalState.Completed;
            }
            return Status;
        }

        public override void Terminate()
        {
            base.Terminate();
            if (simultGoals != null)
            {
                foreach (var goal in simultGoals)
                {
                    goal.Terminate();
                }
                simultGoals.Clear();
            }
        }
    }
}