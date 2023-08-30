
namespace FracturedState.Game.AI
{
    /// <summary>
    /// An interface for states that must make requests to the pathfinding thread and are therefore wrapped in PathRequest instances
    /// for marshalling back and forth
    /// </summary>
    public interface IPathableState
    {
        /// <summary>
        /// Is this state currently active? This flag should be set to true in Enter() and false in Exit()
        /// This flag is checked in the pathfinding thread in order to skip requests that have become stale due to
        /// the unit being issued a new command
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Sets the path for this request as determined by the pathfinding algorithm. This method is used to marshal a result
        /// back to the unit's state so that it may act upon it on the subsequent frame
        /// </summary>
        void SetPath(System.Collections.Generic.List<UnityEngine.Vector3> path);
    }
}
