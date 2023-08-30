using FracturedState.Game.Management;
using System;
using System.Collections.Generic;

namespace FracturedState.Game.AI
{
    public abstract class CompositeGoal<T> : IGoal where T : IGoal
    {
        public GoalState Status { get; protected set; }
        public Team OwnerTeam { get; private set; }
        public Action<T> OnComplete;

        protected Stack<IGoal> subgoals;

        public CompositeGoal(Team ownerTeam)
        {
            OwnerTeam = ownerTeam;
            subgoals = new Stack<IGoal>();
            Status = GoalState.Inactive;
        }

        public virtual void Activate()
        {
            Status = GoalState.Active;
        }

        public virtual GoalState Process()
        {
            if (Status == GoalState.Inactive)
                Activate();

            Status = ProcessSubGoals();

            return Status;
        }

        private GoalState ProcessSubGoals()
        {
            while (subgoals.Count > 0 && (subgoals.Peek().Status == GoalState.Completed || subgoals.Peek().Status == GoalState.Failed))
            {
                IGoal g = subgoals.Pop();
                g.Terminate();
            }

            if (subgoals.Count > 0)
            {
                GoalState status = subgoals.Peek().Process();
                if (status == GoalState.Completed && subgoals.Count > 1)
                {
                    return GoalState.Active;
                }
                return status;
            }

            return GoalState.Completed;
        }

        public void RemoveAllSubGoals()
        {
            subgoals.Clear();
        }

        public virtual void Terminate()
        {
            while (subgoals.Count > 0)
            {
                IGoal g = subgoals.Pop();
                g.Terminate();
            }
        }

        public virtual void AddSubGoal(IGoal subGoal)
        {
            subgoals.Push(subGoal);
        }
    }
}