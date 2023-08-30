using System.Collections.Generic;
using System.Linq;
using FracturedState.Game.Data;
using UnityEngine;

namespace FracturedState.Game.Nav
{
    public enum State { Exterior, Interior }

    public class NavGrid
    {
        public const float InteriorSpaceStep = 0.5f;
        private const int ExteriorPointStep = 1;
        private const float InteriorPointStep = 0.125f;

        private int xLength, zLength;
        private NavGridPoint[,] points;

        private readonly Dictionary<UnitManager, List<NavGridPoint>> occupied = new Dictionary<UnitManager, List<NavGridPoint>>();
        private readonly Dictionary<int, List<NavGridPoint>> exteriorLookup;

        public NavGrid(int xUbound, int xLbound, int zUbound, int zLbound)
        {
            exteriorLookup = new Dictionary<int, List<NavGridPoint>>();
            BuildExterior(xUbound, xLbound, zUbound, zLbound);
        }

        public NavGrid(Collider[] boundingBoxes, float offset)
        {
            BuildInterior(boundingBoxes, offset);
        }

        public NavGrid(Collider[] boundingBoxes, float offset, float step)
        {
            BuildInterior(boundingBoxes, offset, step);
        }

        private void BuildExterior(int xUbound, int xLbound, int zUbound, int zLbound)
        {
            xLength = Mathf.RoundToInt((xUbound - xLbound) / (float)ExteriorPointStep) + 1;
            zLength = Mathf.RoundToInt((zUbound - zLbound) / (float)ExteriorPointStep) + 1;
            points = new NavGridPoint[xLength, zLength];
            var maxRadius = ConfigSettings.Instance.Values.NavPointMaxRadius;
            var xIndex = 0;
            for (var x = xLbound; x <= xUbound; x += ExteriorPointStep)
            {
                var zIndex = 0;
                for (var z = zLbound; z <= zUbound; z += ExteriorPointStep)
                {
                    var pos = new Vector3(x, 100, z);
                    var terrain = RaycastUtil.RaycastTerrain(pos);
                    if (terrain.transform != null)
                    {
                        var ext = RaycastUtil.RaycastExterior(pos);
                        if (ext.transform == null)
                        {
                            var p = new NavGridPoint(terrain.point, xIndex, zIndex, maxRadius);
                            var nearby = Physics.OverlapSphere(terrain.point, maxRadius, GameConstants.ExteriorNavMask);
                            double rad = maxRadius;
                            for (var n = 0; n < nearby.Length; n++)
                            {
                                var distance = (terrain.point - (nearby[n].ClosestPointOnBounds(terrain.point))).magnitude;
                                if (distance < rad)
                                {
                                    rad = distance;
                                }
                            }
                            p.Radius = rad;
                            points[xIndex, zIndex] = p;
                            var id = terrain.transform.gameObject.GetInstanceID();
                            List<NavGridPoint> pointCache;
                            if (!exteriorLookup.TryGetValue(id, out pointCache))
                            {
                                pointCache = new List<NavGridPoint>();
                                exteriorLookup[id] = pointCache;
                            }
                            pointCache.Add(p);
//                            var test = GameObject.CreatePrimitive(PrimitiveType.Sphere);
//                            test.layer = GameConstants.ExteriorLayer;
//                            test.transform.position = terrain.point;
                        }
                    }
                    zIndex++;
                }
                xIndex++;
            }
        }

        private void BuildInterior(Collider[] colliders, float offset, float step = InteriorPointStep)
        {
            var minX = float.MaxValue;
            var maxX = float.MinValue;
            var minZ = float.MaxValue;
            var maxZ = float.MinValue;

            foreach (var col in colliders)
            {
                var maxBounds = col.bounds.max;
                var minBounds = col.bounds.min;

                if (maxBounds.x > maxX)
                    maxX = maxBounds.x;

                if (maxBounds.z > maxZ)
                    maxZ = maxBounds.z;

                if (minBounds.x < minX)
                    minX = minBounds.x;

                if (minBounds.z < minZ)
                    minZ = minBounds.z;
            }

            xLength = Mathf.RoundToInt((maxX - minX) / step) + 1;
            zLength = Mathf.RoundToInt((maxZ - minZ) / step) + 1;
            points = new NavGridPoint[xLength, zLength];
            var maxRadius = ConfigSettings.Instance.Values.NavPointMaxRadius;

            var xIndex = 0;
            for (var x = minX; x <= maxX; x += step)
            {
                var zIndex = 0;
                for (var z = minZ; z <= maxZ; z += step)
                {
                    var point = new Vector3(x, 100, z);
                    var extHit = RaycastUtil.RaycastExterior(point);
                    if(extHit.transform != null)
                    {
                        var intHit = RaycastUtil.RaycastInterior(point);
                        if (intHit.transform == null)
                        {
                            points[xIndex, zIndex] = new NavGridPoint(new Vector3(x, offset, z), xIndex, zIndex, maxRadius);
                        }
                    }
                    zIndex++;
                }
                xIndex++;
            }
        }

