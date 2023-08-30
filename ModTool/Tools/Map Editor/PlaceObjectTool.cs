using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using FracturedState.Game;
using FracturedState.Game.Data;

namespace FracturedState.ModTools
{
    public enum ObjectToolCategory { None, Structure, Prop, System }

    public class PlaceObjectTool : OwnedTool<MapEditorToolManager>
    {
        private string[] categories;
        private Dictionary<string, bool> openCategories;
        private Dictionary<string, Prop[]> categorizedEntries;
		private string[] objectList;
		private Vector2 scrollPos;
        private ObjectToolCategory currentCategory = ObjectToolCategory.None;
		
		// a doppleganger object that follows the mouse
		private GameObject currentObject;
        private string currentObjectName;
		// the real object that gets place on the map
		private GameObject placeableObject;

        private readonly Transform parent;
	
        public PlaceObjectTool(MapEditorToolManager owner)
			: base(owner)
		{
			owner.RightClickAction = DeSelect;
            parent = GameObject.Find("MapParent").transform;
		}

        public override void DrawToolOptions()
        {
            GUILayout.BeginVertical();
			
			// Object Category Buttons
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Prop"))
            {
                DeSelect();
                categories = XmlCacheManager.Props.Values.Select(p => p.Category ?? "Misc").Distinct().OrderBy(c => c).ToArray();
                objectList = XmlCacheManager.Props.Values.Select(p => p.Name).ToArray();
                openCategories = new Dictionary<string, bool>();
                categorizedEntries = new Dictionary<string, Prop[]>();
                foreach (var cat in categories)
                {
                    openCategories[cat] = false;
                    if (cat == "Misc")
                    {
                        categorizedEntries[cat] = XmlCacheManager.Props.Values
                            .Where(p => string.IsNullOrEmpty(p.Category)).ToArray();
                    }
                    else
                    {
                        categorizedEntries[cat] = XmlCacheManager.Props.Values.Where(p => p.Category == cat).ToArray();
                    }
                }
                scrollPos = Vector2.zero;
                currentCategory = ObjectToolCategory.Prop;
            }
			
            if (GUILayout.Button("Struct"))
			{
                DeSelect();
				objectList = XmlCacheManager.Structures.Values.Select(s => s.Name).ToArray();
				scrollPos = Vector2.zero;
				currentCategory = ObjectToolCategory.Structure;
			}

            GUILayout.Button("Unit");

            if (GUILayout.Button("Sys"))
            {
                DeSelect();
                objectList = null;
                scrollPos = Vector2.zero;
                currentCategory = ObjectToolCategory.System;
            }
			
            GUILayout.EndHorizontal();

            scrollPos = GUILayout.BeginScrollView(scrollPos);

            // categorized props
            if (currentCategory == ObjectToolCategory.Prop)
            {
                foreach (var cat in categories)
                {
                    if (GUILayout.Button(cat))
                    {
                        openCategories[cat] = !openCategories[cat];
                    }

                    if (openCategories[cat])
                    {
                        foreach (var prop in categorizedEntries[cat])
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Space(10);
                            if (GUILayout.Button(prop.Name))
                            {
                                DeSelect();
                                currentObjectName = prop.Name;
                                currentObject = new GameObject("tempProp");
                                
                                int layer;
                                if (owner.MapCamera.GetComponent<EditorCameraController>().ViewState == CameraViewState.Exterior)
                                {
                                    layer = GameConstants.ExteriorLayer;
                                }
                                else
                                {
                                    layer = GameConstants.InteriorLayer;
                                }
                                if (prop.BoundsBox != null)
                                {
                                    Vector3 center, bounds;
                                    if (prop.BoundsBox.Center.TryVector3(out center) && prop.BoundsBox.Bounds.TryVector3(out bounds))
                                    {
                                        var col = currentObject.AddComponent<BoxCollider>();
                                        col.center = center;
                                        col.size = bounds;
                                    }
                                }
                                InitModel(prop.Model.ExteriorModel, currentObject.transform, layer);
                                
                                var p = currentObject.AddComponent<PlaceObjectToolMouseFollow>();
                                p.cam = owner.MapCamera;
                                p.RayMask = GameConstants.TerrainMask;
                            }
                            GUILayout.EndHorizontal();
                        }
                    }
                }
            }
			// Object List
			else if (objectList != null && objectList.Length > 0)
			{
				for (var i = 0; i < objectList.Length; i++)
				{
					if (GUILayout.Button(objectList[i]))
					{
                        DeSelect();
                        currentObjectName = objectList[i];
                        currentObject = new GameObject("tempProp");

                        // structures get interior/exterior art loaded
                        if (currentCategory == ObjectToolCategory.Structure)
                        {
                            var structure = XmlCacheManager.Structures[objectList[i]];
                            if (structure.Model.ExteriorModel != null)
                            {
                                InitModel(structure.Model.ExteriorModel, currentObject.transform, GameConstants.ExteriorLayer);
                            }
                            if (structure.Model.InteriorModel != null)
                            {
                                InitModel(structure.Model.InteriorModel, currentObject.transform, GameConstants.InteriorLayer);
                            }
                        }

                        var p = currentObject.AddComponent<PlaceObjectToolMouseFollow>();
                        p.cam = owner.MapCamera;
                        p.RayMask = GameConstants.TerrainMask;
					}
				}
			}
            // system objects
            else if (currentCategory == ObjectToolCategory.System)
            {
                if (GUILayout.Button("Starting Point"))
                {
                    DeSelect();
                    placeableObject = Object.Instantiate(PrefabManager.StartingPoint);
                    placeableObject.SetActive(false);
                }
            }
            GUILayout.EndScrollView();
			
