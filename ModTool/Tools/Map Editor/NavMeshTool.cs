using System.Collections.Generic;
using UnityEngine;
using FracturedState.Game;
using FracturedState.Game.Nav;

namespace FracturedState.ModTools
{
    public class NavMeshTool : OwnedTool<MapEditorToolManager>, IKeyStrokeTool
    {
        private enum NavMeshToolState { Select = 0, PlaceVertex = 1, CreateTri = 2 };
        private enum NavMeshToolMode { Exterior, Interior };
        private NavMeshToolMode currentMode;
        private string[] toolNames = new string[] { "Select", "Place Vertex", "Create Triangle" };
        private int currentTool = 0;
        private GameObject navMeshPointer;
        private int origCamMask;

        // lookup table for editor representations of navmesh points
        private static Dictionary<GameObject, NavMeshPoint> points;

        // temp container to use when manually creating triangles
        private List<GameObject> edgePoints;

        public NavMeshTool(MapEditorToolManager owner)
            : base(owner)
        {
            owner.RightClickAction = RightClick;
            currentMode = NavMeshToolMode.Exterior;
            navMeshPointer = FracturedState.Game.PrefabManager.NavMeshPointHelper;
            points = new Dictionary<GameObject, NavMeshPoint>();
            edgePoints = new List<GameObject>();
        }

        public override void Enter()
        {
            origCamMask = owner.MapCamera.cullingMask;
            owner.MapCamera.cullingMask = origCamMask | GameConstants.NavMeshAllMask;
        }

        public override void DrawToolOptions()
        {
            GUILayout.BeginVertical();

            currentTool = GUILayout.SelectionGrid(currentTool, toolNames, 1);
            GUILayout.Label(currentMode.ToString());

            if (GUILayout.Button("Make Nav Grid"))
            {
                GameObject[] helpers = GameObject.FindGameObjectsWithTag("NavHelper");
                foreach (GameObject helper in helpers)
                {
                    GameObject.DestroyImmediate(helper);
                }
            }

            GUILayout.EndVertical();
        }

        public void DoKeyStroke(KeyCode keyCode)
        {
            if (keyCode == KeyCode.Tab)
            {
                currentMode = (currentMode == NavMeshToolMode.Exterior) ? NavMeshToolMode.Interior : NavMeshToolMode.Exterior;
                edgePoints.Clear();
            }
        }

        protected override void DoToolExecution()
        {
            NavMeshToolState toolState = (NavMeshToolState)currentTool;
            Ray ray = owner.MapCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (toolState == NavMeshToolState.PlaceVertex)
            {
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, owner.TerrainMask))
                {
                    GameObject helper = (GameObject)GameObject.Instantiate(navMeshPointer, hit.point, Quaternion.identity);
                    if (currentMode == NavMeshToolMode.Exterior)
                    {
                        helper.layer = FracturedState.Game.GameConstants.NavMeshExtLayer;
                    }
                    else
                    {
                        helper.layer = FracturedState.Game.GameConstants.NavMeshIntLayer;
                    }
                    points[helper] = new NavMeshPoint(hit.point);
                }
            }
            else
            {
                if (toolState == NavMeshToolState.CreateTri)
                {
                    int layerMask = (currentMode == NavMeshToolMode.Exterior) ? GameConstants.ExteriorMoveMask : GameConstants.InteriorMoveMask;
                    if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
                    {
                        if (!edgePoints.Contains(hit.collider.gameObject))
                        {
                            edgePoints.Add(hit.collider.gameObject);
                        }
                        if (edgePoints.Count == 3)
                        {
                            NavMeshPoint p1 = points[edgePoints[0]];
                            NavMeshPoint p2 = points[edgePoints[1]];
                            NavMeshPoint p3 = points[edgePoints[2]];

                            // attempt to find edges already created based on selected points
                            // or create new edges if none exist
                            NavMeshEdge e1, e2, e3;
                            e1 = p1.GetEdgeWithPoint(p2);
                            if (e1 == null)
                            {
                                e1 = new NavMeshEdge(p1, p2);
                                p1.AddEdge(e1);
                                p2.AddEdge(e1);
                            }
                            e2 = p2.GetEdgeWithPoint(p3);
                            if (e2 == null)
                            {
                                e2 = new NavMeshEdge(p2, p3);
                                p2.AddEdge(e2);
                                p3.AddEdge(e2);
                            }
                            e3 = p1.GetEdgeWithPoint(p3);
                            if (e3 == null)
                            {
                                e3 = new NavMeshEdge(p1, p3);
                                p1.AddEdge(e3);
                                p3.AddEdge(e3);
                            }

                            if (e1.TriangleCount < 2 && e2.TriangleCount < 2 && e3.TriangleCount < 2)
                            {
                                if (currentMode == NavMeshToolMode.Exterior)
                                {
                                    // owner.ExteriorNavMesh.AddTriangle(e1, e2, e3);
                                }
                                else
                                {
                                    // owner.InteriorNavMesh.AddTriangle(e1, e2, e3);
                                }
                            }
                            else
                            {
                                // TODO: Proper heads up to user about NavMesh triangle creation status
                                Debug.Log("Can't create triangle because one or more edges is already shared by 2 triangles");
                            }

                            edgePoints.Clear();
                        }
                    }
                    else
                    {
                        edgePoints.Clear();
                    }
                }
            }
        }

        public void RightClick()
        {
            currentTool = 0;
        }

        public override void Exit()
        {
            owner.MapCamera.cullingMask = origCamMask;
        }
    }
}