        /// <summary>
        /// Returns the closest nav point to the given position in world space
        /// </summary>
        public NavGridPoint GetClosestPoint(Vector3 worldPosition, float minRadius, int? cacheId = null)
        {
            var closest = Closest(worldPosition, cacheId);
            if (closest?.Radius < minRadius)
            {
                var q = new Queue<NavGridPoint>();
                var s = new HashSet<NavGridPoint>();
                q.Enqueue(closest);
                s.Add(closest);
                while (q.Count > 0)
                {
                    var p = q.Dequeue();
                    if (p.Radius > minRadius)
                    {
                        return p;
                    }
                    var neighors = GetNeighbors(p);
                    for (var i = 0; i < neighors.Count; i++)
                    {
                        if (!s.Contains(neighors[i]))
                        {
                            q.Enqueue(neighors[i]);
                            s.Add(neighors[i]);
                        }
                    }
                }
            }
            return closest;
        }

        /// <summary>
        /// Returns the nav point closest to the given position that is also not farther away from destination than
        /// position is
        /// </summary>
        public NavGridPoint GetClosestFreePoint(Vector3 worldPosition, float minRadius, Vector3 destination)
        {
            var compareDist = (worldPosition - destination).sqrMagnitude;
            var closest = Closest(worldPosition);
            if (closest == null) return null;
            if (closest.Radius >= minRadius) return closest;
            var q = new Queue<NavGridPoint>();
            var s = new HashSet<NavGridPoint>();
            q.Enqueue(closest);
            s.Add(closest);
            while (q.Count > 0)
            {
                var p = q.Dequeue();
                if (p.Radius >= minRadius && !p.IsOccupied && (p.WorldPosition - worldPosition).sqrMagnitude <= compareDist) return p;
                
                var neighbors = GetNeighbors(p);
                foreach (var n in neighbors)
                {
                    if (s.Add(n))
                    {
                        q.Enqueue(n);
                    }
                }
            }
            return closest;
        }

        private NavGridPoint Closest(Vector3 worldPosition, int? cacheId = null)
        {
            NavGridPoint closest = null;
            var dist = float.MaxValue;
            if (cacheId.HasValue && exteriorLookup != null)
            {
                List<NavGridPoint> cachePoints;
                if (exteriorLookup.TryGetValue(cacheId.Value, out cachePoints))
                {
                    for (var i = 0; i < cachePoints.Count; i++)
                    {
                        var p = cachePoints[i];
                        if (p == null) continue;

                        var toPoint = (worldPosition - p.WorldPosition).sqrMagnitude;
                        if (toPoint < dist)
                        {
                            dist = toPoint;
                            closest = p;
                        }
                    }

                    return closest;
                }
            }
            
            for (var x = 0; x < xLength; x++)
            {
                for (var z = 0; z < zLength; z++)
                {
                    var p = points[x, z];
                    if (p == null) continue;
                    
                    var toPoint = (worldPosition - p.WorldPosition).sqrMagnitude;
                    if (toPoint < dist)
                    {
                        dist = toPoint;
                        closest = p;
                    }
                }
            }
            return closest;
        }

