using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;

namespace FracturedState.Game.Nav
{
    public enum NavMeshRole { Exterior, Interior }

    /// <summary>
    /// This class represents the data structures that makes up a navigation mesh used in a map.  It provides accessors into triangles by index with the intention
    /// of being Raycast against as well as some helper methods for accessing deeper parts of the structure (triangles, edges, and points).  This class is also responsible
    /// for the visual representation of the navigation mesh in the Map Editor.
    /// </summary>
    [Serializable]
    public class NavMesh : ISerializable
    {
        private List<NavMeshTriangle> triangles;
        private GameObject helper;
        private Mesh helperMesh;

        private List<Vector3> helperVerts = new List<Vector3>();
        private List<int> helperTriangles = new List<int>();

        private Dictionary<int, NavMeshTriangle> triangleIndex;

        private int currentTriangleId;

        /// <summary>
        /// The total number of triangles in this navigation mesh
        /// </summary>
        public int TriangleCount
        {
            get
            {
                return triangles.Count;
            }
        }

        public NavMesh()
        {
            triangles = new List<NavMeshTriangle>();
        }

        public NavMesh(NavMeshRole role)
        {
            if (role == NavMeshRole.Exterior)
            {
                helper = new GameObject("NavMesh_Exterior");
                helper.layer = GameConstants.NavMeshExtLayer;
            }
            else if (role == NavMeshRole.Interior)
            {
                helper = new GameObject("NavMesh_Interior");
                helper.layer = GameConstants.NavMeshIntLayer;
            }
            MeshRenderer render = helper.AddComponent<MeshRenderer>();
            render.sharedMaterial = new Material(Shader.Find("FracturedState/NavMesh"));
            MeshFilter mf = helper.AddComponent<MeshFilter>();
            helperMesh = new Mesh();
            mf.sharedMesh = helperMesh;
            triangles = new List<NavMeshTriangle>();
        }

        // binary serialization ctor
        protected NavMesh(SerializationInfo info, StreamingContext context)
        {
            triangles = (List<NavMeshTriangle>)info.GetValue("triangles", typeof(List<NavMeshTriangle>));
            triangleIndex = new Dictionary<int, NavMeshTriangle>();
        }

        /// <summary>
        /// This is used when loading a navigation mesh from a file.  It's called after loading the entire data structure to ensure all
        /// data is loaded prior to hooking everything together
        /// </summary>
        public void InitTriangleIndex()
        {
            for (var i = 0; i < triangles.Count; i++)
            {
                triangleIndex[triangles[i].Id] = triangles[i];
            }
        }

        /// <summary>
        /// Returns the triangle associated with the given index.  The indices are the same as what would be returned by RaycastHit.triangleIndex
        /// </summary>
        /// <returns></returns>
        public NavMeshTriangle GetTriangleByIndex(int index)
        {
            if (triangleIndex == null)
                throw new FracturedStateException("Cannot get triangle by index when in edit mode.");

            NavMeshTriangle t;
            if (triangleIndex.TryGetValue(index, out t))
                return t;

            return null;
        }

        /// <summary>
        /// Returns the triangle that is closest to the given world position.  This is calculated by taking the average position of the triangle and comparing it
        /// to the given position.  This can be an expensive operation because the entire triangle collection is enumerated and evaluated.  This may also give odd results
        /// because a point might be closer to the position but the triangle containing that point might not be returned.
        /// </summary>
        public NavMeshTriangle GetTriangleByWorldPosition(Vector3 position)
        {
            NavMeshTriangle[] tris = triangleIndex.Values.ToArray();
            float dist = float.MaxValue;
            NavMeshTriangle t = null;
            for (var i = 0; i < tris.Length; i++)
            {
                float toTri = (tris[i].AverageWorldPosition - position).sqrMagnitude;
                if (toTri < dist)
                {
                    dist = toTri;
                    t = tris[i];
                }
            }
            return t;
        }

