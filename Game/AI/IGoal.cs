using FracturedState.Game.Management;

namespace FracturedState.Game.AI
{
    public enum GoalState { Inactive, Active, Completed, Failed }

    public interface IGoal
    {
        GoalState Status { get; }
        Team OwnerTeam { get; }

        void Activate();
        GoalState Process();
        void Terminate();
        void AddSubGoal(IGoal subGoal);
    }
}