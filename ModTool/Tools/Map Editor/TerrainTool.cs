using System.Collections.Generic;
using System.Linq;
using FracturedState.Game;
using FracturedState.Game.Data;
using UnityEngine;

namespace FracturedState.ModTools
{
    public class TerrainTool : OwnedTool<MapEditorToolManager>
    {
        private string[] categories;
        private Dictionary<string, bool> openCategories;
        private Dictionary<string, TerrainEntry[]> categorizedEntries;

        private readonly Transform parent;

        private GameObject current;
        private string currentName;
        private Vector2 scrollPos;

        public TerrainTool(MapEditorToolManager owner)
            : base(owner)
        {
            owner.RightClickAction = DeSelect;
            parent = GameObject.Find("MapParent").transform.Find("Terrain");
        }

        public override void Enter()
        {
            var terrainList = DataUtil.DeserializeXml<TerrainList>(DataLocationConstants.GameRootPath + DataLocationConstants.TerrainDataFile);
            categories = terrainList.Entries.Where(t => !string.IsNullOrEmpty(t.EditorCategory)).Select(t => t.EditorCategory).Distinct().OrderBy(c => c).ToArray();
            openCategories = new Dictionary<string, bool>();
            categorizedEntries = new Dictionary<string, TerrainEntry[]>();
            foreach (var cat in categories)
            {
                openCategories[cat] = false;
                categorizedEntries[cat] = terrainList.Entries.Where(t => t.EditorCategory == cat && !t.Hidden).OrderBy(t => t.DisplayName).ToArray();
            }
        }

        public override void DrawToolOptions()
        {
            GUILayout.BeginVertical();
            
            scrollPos = GUILayout.BeginScrollView(scrollPos);

            foreach (var cat in categories)
            {
                if (GUILayout.Button(cat))
                {
                    openCategories[cat] = !openCategories[cat];
                }

                if (openCategories[cat])
                {
                    foreach (var tile in categorizedEntries[cat])
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(10);
                        if (GUILayout.Button(tile.DisplayName))
                        {
                            DeSelect();
                            currentName = tile.Id.ToString();
                            current = DataUtil.LoadBuiltInModel(tile.ModelName);
                            var mf = current.AddComponent<TerrainToolMouseFollow>();
                            mf.cam = owner.MapCamera;
                        }
                        GUILayout.EndHorizontal();
                    }
                }
            }
            
            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }

        protected override void DoToolExecution()
        {
            if (current != null)
            {
                var t = Object.Instantiate(current, current.transform.position, current.transform.rotation);
                Object.Destroy(t.GetComponent<TerrainToolMouseFollow>());
                var p = t.transform.position;
                p.y = 0;
                t.transform.position = p;
                t.transform.parent = parent;
                t.name = currentName;
                t.gameObject.SetLayerRecursively(GameConstants.TerrainLayer);
            }
            else
            {
                var ray = owner.MapCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, owner.TerrainMask))
                {
                    var t = hit.transform.GetAbsoluteParent();
                    owner.SetSelectedObject(t);
                }
                else if (!Physics.Raycast(ray, Mathf.Infinity, GameConstants.GizmoMask))
                {
                    owner.Unselect();
                }
            }
        }

        public override void Exit()
        {
            DeSelect();
        }

        private void DeSelect()
        {
            owner.Unselect();
            if (current != null)
                Object.DestroyImmediate(current);
        }
    }
}