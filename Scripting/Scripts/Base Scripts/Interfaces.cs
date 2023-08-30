
namespace FracturedState.Scripting
{
    /// <summary>
    /// A common interface used in all ability scripts that provides a single method for executing the script.
    /// The assumption throughout the framework is that every ability script will implement this method.
    /// </summary>
    public interface IFracAbility
    {
        void ExecuteAbility();
    }

    /// <summary>
    /// An ability interface meant to be used for abilities that monitor conditions in the world to
    /// determine their state of execution.
    /// </summary>
    public interface IMonitorAbility
    {
        void Update();
        void Finish();
    }
}