        public void OccupyPoint(UnitManager unit, NavGridPoint point)
        {
            var oPoints = new List<NavGridPoint> {point};
            var r = unit.Data?.Physics?.PathRadius ?? 0;
            if (r > 0)
            {
                var done = new HashSet<NavGridPoint>();
                var neighbors = GetNeighbors(point);
                while (neighbors.Count > 0)
                {
                    var n = neighbors[neighbors.Count - 1];
                    neighbors.RemoveAt(neighbors.Count - 1);
                    done.Add(n);
                    if ((n.WorldPosition - point.WorldPosition).magnitude < r)
                    {
                        oPoints.Add(n);
                        neighbors.AddRange(GetNeighbors(n).Where(p => !done.Contains(p)).ToList());
                    }
                }
            }

            foreach (var p in oPoints)
            {
                p.IsOccupied = true;
            }

            occupied[unit] = oPoints;
        }

        public void OpenPoint(UnitManager unit)
        {
            List<NavGridPoint> oPoints;
            if (occupied.TryGetValue(unit, out oPoints))
            {
                foreach (var p in oPoints)
                {
                    p.IsOccupied = false;
                }
                oPoints.Clear();
                occupied.Remove(unit);
            }
        }

        /// <summary>
        /// Searches neighboring points for the first unoccupied point
        /// </summary>
        public NavGridPoint GetClosestFreePoint(NavGridPoint point)
        {
            var q = new Queue<NavGridPoint>();
            var s = new HashSet<NavGridPoint>();
            q.Enqueue(point);
            s.Add(point);

            while (q.Count > 0)
            {
                var p = q.Dequeue();
                if (!p.IsOccupied)
                {
                    return p;
                }
                var neighors = GetNeighbors(p);
                for (var i = 0; i < neighors.Count; i++)
                {
                    if (!s.Contains(neighors[i]))
                    {
                        q.Enqueue(neighors[i]);
                        s.Add(neighors[i]);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Returns every direct neighbor to the given nav point
        /// </summary>
        public List<NavGridPoint> GetNeighbors(NavGridPoint point)
        {
            var x = point.Xindex;
            var z = point.Zindex;
            var neighbors = new List<NavGridPoint>();

            var worldY = point.WorldPosition.y;

            if (z > 0 && points[x, z - 1] != null && Mathf.Abs(points[x, z - 1].WorldPosition.y - worldY) <= ConfigSettings.Instance.Values.NavOffsetThreshold)
            {
                neighbors.Add(points[x, z - 1]);
            }

            if (z < zLength - 1 && points[x, z + 1] != null && Mathf.Abs(points[x, z + 1].WorldPosition.y - worldY) <= ConfigSettings.Instance.Values.NavOffsetThreshold)
            {
                neighbors.Add(points[x, z + 1]);
            }

            if (x > 0)
            {
                if (points[x - 1, z] != null && Mathf.Abs(points[x - 1, z].WorldPosition.y - worldY) <= ConfigSettings.Instance.Values.NavOffsetThreshold)
                {
                    neighbors.Add(points[x - 1, z]);
                }

                if (z > 0 && points[x - 1, z - 1] != null && Mathf.Abs(points[x - 1, z - 1].WorldPosition.y - worldY) <= ConfigSettings.Instance.Values.NavOffsetThreshold)
                {
                    neighbors.Add(points[x - 1, z - 1]);
                }

                if (z < zLength - 1 && points[x - 1, z + 1] != null && Mathf.Abs(points[x - 1, z + 1].WorldPosition.y - worldY) <= ConfigSettings.Instance.Values.NavOffsetThreshold)
                {
                    neighbors.Add(points[x - 1, z + 1]);
                }
            }

            if (x < xLength - 1)
            {
                if (points[x + 1, z] != null && Mathf.Abs(points[x + 1, z].WorldPosition.y - worldY) <= ConfigSettings.Instance.Values.NavOffsetThreshold)
                {
                    neighbors.Add(points[x + 1, z]);
                }

                if (z > 0 && points[x + 1, z - 1] != null && Mathf.Abs(points[x + 1, z - 1].WorldPosition.y - worldY) <= ConfigSettings.Instance.Values.NavOffsetThreshold)
                {
                    neighbors.Add(points[x + 1, z - 1]);
                }

                if (z < zLength - 1 && points[x + 1, z + 1] != null && Mathf.Abs(points[x + 1, z + 1].WorldPosition.y - worldY) <= ConfigSettings.Instance.Values.NavOffsetThreshold)
                {
                    neighbors.Add(points[x + 1, z + 1]);
                }
            }

            return neighbors;
        }
    }
}