using UnityEngine;
using System.Linq;
using FracturedState.Game;
using FracturedState.Game.Data;
using System.Collections.Generic;
using System.Reflection;
using ThreeEyedGames;

namespace FracturedState.ModTools
{
    public class OpenMapTool : OwnedTool<MapEditorToolManager>
    {
        private string[] mapFileList;

        public OpenMapTool(MapEditorToolManager owner)
            : base(owner)
        {
            mapFileList = DataUtil.GetMapFileListForDisplay();
            this.owner.RightClickAction = null;
        }

        public override void DrawToolOptions()
        {
            GUILayout.BeginVertical();

            for (var i = 0; i < mapFileList.Length; i++)
            {
                if (GUILayout.Button(mapFileList[i]))
                {
                    owner.ClearCurrentMap();
                    var mapData = DataUtil.DeserializeXml<RawMapData>(DataLocationConstants.GameRootPath + DataLocationConstants.MapDirectory + "/" + mapFileList[i] + "/map.xml");
                    
                    owner.Territories = new List<TerritoryData>(mapData.Territories);
                    
                    var worldParent = GameObject.Find("MapParent").transform;
                    var localParent = worldParent.Find("Terrain");

                    var terrainList = DataUtil.DeserializeXml<TerrainList>(DataLocationConstants.GameRootPath + DataLocationConstants.TerrainDataFile);
                    foreach (var terrain in mapData.Terrains)
                    {
                        var terrainData = terrainList.Entries.FirstOrDefault(t => t.Id == int.Parse(terrain.BaseObjectName)) ;
                        if (terrainData == null) continue;
                        
                        var tGo = DataUtil.LoadBuiltInModel(terrainData.ModelName);
                        tGo.SetLayerRecursively(GameConstants.TerrainLayer);
                        tGo.name = terrainData.Id.ToString();
                        Vector3 pos;
                        terrain.PositionString.TryVector3(out pos);
                        tGo.transform.position = pos;
                        Vector3 rot;
                        terrain.RotationString.TryVector3(out rot);
                        tGo.transform.rotation = Quaternion.Euler(rot);
                        tGo.transform.parent = localParent;
                        if (terrain.TerritoryID >= 0 && mapData.Territories.Length > 0)
                        {
                            owner.AssignTerritory(tGo, mapData.Territories[terrain.TerritoryID]);
                        }
                    }

                    var idLookup = new Dictionary<int, GameObject>();
                    localParent = worldParent.Find("Structures");
                    foreach (var structure in mapData.Structures)
                    {
                        var structureData = XmlCacheManager.Structures.Values.FirstOrDefault(s => s.Name == structure.BaseObjectName);
                        if (structureData == null) continue;

                        var sGo = new GameObject(structure.BaseObjectName);
                        idLookup[structure.UID] = sGo;
                        if (structureData.Model.ExteriorModel != null)
                        {
                            var ext = DataUtil.LoadBuiltInModel(structureData.Model.ExteriorModel);
                            ext.transform.parent = sGo.transform;
                            ext.SetLayerRecursively(GameConstants.ExteriorLayer);
                        }
                        if (structureData.Model.InteriorModel != null)
                        {
                            var intr = DataUtil.LoadBuiltInModel(structureData.Model.InteriorModel);
                            intr.transform.parent = sGo.transform;
                            intr.SetLayerRecursively(GameConstants.InteriorLayer);
                        }
                        Vector3 pos;
                        Vector3 rot;
                        structure.PositionString.TryVector3(out pos);
                        structure.RotationString.TryVector3(out rot);
                        sGo.transform.position = pos;
                        sGo.transform.rotation = Quaternion.Euler(rot);
                        var m = sGo.AddComponent<MapDataContainer>();
                        m.SetName(sGo.name);
                        m.SetPositionRotation();
                        sGo.transform.parent = localParent;
                    }

                    localParent = worldParent.Find("Props");
                    foreach (var prop in mapData.Props)
                    {
                        var propData = XmlCacheManager.Props.Values.FirstOrDefault(p => p.Name == prop.BaseObjectName);
                        if (propData == null) continue;
                        
                        var pGo = new GameObject(propData.Name);
                        var model = DataUtil.LoadBuiltInModel(propData.Model.ExteriorModel);
                        model.transform.parent = pGo.transform;
                        pGo.SetLayerRecursively(prop.Layer);
                        Vector3 pos;
                        Vector3 rot;
                        prop.PositionString.TryVector3(out pos);
                        prop.RotationString.TryVector3(out rot);
                        pGo.transform.position = pos;
                        pGo.transform.rotation = Quaternion.Euler(rot);
                        if (propData.BoundsBox != null)
                        {
                            Vector3 center, bounds;
                            if (propData.BoundsBox.Center.TryVector3(out center) && propData.BoundsBox.Bounds.TryVector3(out bounds))
                            {
                                var col = pGo.AddComponent<BoxCollider>();
                                col.center = center;
                                col.size = bounds;
                            }
                        }
                        var m = pGo.AddComponent<MapDataContainer>();
                        m.SetName(pGo.name);
                        m.SetPositionRotation();
                        pGo.transform.parent = localParent;
                    }

                    foreach (var startingPos in mapData.StartingPoints)
                    {
                        Vector3 pos;
                        startingPos.TryVector3(out pos);
                        var point = Object.Instantiate(PrefabManager.StartingPoint);
                        point.transform.position = pos;
                    }

                    if (mapData.Decals != null)
                    {
                        foreach (var mapDecal in mapData.Decals)
                        {
                            var d = Resources.Load<GameObject>($"{DataLocationConstants.BuiltInDecalDirecotry}{mapDecal.Name}");
                            if (d == null) continue;
                            var decal = Object.Instantiate(d).GetComponent<Decal>();

                            decal.name = mapDecal.Name;
                            
                            Vector3 pos, rot, scale;
                            if (mapDecal.Position.TryVector3(out pos))
                            {
                                decal.transform.position = pos;
                            }

                            if (mapDecal.Rotation.TryVector3(out rot))
                            {
                                decal.transform.rotation = Quaternion.Euler(rot);
                            }

                            if (mapDecal.Scale.TryVector3(out scale))
                            {
                                decal.transform.localScale = scale;
                            }

                            decal.Fade = mapDecal.Alpha;
                            
                            if (mapDecal.TerrainLimited)
                            {
                                decal.LimitTo = worldParent.Find("Terrain").gameObject;
                            }
                            else if (mapDecal.StructureId > 0)
                            {
                                GameObject limit;
                                if (idLookup.TryGetValue(mapDecal.StructureId, out limit))
                                {
                                    if (mapDecal.LimitExterior)
                                    {
                                        foreach (Transform child in limit.transform)
                                        {
                                            if (child.gameObject.layer == GameConstants.ExteriorLayer)
                                            {
                                                decal.LimitTo = child.gameObject;
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        foreach (Transform child in limit.transform)
                                        {
                                            if (child.gameObject.layer == GameConstants.InteriorLayer)
                                            {
                                                decal.LimitTo = child.gameObject;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }

                            decal.gameObject.AddComponent<BoxCollider>();
                            decal.gameObject.SetLayerRecursively(GameConstants.DecalLayer);
                        }
                    }

                    owner.SetMapBounds(mapData.XUpperBound, mapData.XLowerBound, mapData.ZUpperBound, mapData.ZLowerBound);
                    owner.ToggleTerritoryHelpers(false);
                }
            }

            GUILayout.EndVertical();
        }
    }
}