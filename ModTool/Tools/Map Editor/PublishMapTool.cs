using System.Collections;
using System.Collections.Generic;
using System.IO;
using FracturedState.Game;
using FracturedState.Game.Data;
using FracturedState.UI;
using ThreeEyedGames;
using UnityEngine;

namespace FracturedState.ModTools
{
	/// <summary>
	/// A Map Editor tool responsible for collecting all objects associated with the current session map and
	/// creating a directory full of data files that can be loaded by the game module.
	/// </summary>
	public class PublishMapTool : OwnedTool<MapEditorToolManager>
	{
		private string mapName = string.Empty;

        private string xUpperBound;
        private string xLowerBound;
        private string zUpperBound;
        private string zLowerBound;

		public PublishMapTool(MapEditorToolManager owner)
			: base(owner)
		{
			owner.RightClickAction = null;
            xUpperBound = this.owner.XUpperBound.ToString();
            xLowerBound = this.owner.XLowerBound.ToString();
            zUpperBound = this.owner.ZUpperBound.ToString();
            zLowerBound = this.owner.ZLowerBound.ToString();
		}
		
		public override void DrawToolOptions()
		{
			GUILayout.BeginVertical();
			
			GUILayout.Label("Map Name");
			mapName = GUILayout.TextField(mapName);
			
			if (GUILayout.Button("Publish Map"))
				PublishMap();

            GUILayout.BeginHorizontal();
            GUILayout.Label("X Upper:");
            xUpperBound = GUILayout.TextField(xUpperBound);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("X Lower:");
            xLowerBound = GUILayout.TextField(xLowerBound);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Z Upper:");
            zUpperBound = GUILayout.TextField(zUpperBound);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Z Lower:");
            zLowerBound = GUILayout.TextField(zLowerBound);
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Update Map Bounds"))
            {
                int xU, xL, zU, zL;
                if (int.TryParse(xUpperBound, out xU) && int.TryParse(xLowerBound, out xL) && int.TryParse(zUpperBound, out zU) && int.TryParse(zLowerBound, out zL))
                {
                    owner.SetMapBounds(xU, xL, zU, zL);
                }
            }

			GUILayout.EndVertical();
		}
		
