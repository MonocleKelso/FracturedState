using System;
using System.Collections.Generic;
using UnityEngine;

namespace FracturedState.Game.Nav
{
    public class PathNode : IComparable<PathNode>
    {
        public double GCost = double.MaxValue;
        public double FCost;
        public NavMeshPoint Point;
        public NavMeshPoint Parent;

        public int CompareTo(PathNode other)
        {
            if (FCost < other.FCost)
                return -1;
            if (FCost > other.FCost)
                return 1;
            return 0;
        }
    }

    public class PathPlanner
    {
        private static PathPlanner instance;
        public static PathPlanner Instance
        {
            get
            {
                if (instance == null)
                    instance = new PathPlanner();

                return instance;
            }
        }

        // private NavMesh navMesh;
        private NavMesh exteriorNavMesh;
        private NavMesh interiorNavMesh;
        private Camera gameCamera;
        private RaycastHit hit;

        private PathPlanner()
        {
            gameCamera = Camera.main;
        }

        public void SetNavMesh(NavMesh exteriorNavMesh, NavMesh interiorNavMesh)
        {
            // this.navMesh = navMesh;
            // this.navMesh.InitTriangleIndex();
            this.exteriorNavMesh = exteriorNavMesh;
            this.interiorNavMesh = interiorNavMesh;
            this.exteriorNavMesh.InitTriangleIndex();
            this.interiorNavMesh.InitTriangleIndex();
        }

        /// <summary>
        /// Calculates a path from the given origin to the given destination.
        /// Returns null if no path can be calculated or if the given destination is not contained on the navigation mesh
        /// </summary>
        public List<Vector3> DoPath(Vector3 origin, Vector3 destination, NavMeshRole role, float unitRadius)
        {
            int layerMask = (role == NavMeshRole.Exterior) ? GameConstants.ExteriorMoveMask : GameConstants.InteriorMoveMask;
            if (NavMeshRayCast(destination, layerMask))
            {
                return Calc(origin, role, unitRadius);
            }
            return null;
        }

        private List<Vector3> Calc(Vector3 origin, NavMeshRole role, float unitRadius)
        {
            NavMesh navMesh;
            int layerMask;
            if (role == NavMeshRole.Exterior)
            {
                navMesh = exteriorNavMesh;
                layerMask = GameConstants.ExteriorMoveMask;
            }
            else
            {
                navMesh = interiorNavMesh;
                layerMask = GameConstants.InteriorMoveMask;
            }
            // triangle containing start point
            NavMeshTriangle startTriangle;
            Vector3 posUp = origin + Vector3.up;
            RaycastHit origHit;
            if (Physics.Raycast(posUp, -Vector3.up, out origHit, Mathf.Infinity, layerMask))
            {
                startTriangle = navMesh.GetTriangleByIndex(origHit.triangleIndex);
            }
            else
            {
                startTriangle = navMesh.GetTriangleByWorldPosition(origin);
            }

            // if the destination is in the same triangle as the start then just return a path containing the end node
            if (hit.triangleIndex == origHit.triangleIndex)
            {
                List<Vector3> path = new List<Vector3>();
                path.Add(hit.point);
                return path;
            }

            // triangle containing end point
            NavMeshTriangle endTriangle = navMesh.GetTriangleByIndex(hit.triangleIndex);
            // end point in world space
            Vector3 endPoint = hit.point;

            // nodes already considered
            Dictionary<NavMeshPoint, PathNode> closedSet = new Dictionary<NavMeshPoint, PathNode>();
            // all processed nodes - used to build path
            Dictionary<NavMeshPoint, PathNode> map = new Dictionary<NavMeshPoint, PathNode>();
            // nodes yet to be considered - sorted by cost
            PriorityQueue<PathNode> openSet = new PriorityQueue<PathNode>();
            // open node lookup
            Dictionary<NavMeshPoint, PathNode> openLookup = new Dictionary<NavMeshPoint, PathNode>();

            // create a PathNode for the point on the starting triangle closest to the destination
            PathNode startNode = new PathNode();
            NavMeshPoint startPoint = startTriangle.GetClosestTrianglePoint(endPoint);
            startNode.Point = startPoint;
            startNode.FCost = (startPoint.WorldPosition - endPoint).sqrMagnitude;
            startNode.GCost = 0;
            // add to map and open set
            map[startPoint] = startNode;
            openLookup[startPoint] = startNode;
            openSet.Enqueue(startNode);

            // process while we have open nodes or we get to destination
            while (!openSet.IsEmpty())
            {
                // get next closest node
                PathNode nextClosestNode = openSet.Dequeue();
                openLookup.Remove(nextClosestNode.Point);
                // add to closed set
                closedSet[nextClosestNode.Point] = nextClosestNode;

                // if this point is part of the end point triangle then we're done
                if (endTriangle.FirstEdge.ContainsPoint(nextClosestNode.Point) ||
                    endTriangle.SecondEdge.ContainsPoint(nextClosestNode.Point) ||
                    endTriangle.ThirdEdge.ContainsPoint(nextClosestNode.Point))
                {
                    List<Vector3> path = new List<Vector3>();
                    // add current node
                    path.Add(nextClosestNode.Point.WorldPosition);
                    // path.Add(nextClosestNode.Point.GetAdjustedWorldPosition(unitRadius));
                    // add destination
                    path.Add(endPoint);
                    // recurse backwards through nodes to build path
                    PathNode n = nextClosestNode;
                    while (true)
                    {
                        path.Insert(0, n.Point.WorldPosition);
                        // path.Insert(0, n.Point.GetAdjustedWorldPosition(unitRadius));
                        if (n.Parent == null)
                            break;

                        n = map[n.Parent];
                    }
                    return SmoothPath(origin, path, role);
                }

                for (var i = 0; i < nextClosestNode.Point.Edges.Count; i++)
                {
                    NavMeshPoint newPoint = nextClosestNode.Point.Edges[i].GetOtherPoint(nextClosestNode.Point);

                    // skip if point is in closed set
                    if (closedSet.ContainsKey(newPoint))
                        continue;

                    double gCost = nextClosestNode.GCost + (nextClosestNode.Point.WorldPosition - newPoint.WorldPosition).sqrMagnitude;
                    PathNode node;
                    // if node not in open set add
                    // otherwise update open set with new priority if the gCost is less
                    if (!openLookup.TryGetValue(newPoint, out node))
                    {
                        node = new PathNode();
                        node.Point = newPoint;
                        map[newPoint] = node;
                        openLookup[newPoint] = node;
                        node.GCost = gCost;
                        node.Parent = nextClosestNode.Point;
                        node.FCost = node.GCost + (newPoint.WorldPosition - endPoint).sqrMagnitude;
                        openSet.Enqueue(node);
                    }
                    else if (node.GCost > gCost)
                    {
                        node.GCost = gCost;
                        node.Parent = nextClosestNode.Point;
                        node.FCost = node.GCost + (newPoint.WorldPosition - endPoint).sqrMagnitude;
                        openSet.ChangePriority(node);
                    }
                }
            }
            return null;
        }

