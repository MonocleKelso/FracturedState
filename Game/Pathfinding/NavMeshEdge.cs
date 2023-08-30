using System;
using System.Runtime.Serialization;

namespace FracturedState.Game.Nav
{
    /// <summary>
    /// The class representing an edge between two points in a navigation mesh.  Three of these make up a NavMeshTriangle
    /// </summary>
    [Serializable]
    public class NavMeshEdge : ISerializable
    {
        public NavMeshPoint FirstPoint { get; private set; }
        public NavMeshPoint SecondPoint { get; private set; }

        public NavMeshTriangle[] Triangles { get; private set; }

        public NavMeshEdge(NavMeshPoint firstPoint, NavMeshPoint secondPoint)
        {
            FirstPoint = firstPoint;
            SecondPoint = secondPoint;
        }

        protected NavMeshEdge(SerializationInfo info, StreamingContext context)
        {
            FirstPoint = (NavMeshPoint)info.GetValue("FirstPoint", typeof(NavMeshPoint));
            SecondPoint = (NavMeshPoint)info.GetValue("SecondPoint", typeof(NavMeshPoint));
            Triangles = (NavMeshTriangle[])info.GetValue("Triangles", typeof(NavMeshTriangle[]));
        }

        /// <summary>
        /// Returns the number of triangles that this edge is shared by
        /// </summary>
        public int TriangleCount
        {
            get
            {
                if (Triangles == null || (Triangles[0] == null && Triangles[1] == null))
                {
                    return 0;
                }

                if (Triangles[0] == null || Triangles[1] == null)
                {
                    return 1;
                }

                return 2;
            }
        }

        /// <summary>
        /// Returns the point in the edge that is not the point passed in
        /// Throws an exception if point is not contained in this edge
        /// </summary>
        public NavMeshPoint GetOtherPoint(NavMeshPoint point)
        {
            if (FirstPoint == point)
                return SecondPoint;

            if (SecondPoint == point)
                return FirstPoint;

            throw new FracturedStateException("Attempted to find other point on edge that does not contain point specified.");
        }

        /// <summary>
        /// Returns the triangle used by this edge that is NOT the given triangle.  Returns null if this edge is only used by one triangle
        /// </summary>
        public NavMeshTriangle GetOtherTriangle(NavMeshTriangle triangle)
        {
            if (Triangles[0] == triangle)
                return Triangles[1];
            if (Triangles[1] == triangle)
                return Triangles[0];

            return null;
        }

        /// <summary>
        /// Returns true if this edge contains the given point
        /// </summary>
        public bool ContainsPoint(NavMeshPoint point)
        {
            if (FirstPoint == point || SecondPoint == point)
                return true;

            return false;
        }

        /// <summary>
        /// Adds a triangle shared by this edge.  Throws an exception if this edge is already shared by two triangles
        /// </summary>
        public void AddTriangle(NavMeshTriangle triangle)
        {
            if (Triangles == null)
            {
                Triangles = new NavMeshTriangle[2];
            }

            if (Triangles[0] == null)
            {
                Triangles[0] = triangle;
            }
            else if (Triangles[1] == null)
            {
                Triangles[1] = triangle;
            }
            else
            {
                throw new FracturedStateException("Triangle edge limit exceeded.");
            }
        }

        /// <summary>
        /// Deletes the given triangle from this edge.  Throws an exception if this edge isn't shared by any triangles or this edge isn't shared by the given triangle
        /// </summary>
        public void DeleteTriangle(NavMeshTriangle triangle)
        {
            if (Triangles == null)
                throw new FracturedStateException("Cannot delete from empty triangle edge.");

            if (Triangles[0] == triangle)
            {
                Triangles[0] = null;
            }
            else if (Triangles[1] == triangle)
            {
                Triangles[1] = null;
            }
            else
            {
                throw new FracturedStateException("Attempted to delete triangle from non-containing edge.");
            }
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("FirstPoint", FirstPoint);
            info.AddValue("SecondPoint", SecondPoint);
            info.AddValue("Triangles", Triangles);
        }
    }
}