		private void PublishMap()
		{
			if (mapName.Length == 0) return;
			
			var data = new RawMapData
			{
				MapName = mapName,
				XUpperBound = owner.XUpperBound,
				XLowerBound = owner.XLowerBound,
				ZUpperBound = owner.ZUpperBound,
				ZLowerBound = owner.ZLowerBound,
				Territories = owner.Territories.ToArray()
			};

			var spObjects = GameObject.FindGameObjectsWithTag("StartingPoint");
			data.StartingPoints = new Vec3String[spObjects.Length];
			for (var i = 0; i < spObjects.Length; i++)
			{
				data.StartingPoints[i] = new Vec3String(spObjects[i].transform.position);
			}

			var uid = 1;

			var mapParent = GameObject.Find("MapParent");
			var terrainParent = mapParent.transform.Find("Terrain");
			var terrainList = new List<CustomMapData>();
			foreach (Transform child in terrainParent)
			{
				terrainList.Add(new CustomMapData
				{
					BaseObjectName = child.gameObject.name,
					PositionString = new Vec3String(child.position),
					RotationString = new Vec3String(child.rotation.eulerAngles),
					UID = uid++,
					TerritoryID = owner.GetTerritoryIndex(child.gameObject)
				});
			}
			data.Terrains = terrainList.ToArray();

			// used later by decals to hookup their structure masks by ID
			var structUidLookup = new Dictionary<Transform, int>();
			
			var structParent = mapParent.transform.Find("Structures");
			var structureList = new List<CustomMapData>();
			foreach (Transform child in structParent)
			{
				var id = uid++;
				structureList.Add(new CustomMapData
				{
					BaseObjectName = child.gameObject.name,
					PositionString = new Vec3String(child.position),
					RotationString = new Vec3String(child.rotation.eulerAngles),
					UID = id
				});
				structUidLookup[child] = id;
			}
			data.Structures = structureList.ToArray();

			var propParent = mapParent.transform.Find("Props");
			var propList = new List<CustomMapData>();
			foreach (Transform prop in propParent)
			{
				propList.Add(new CustomMapData
				{
					BaseObjectName = prop.gameObject.name,
					PositionString = new Vec3String(prop.position),
					RotationString = new Vec3String(prop.rotation.eulerAngles),
					Layer = prop.gameObject.layer,
					UID = uid++
				});
			}
			data.Props = propList.ToArray();

			var decals = Object.FindObjectsOfType<Decal>();
			var decalList = new List<MapDecal>();
			foreach (var decal in decals)
			{
				var transform = decal.gameObject.transform;
				var mapDecal = new MapDecal
				{
					Name = decal.gameObject.name,
					Position = new Vec3String(transform.position),
					Rotation = new Vec3String(transform.rotation.eulerAngles),
					Scale = new Vec3String(transform.localScale),
					Alpha = decal.Fade,
					TerrainLimited = false
				};

				if (decal.LimitTo != null)
				{
					var parent = decal.LimitTo.transform.GetAbsoluteParent();
					int id;
					if (structUidLookup.TryGetValue(parent, out id))
					{
						mapDecal.StructureId = id;
						mapDecal.LimitExterior = decal.LimitTo.layer == GameConstants.ExteriorLayer;
					}
					else
					{
						// if we are limited to something but it's not a structure then it has to be terrain
						// otherwise if limit to is null then the decal is set to All
						mapDecal.TerrainLimited = true;
					}
				}
				
				decalList.Add(mapDecal);
			}

			data.Decals = decalList.ToArray();
					
			// each map gets its own directory
			var filePath = DataLocationConstants.GameRootPath + DataLocationConstants.MapDirectory + "/" + mapName;
			// map data file
			DataUtil.SerializeXml(data, filePath, "map.xml");
			
			owner.StartCoroutine(GeneratePreview(filePath));
		}

		private IEnumerator GeneratePreview(string filePath)
		{
			owner.SetBoundshelperActive(false);

			yield return null;
			
			var camera = new GameObject("PreviewCamera").AddComponent<Camera>();
			camera.orthographic = true;
			camera.cullingMask = 1 << GameConstants.ExteriorLayer | 1 << GameConstants.TerrainLayer;
			camera.renderingPath = RenderingPath.Forward;
			camera.backgroundColor = owner.PreviewBackgroundColor;

			var width = owner.XUpperBound - owner.XLowerBound;
			var height = owner.ZUpperBound - owner.ZLowerBound;
			camera.aspect = (float)width / height;

			var lower = new Vector3(owner.XLowerBound, 100, owner.ZLowerBound);
			var upper = new Vector3(owner.XUpperBound, 100, owner.ZUpperBound);
			camera.transform.position = (lower + upper) / 2f;
			camera.transform.Rotate(new Vector3(90, 0, 0));
			camera.orthographicSize = width > height ? height * 0.51f : width * 0.51f;

			yield return null;

			var preview = new RenderTexture(width * 5, height * 5, 0, RenderTextureFormat.ARGB32);
			camera.targetTexture = preview;
			RenderTexture.active = preview;
			camera.Render();
			var texture = new Texture2D(width * 5, height * 5, TextureFormat.ARGB32, false);
			texture.ReadPixels(new Rect(0, 0, width * 5, height * 5), 0, 0);
			var data = texture.EncodeToJPG();
			using (var file = new FileStream($"{filePath}/preview.jpg", FileMode.Create, FileAccess.Write))
			{
				file.Write(data, 0, data.Length);
			}

			yield return null;

			RenderTexture.active = null;
			Object.Destroy(texture);
			Object.Destroy(preview);
			Object.Destroy(camera.gameObject);
			owner.SetBoundshelperActive(true);
		}
	}
}