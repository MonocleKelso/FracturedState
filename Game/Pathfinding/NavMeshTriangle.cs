using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace FracturedState.Game.Nav
{
    /// <summary>
    /// A class represnting an individual triangle in a navigation mesh
    /// </summary>
    [Serializable]
    public class NavMeshTriangle : ISerializable
    {
        public int Id { get; set; }
        public NavMeshEdge FirstEdge { get; private set; }
        public NavMeshEdge SecondEdge { get; private set; }
        public NavMeshEdge ThirdEdge { get; private set; }

        /// <summary>
        /// Returns the position that is roughly the center of this triangle in world space - (p1 + p2 + p3) / 3
        /// </summary>
        public Vector3 AverageWorldPosition
        {
            get
            {
                Vector3[] v = GetWorldPositionVertices();
                return ((v[0] + v[1] + v[2]) / 3);
            }
        }

        public NavMeshTriangle(NavMeshEdge firstEdge, NavMeshEdge secondEdge, NavMeshEdge thirdEdge)
        {
            FirstEdge = firstEdge;
            SecondEdge = secondEdge;
            ThirdEdge = thirdEdge;

            FirstEdge.AddTriangle(this);
            SecondEdge.AddTriangle(this);
            ThirdEdge.AddTriangle(this);
        }

        // binary serialization ctor
        protected NavMeshTriangle(SerializationInfo info, StreamingContext context)
        {
            Id = (int)info.GetValue("Id", typeof(int));
            FirstEdge = (NavMeshEdge)info.GetValue("FirstEdge", typeof(NavMeshEdge));
            SecondEdge = (NavMeshEdge)info.GetValue("SecondEdge", typeof(NavMeshEdge));
            ThirdEdge = (NavMeshEdge)info.GetValue("ThirdEdge", typeof(NavMeshEdge));
        }

        /// <summary>
        /// Returns the position of the three points making up this triangle in world space
        /// </summary>
        public Vector3[] GetWorldPositionVertices()
        {
            Vector3[] v = new Vector3[3];
            v[0] = FirstEdge.FirstPoint.WorldPosition;
            v[1] = FirstEdge.SecondPoint.WorldPosition;

            // use whatever point on the second edge that isn't shared by the first edge
            if (!FirstEdge.ContainsPoint(SecondEdge.FirstPoint))
            {
                v[2] = SecondEdge.FirstPoint.WorldPosition;
            }
            else
            {
                v[2] = SecondEdge.SecondPoint.WorldPosition;
            }

            return v;
        }

        /// <summary>
        /// Returns every triangle that shares an edge with this triangle.  The max number of triangles is 3 because an edge can only be shared by 2 triangles.
        /// Returns an empty list if the triangle is isolated
        /// </summary>
        public List<NavMeshTriangle> GetNeighbors()
        {
            List<NavMeshTriangle> n = new List<NavMeshTriangle>();

            NavMeshTriangle t1 = FirstEdge.GetOtherTriangle(this);
            if (t1 != null)
                n.Add(t1);

            NavMeshTriangle t2 = SecondEdge.GetOtherTriangle(this);
            if (t2 != null)
                n.Add(t2);

            NavMeshTriangle t3 = ThirdEdge.GetOtherTriangle(this);
            if (t3 != null)
                n.Add(t3);

            return n;
        }

        /// <summary>
        /// Returns the edge shared by this triangle and the given triangle or null of the triangles do not share an edge
        /// </summary>
        public NavMeshEdge GetSharedEdge(NavMeshTriangle other)
        {
            if (FirstEdge == other.FirstEdge || FirstEdge == other.SecondEdge || FirstEdge == other.ThirdEdge)
                return FirstEdge;
            if (SecondEdge == other.FirstEdge || SecondEdge == other.SecondEdge || SecondEdge == other.ThirdEdge)
                return SecondEdge;
            if (ThirdEdge == other.FirstEdge || ThirdEdge == other.SecondEdge || ThirdEdge == other.ThirdEdge)
                return ThirdEdge;

            return null;
        }

        /// <summary>
        /// Returns the point making up this triangle that is closest to the given world position
        /// </summary>
        public NavMeshPoint GetClosestTrianglePoint(Vector3 pos)
        {
            float p1Dist = (FirstEdge.FirstPoint.WorldPosition - pos).sqrMagnitude;
            float p2Dist = (FirstEdge.SecondPoint.WorldPosition - pos).sqrMagnitude;
            NavMeshPoint thirdPoint;

            if (!FirstEdge.ContainsPoint(SecondEdge.FirstPoint))
            {
                thirdPoint = SecondEdge.FirstPoint;
            }
            else
            {
                thirdPoint = SecondEdge.SecondPoint;
            }

            float p3Dist = (thirdPoint.WorldPosition - pos).sqrMagnitude;

            if (p1Dist < p2Dist && p1Dist < p3Dist)
            {
                return FirstEdge.FirstPoint;
            }

            if (p2Dist < p3Dist)
            {
                return FirstEdge.SecondPoint;
            }
            return thirdPoint;
        }

        /// <summary>
        /// Returns the distance of the closest point making up this triangle and the given position
        /// </summary>
        public double GetDistanceToClosestVertex(Vector3 origin)
        {
            double d1, d2, d3;
            Vector3[] worldPoints = GetWorldPositionVertices();
            d1 = (worldPoints[0] - origin).sqrMagnitude;
            d2 = (worldPoints[1] - origin).sqrMagnitude;
            d3 = (worldPoints[2] - origin).sqrMagnitude;

            if (d1 < d2)
            {
                if (d1 < d3)
                    return d1;
                return d3;
            }

            if (d2 < d3)
                return d2;
            return d3;
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Id", Id);
            info.AddValue("FirstEdge", FirstEdge);
            info.AddValue("SecondEdge", SecondEdge);
            info.AddValue("ThirdEdge", ThirdEdge);
        }
    }
}