            GUILayout.EndVertical();
        }
		
		protected override void DoToolExecution()
		{
            var ray = owner.MapCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            var rayCast = Physics.Raycast(ray, out hit, Mathf.Infinity, owner.TerrainMask);

            if (currentObject != null)
            {
                
                if (rayCast)
                {
                    var newObj = Object.Instantiate(currentObject, hit.point, currentObject.transform.rotation);
                    Object.Destroy(newObj.GetComponent<PlaceObjectToolMouseFollow>());
                    newObj.name = currentObjectName;

                    if (currentCategory == ObjectToolCategory.Structure)
                    {
                        var pos = newObj.transform.position;
                        pos.y += GameConstants.ObjectYAdjustment; // prevent z-fighting on interior view floor
                        newObj.transform.position = pos;
                        newObj.transform.parent = parent.Find("Structures");
                    }
                    else if (currentCategory == ObjectToolCategory.Prop)
                    {
                        if (owner.MapCamera.GetComponent<EditorCameraController>().ViewState == CameraViewState.Exterior)
                        {
                            newObj.layer = GameConstants.ExteriorLayer;
                        }
                        else
                        {
                            newObj.layer = GameConstants.InteriorLayer;
                        }
                        newObj.transform.parent = parent.Find("Props");
                    }
                }
            }
            else if (placeableObject != null)
            {
                if (currentCategory == ObjectToolCategory.System)
                {
                    if (rayCast)
                    {
                        var newObj = (GameObject)Object.Instantiate(placeableObject, hit.point, Quaternion.identity);
                        newObj.SetActive(true);
                    }
                }
            }
            else
            {
                // select existing object
                if (!owner.InSelectWindow())
                {
                    if (Physics.Raycast(ray, out hit, Mathf.Infinity, owner.ObjMask))
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
		}
		
		public override void Exit()
		{
			DeSelect();
		}
		
		private void DeSelect()
		{
            if (!owner.InSelectWindow())
            {
                if (currentObject != null)
                    Object.DestroyImmediate(currentObject);

                if (placeableObject != null)
                    Object.DestroyImmediate(placeableObject);

                owner.Unselect();
            }
		}

        private static void InitModel(string modelName, Transform modelParent, int layer)
        {
            var model = DataUtil.LoadBuiltInModel(modelName);
            model.SetLayerRecursively(layer);
            model.transform.parent = modelParent;
        }
    }
}