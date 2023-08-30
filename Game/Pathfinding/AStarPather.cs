using System;
using System.Collections.Generic;
using System.Diagnostics;
using FracturedState.Game.Data;
using UnityEngine;

namespace FracturedState.Game.Nav
{
    public class AStarPathNode : IComparable<AStarPathNode>
    {
        public int GCost = int.MaxValue;
        public int FCost;
        public NavGridPoint Point;
        public NavGridPoint Parent;

        public int CompareTo(AStarPathNode other)
        {
            if (FCost < other.FCost) return -1;
            return FCost > other.FCost ? 1 : 0;
        }
    }

    public class AStarPather
    {
        private static AStarPather instance;
        public static AStarPather Instance => instance ?? (instance = new AStarPather());

        public NavGrid ExteriorGrid { get; private set; }

//        private readonly Stopwatch pathTimer = new Stopwatch();

        private AStarPather() { }

        public void GenerateExteriorGrid(RawMapData mapData)
        {
            ExteriorGrid = new NavGrid(mapData.XUpperBound, mapData.XLowerBound, mapData.ZUpperBound, mapData.ZLowerBound);
        }

        private List<Vector3> DoPath(NavGrid grid, Vector3 start, Vector3 end, float unitRadius, int? startId = null, int? endId = null)
        {
            var startPoint = grid.GetClosestPoint(start, unitRadius, startId);
            var endPoint = grid.GetClosestPoint(end,unitRadius, endId);
            
            var closedSet = new HashSet<NavGridPoint>();
            var map = new Dictionary<NavGridPoint, AStarPathNode>();
            var openSet = new PriorityQueue<AStarPathNode>();
            var openLookup = new Dictionary<NavGridPoint, AStarPathNode>();

            var startNode = new AStarPathNode
            {
                Point = startPoint,
                FCost = Mathf.Abs(startPoint.Xindex - endPoint.Xindex) + Mathf.Abs(startPoint.Zindex - endPoint.Zindex),
                GCost = 0
            };
            map[startPoint] = startNode;
            openLookup[startPoint] = startNode;
            openSet.Enqueue(startNode);

            

            while (!openSet.IsEmpty())
            {
                var nextNode = openSet.Dequeue();
                openLookup.Remove(nextNode.Point);
                closedSet.Add(nextNode.Point);
                
                if (nextNode.Point == endPoint)
                {
                    
                    var path = new List<AStarPathNode>();
                    var n = nextNode;
                    while (n.Parent != null)
                    {
                        path.Insert(0, n);
                        n = map[n.Parent];
                    }

                    var vecPath = new List<Vector3>();
                    for (var i = 0; i < path.Count; i++)
                    {
                        vecPath.Add(path[i].Point.WorldPosition);
                    }
                    return vecPath;
                }
                
                var neighbors = grid.GetNeighbors(nextNode.Point);
                for (var i = 0; i < neighbors.Count; i++)
                {
                    if (closedSet.Contains(neighbors[i]))
                        continue;

                    if (neighbors[i].Radius < unitRadius)
                    {
                        closedSet.Add(neighbors[i]);
                        continue;
                    }

                    var gCost = nextNode.GCost + Mathf.Abs(nextNode.Point.Xindex - neighbors[i].Xindex) + Mathf.Abs(nextNode.Point.Zindex - neighbors[i].Zindex);

                    AStarPathNode newNode;
                    if (!openLookup.TryGetValue(neighbors[i], out newNode))
                    {
                        newNode = new AStarPathNode {Point = neighbors[i]};
                        map[neighbors[i]] = newNode;
                        openLookup[neighbors[i]] = newNode;
                        newNode.GCost = gCost;
                        newNode.FCost = gCost + Mathf.Abs(neighbors[i].Xindex - endPoint.Xindex) + Mathf.Abs(neighbors[i].Zindex - endPoint.Zindex);
                        newNode.Parent = nextNode.Point;
                        openSet.Enqueue(newNode);
                    }
                    else if (newNode.GCost > gCost)
                    {
                        newNode.GCost = gCost;
                        newNode.Parent = nextNode.Point;
                        newNode.FCost = gCost + Mathf.Abs(neighbors[i].Xindex - endPoint.Xindex) + Mathf.Abs(neighbors[i].Zindex - endPoint.Zindex);
                        openSet.ChangePriority(newNode);
                    }
                }
            }

            return null;
        }

        public static List<Vector3> SmoothPath(List<Vector3> nodeList, float unitRadius, State state)
        {
            if (nodeList == null || nodeList.Count == 0)
                return null;

            var path = new List<Vector3>();
            if (nodeList.Count == 1)
            {
                path.Add(nodeList[0]);
                return path;
            }

            var endPoint = nodeList[nodeList.Count - 1];
            var mask = (state == State.Exterior) ? GameConstants.ExteriorNavMask : GameConstants.InteriorMask;

            var i = 1;
            var currentPoint = nodeList[0];
            do
            {
                if (Mathf.Abs(currentPoint.y - nodeList[i].y) > 0.001f || Physics.CheckCapsule(currentPoint, nodeList[i], unitRadius, mask))
                {
                    path.Add(nodeList[i]);
                    currentPoint = nodeList[i];
                }
                i++;
            } while (i < nodeList.Count - 1);
            path.Add(endPoint);
            return path;
        }

        public List<Vector3> PlanInteriorPath(NavGrid structureGrid, Vector3 start, Vector3 end, float unitRadius)
        {
            return DoPath(structureGrid, start, end, unitRadius);
        }

        public List<Vector3> PlanExteriorPath(Vector3 start, Vector3 end, float unitRadius, int? startId, int? endId)
        {
            return DoPath(ExteriorGrid, start, end, unitRadius, startId, endId);
        }
    }
}