        /// <summary>
        /// Adds the given triangle to the navigation mesh, assigns it an index, and updates the Map Editor's helper mesh accordingly
        /// </summary>
        public void AddTriangle(NavMeshTriangle triangle)
        {
            if (helper == null)
            {
                helper = new GameObject("NavMesh");
                helper.layer = GameConstants.NavMeshExtLayer;
                MeshRenderer render = helper.AddComponent<MeshRenderer>();
                render.sharedMaterial = new Material(Shader.Find("FracturedState/NavMesh"));
                MeshFilter mf = helper.AddComponent<MeshFilter>();
                helperMesh = new Mesh();
                mf.sharedMesh = helperMesh;
            }

            // move helper back to zero
            Vector3 pos = helper.transform.position;
            pos.y = 0f;
            helper.transform.position = pos;

            triangles.Add(triangle);
            helperVerts.AddRange(triangle.GetWorldPositionVertices());

            triangle.Id = currentTriangleId++;

            int currentTri = helperTriangles.Count;
            helperTriangles.Add(currentTri++);
            helperTriangles.Add(currentTri++);
            helperTriangles.Add(currentTri++);

            helperMesh.Clear();
            helperMesh.vertices = helperVerts.ToArray();
            helperMesh.triangles = helperTriangles.ToArray();

            // bump helper up to draw above terrain
            pos = helper.transform.position;
            pos.y = 0.1f;
            helper.transform.position = pos;
        }

        /// <summary>
        /// Creates a triangle out of the given three edges and adds it to the navigation mesh
        /// </summary>
        public void AddTriangle(NavMeshEdge e1, NavMeshEdge e2, NavMeshEdge e3)
        {
            AddTriangle(new NavMeshTriangle(e1, e2, e3));
        }

        /// <summary>
        /// Adds a new triangle to the navigation mesh by creating three edges out of the given points.  This method always creates new edges
        /// </summary>
        public void AddTriangle(NavMeshPoint p1, NavMeshPoint p2, NavMeshPoint p3)
        {
            NavMeshEdge e1 = new NavMeshEdge(p1, p2);
            NavMeshEdge e2 = new NavMeshEdge(p2, p3);
            NavMeshEdge e3 = new NavMeshEdge(p1, p3);
            AddTriangle(e1, e2, e3);
        }

        /// <summary>
        /// Builds a mesh that can be raycasted against to provide pathfinding in the game.  The difference between this mesh and the Map Editor's helper mesh
        /// is that this one winds its triangles in the correct order to ensure a Mesh Collider works
        /// </summary>
        public void BuildHelperMesh(NavMeshRole role)
        {
            GameObject helperParent = GameObject.Find("NavMesh");
            if (role == NavMeshRole.Exterior)
            {
                helper = helperParent.transform.Find("Exterior").gameObject;
            }
            else
            {
                helper = helperParent.transform.Find("Interior").gameObject;
            }
            helperMesh = new Mesh();
            helperVerts = new List<Vector3>();
            helperTriangles = new List<int>();

            int triCount = 0;

            // triangle list sorted by ID to ensure that we build helper mesh in triangle index order for raycasting
            var sortedTri = from tri in triangles orderby tri.Id select tri;

            foreach (NavMeshTriangle tri in sortedTri)
            {
                Vector3[] v = tri.GetWorldPositionVertices();

                // ensure that vertics wind in the correct order to draw triangle facing world up
                // translated from: http://debian.fmi.uni-sofia.bg/~sergei/cgsr/docs/clockwise.htm
                int j, k;
                int count = 0;
                float z;
                for (var i = 0; i < 3; i++)
                {
                    j = (i + 1) % 3;
                    k = (i + 2) % 3;
                    z = (v[j].x - v[i].x) * (v[k].z - v[j].z);
                    z -= (v[j].z - v[i].z) * (v[k].x - v[j].x);
                    if (z < 0)
                        count--;
                    else
                        count++;
                }
                if (count > 0)
                {
                    v = new Vector3[3] { v[2], v[1], v[0] };
                }

                helperVerts.AddRange(v);
                helperTriangles.Add(triCount++);
                helperTriangles.Add(triCount++);
                helperTriangles.Add(triCount++);
            }

            helperMesh.vertices = helperVerts.ToArray();
            helperMesh.triangles = helperTriangles.ToArray();
            helper.GetComponent<MeshCollider>().sharedMesh = helperMesh;

            // dispose of helper lists because we don't need them anymore
            helperVerts.Clear();
            helperVerts = null;
            helperTriangles.Clear();
            helperTriangles = null;
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("triangles", triangles);
        }
    }
}