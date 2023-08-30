using FracturedState.Game.Management;
using System;

namespace FracturedState.Game.AI
{
    public abstract class AtomicGoal<T> : IGoal where T : IGoal
    {
        public GoalState Status { get; protected set; }
        public Team OwnerTeam { get; private set; }
        public Action<T> OnComplete;

        public AtomicGoal(Team ownerTeam)
        {
            OwnerTeam = ownerTeam;
            Status = GoalState.Inactive;
        }

        public virtual void Activate()
        {
            Status = GoalState.Active;
        }

        public virtual GoalState Process()
        {
            if (Status == GoalState.Inactive)
            {
                Activate();
            }
            return Status;
        }

        public virtual void Terminate() { }

        public virtual void AddSubGoal(IGoal subGoal)
        {
            throw new NotSupportedException("Atomic goals cannot contain subgoals");
        }
    }
}