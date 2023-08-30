using FracturedState.Game.AI;
using UnityEngine;
using System.Collections.Generic;
using FracturedState;
using FracturedState.Game.Nav;

namespace Monocle.Threading
{
    /// <summary>
    /// An object that manages state for a request to the multi-threaded path finding system
    /// </summary>
    public class PathRequest
    {
        public Vector3 Start { get; }
        public Vector3 End { get; }
        public float UnitRadius { get; }
        public NavGrid InteriorGrid { get; }
        public int? StartCacheId { get; }
        public int? EndCacheId { get; }

        public System.Exception Error { private get; set; }

        private readonly IPathableState state;

        public bool IsStateActive => state.IsActive;

        public PathRequest(IPathableState state, Vector3 start, Vector3 end, float unitRadius)
        {
            Start = start;
            End = end;
            UnitRadius = unitRadius;
            this.state = state;
            var startHit = RaycastUtil.RaycastTerrain(start + Vector3.up);
            if (startHit.transform != null)
            {
                StartCacheId = startHit.transform.gameObject.GetInstanceID();
            }

            var endHit = RaycastUtil.RaycastTerrain(end + Vector3.up);
            if (endHit.transform != null)
            {
                EndCacheId = endHit.transform.gameObject.GetInstanceID();
            }
        }

        public PathRequest (IPathableState state, NavGrid interiorGrid, Vector3 start, Vector3 end, float unitRadius)
        {
            InteriorGrid = interiorGrid;
            Start = start;
            End = end;
            UnitRadius = unitRadius;
            this.state = state;
        }

        public void CompleteRequest(List<Vector3> path)
        {
            if (Error != null)
            {
                ThreadedExceptionMonitor.Instance.ThrowException(Error);
            }
            state.SetPath(path);
        }
    }
}
