using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace FracturedState.Game.Nav
{
    /// <summary>
    /// A class representing a point on a navigation mesh.  This class also provides accessors to other parts of the navigation mesh heirarchy
    /// </summary>
    [Serializable]
    public class NavMeshPoint : ISerializable
    {
        public Vector3 WorldPosition { get; private set; }
        public List<NavMeshEdge> Edges { get; private set; }

        public NavMeshPoint(Vector3 worldPosition)
        {
            WorldPosition = worldPosition;
            Edges = new List<NavMeshEdge>();

        }

        // binary ctor
        protected NavMeshPoint(SerializationInfo info, StreamingContext context)
        {
            float x = (float)info.GetValue("X", typeof(float));
            float y = (float)info.GetValue("Y", typeof(float));
            float z = (float)info.GetValue("Z", typeof(float));
            WorldPosition = new Vector3(x, y, z);
            Edges = (List<NavMeshEdge>)info.GetValue("Edges", typeof(List<NavMeshEdge>));
        }

        /// <summary>
        /// Returns the edge shared by this point and the given point or null if these 2 points do not share an edge
        /// </summary>
        public NavMeshEdge GetEdgeWithPoint(NavMeshPoint otherPoint)
        {
            foreach (NavMeshEdge edge in Edges)
            {
                if (edge.GetOtherPoint(this) == otherPoint)
                    return edge;
            }
            return null;
        }

        /// <summary>
        /// Adds the given edge to this point.  Returns false if the edge was already included in this point or true otherwise
        /// </summary>
        public bool AddEdge(NavMeshEdge edge)
        {
            if (!Edges.Contains(edge))
            {
                Edges.Add(edge);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes the given edge from this point.  Returns false if the edge was not included in this point to begin with
        /// </summary>
        public bool RemoveEdge(NavMeshEdge edge)
        {
            return Edges.Remove(edge);
        }

        public Vector3 GetAdjustedWorldPosition(float radius)
        {
            Vector3 avgPos = Vector3.zero;
            int count = 0;
            for (var i = 0; i < Edges.Count; i++)
            {
                count += Edges[i].TriangleCount;
                for (var t = 0; t < Edges[i].TriangleCount; t++)
                {
                    avgPos += Edges[i].Triangles[t].AverageWorldPosition;
                }
            }
            avgPos /= count;
            avgPos -= new Vector3(radius, 0, radius);
            return avgPos;
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("X", WorldPosition.x);
            info.AddValue("Y", WorldPosition.y);
            info.AddValue("Z", WorldPosition.z);
            info.AddValue("Edges", Edges);
        }
    }
}