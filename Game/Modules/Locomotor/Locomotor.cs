using System.Collections.Generic;
using UnityEngine;

namespace FracturedState.Game.Modules
{
    /// <summary>
    /// The base class for any controller implementation that determines how a unit moves around in the world
    /// </summary>
    public abstract class Locomotor
    {
        #region Properties
        
        /// <summary>
        /// The unit this Locomotor instance belongs to
        /// </summary>
        public UnitManager Owner { get; private set; }

        /// <summary>
        /// The current velocity of the owner
        /// </summary>
        public Vector3 CurrentVelocity { get; protected set; }
        
        #endregion
        
        public Locomotor(UnitManager owner)
        {
            Owner = owner;
        }

        #region Public_Methods
        
        /// <summary>
        /// Sets the unit's current velocity to a zeroed vector.
        /// Note: this doesn't actually stop the unit, it will just behave as if it were stopped
        /// when it needs to calculate movement the next time.
        /// </summary>
        public virtual void ZeroVelocity()
        {
            CurrentVelocity = Vector3.zero;
        }
        
        #endregion

        #region Abstract_Methods
        
        public abstract int MoveOnPath(List<Vector3> path, int currentIndex);

        #endregion
    }
}