        public List<Vector3> SmoothPath(Vector3 origin, List<Vector3> fullPath, NavMeshRole role)
        {
            List<Vector3> path = new List<Vector3>();
            Vector3 endPoint = fullPath[fullPath.Count - 1];
            int layerMask = (role == NavMeshRole.Exterior) ? GameConstants.ExteriorMask : GameConstants.InteriorMask;

            // if we can travel straight from start to finish then exit early
            if (!Physics.CheckCapsule(origin, endPoint, 1f, layerMask))
            {
                path.Add(endPoint);
                return path;
            }

            // if we're not traveling straight at our target then we need to adjust our path
            // to account for a unit's radius
            for (var p = 0; p < fullPath.Count; p++)
            {
                Collider[] cols = Physics.OverlapSphere(fullPath[p], 1f, layerMask);
                Vector3 dir = Vector3.zero;
                for (var c = 0; c < cols.Length; c++)
                {
                    dir += (cols[c].ClosestPointOnBounds(fullPath[p]) - fullPath[p]);
                }
                dir /= cols.Length;
                fullPath[p] = fullPath[p] + dir.normalized;
            }

            int i = 0;
            while (true)
            {
                // if we are at the end point
                if (i == fullPath.Count - 1)
                {
                    path.Add(fullPath[i]);
                    return path;
                }
                
                // if this point can travel to the end point then exit early
                if (!Physics.CheckCapsule(fullPath[i], endPoint, 1f, layerMask))
                {
                    path.Add(fullPath[i]);
                    path.Add(endPoint);
                    return path;
                }

                // otherwise check next 2 points
                if (i + 2 < fullPath.Count && Physics.CheckCapsule(fullPath[i], fullPath[i + 2], 1f, layerMask))
                {
                    path.Add(fullPath[i]);
                }

                i++;
            }
        }

        public bool NavMeshRayCast(NavMeshRole role)
        {
            Ray ray = gameCamera.ScreenPointToRay(Input.mousePosition);
            int layerMask = (role == NavMeshRole.Exterior) ? GameConstants.ExteriorMoveMask : GameConstants.InteriorMoveMask;
            return Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask);
        }

        public bool NavMeshRayCast(Vector3 position, int layerMask)
        {
            Vector3 rayStart = position + Vector3.up;
            Ray ray = new Ray(rayStart, -(Vector3.up));
            return Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask);
        }

        /// <summary>
        /// Returns true along with a world position if the area under the mouse cursor is contained within the map's navigation mesh,
        /// otherwise returns false and a zero'd vector
        /// </summary>
        public bool GetNavMeshPositionAtMouse(out Vector3 position, NavMeshRole role)
        {
            bool canNav = NavMeshRayCast(role);
            position = canNav ? hit.point : Vector3.zero;
            return canNav;
        }
    }
}