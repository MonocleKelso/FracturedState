using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;
using FracturedState.Game.Management;
using FracturedState.Game.Nav;
using FracturedState.Game.Network;
using ThreeEyedGames;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace FracturedState.Game.Data
{
    public static class DataUtil
    {
        /// <summary>
        /// Returns a list of files contained in this directory. Each entry is the full path to the file
        /// </summary>
        /// <param name="directory">The root directory to search</param>
        /// <param name="fileExtension">The file format to filter the list of files by</param>
        public static string[] GetFiles(string directory, string fileExtension)
        {
            return Directory.Exists(directory) ? Directory.GetFiles(directory, "*." + fileExtension) : null;
        }

        /// <summary>
        /// Deletes the given file if it exists. If it does not exist then this method does nothing. This method also silently fails if an Exception is thrown
        /// </summary>
        public static void DeleteFile(string filePath)
        {
            if (!File.Exists(filePath)) return;
            try
            {
                File.Delete(filePath);
            }
            catch (Exception)
            {
                // ignored
            }
        }

		/// <summary>
		/// Loads the XML file at the given path and returns the given Typed object.  This method does not wrap any
		/// exception that might occur because the assumption is that a failure to load a file will result in the application
		/// displaying the error to the user and then exiting.  This is done because loaded files are always hardcoded and
		/// required or because the file is loaded as part of a directory search - ie a failure to load is an extreme case.
		/// </summary>
        /// <typeparam name="T">The type of object to be deserialized and returned</typeparam>
        /// <param name="filePath">The absolute path to the XML file being deserialized</param>
        public static T DeserializeXml<T>(string filePath)
        {
            T data;
            using (var file = new FileStream(filePath, FileMode.Open))
            {
                var serial = new XmlSerializer(typeof(T));
                data = (T)serial.Deserialize(file);
            }
            return data;
        }

        /// <summary>
        /// Deserializes the given raw XML data into the provided type and returns it.
        /// </summary>
        /// <typeparam name="T">The type of object to be deserialized and returned</typeparam>
        /// <param name="xmlData">The string representation of the XML data to deserialize</param>
        public static T DeserializeXmlString<T>(string xmlData)
        {
            T data;
            using (TextReader reader = new StringReader(xmlData))
            {
                var serial = new XmlSerializer(typeof(T));
                data = (T)serial.Deserialize(reader);
            }
            return data;
        }

        /// <summary>
        /// Loads the file at the given path and returns the given Typed object via binary deserialization. This method does
        /// not wrap any exception that might occur because the assumption is that a failureto load a file will result in the
        /// application displaying the error to the user and then exiting. This is done because loaded files are always hardcoded and
        /// required or because the file is loaded as part of a directory search - ie a failure to load is an extreme case.
        /// </summary>
        /// <typeparam name="T">The type of the object to be deserialized and returned</typeparam>
        /// <param name="filePath">The absolute path to the file being deserialized including file extension</param>
        public static T DeserializeBinary<T>(string filePath)
        {
            T data;
            using (var file = new FileStream(filePath, FileMode.Open))
            {
                var bFormat = new BinaryFormatter();
                data = (T)bFormat.Deserialize(file);
            }
            return data;
        }

		/// <summary>
		/// Saves the given object as an XML file at the given path.  Note that the extension does not need to be .xml but it
		/// will be treated as such.  This method throws a FracturedStateException that wraps any IO exception that occurs.
		/// The expectation being that the calling method will catch and handle the error.  This was done because this
		/// method is considered a save operation so the application does not necessarily need to exit due to a failure.
		/// </summary>
        /// <typeparam name="T">The type of the object being saved</typeparam>
        /// <param name="obj">The object being saved</param>
        /// <param name="filePath">The absolute location of the resulting file without a trailing slash</param>
        /// <param name="fileName">The name of the resulting file including the extension</param>
		public static void SerializeXml<T>(T obj, string filePath, string fileName)
		{
            Stream file = null;
			try
			{
                if (!Directory.Exists(filePath))
                    Directory.CreateDirectory(filePath);

				var serial = new XmlSerializer(typeof(T));
                file = new FileStream(filePath + "/" + fileName, FileMode.Create);
                using (TextWriter t = new StreamWriter(file, new UTF8Encoding()))
                {
                    file = null;
                    serial.Serialize(t, obj);
                }
			}
			catch (Exception e)
			{
				throw new FracturedStateException("Cannot serialize object of Type " + typeof(T), e);
			}
            finally
			{
			    file?.Dispose();
			}
		}

        /// <summary>
        /// Saves the given object as a binary file at the given path. The extension can be anything but must be included as part of the
        /// path parameter. This methods throws a FracturedStateException that wraps any IO exception that occurs. This was done because this method
        /// is considered a save operation so the application does not necessarily need to exit due to a failure.
        /// </summary>
        /// <typeparam name="T">The type of the object being saved</typeparam>
        /// <param name="obj">The object being saved</param>
        /// <param name="filePath">The absolute location of the resulting file without a trailing slash</param>
        /// <param name="fileName">The name of the resulting file including the extension</param>
        public static void SerializeBinary<T>(T obj, string filePath, string fileName)
        {
            try
            {
                if (!Directory.Exists(filePath))
                    Directory.CreateDirectory(filePath);

                using (var file = new FileStream(filePath + "/" + fileName, FileMode.Create))
                {
                    var bFormat = new BinaryFormatter();
                    bFormat.Serialize(file, obj);
                }
            }
            catch (Exception e)
            {
                throw new FracturedStateException("Cannot serialize objectof Type " + typeof(T), e);
            }
        }

        public static string LoadFileContents(string filePath)
        {
            string contents;
            using (var file = new StreamReader(filePath))
            {
                contents = file.ReadToEnd();
            }
            return contents;
        }

        public static T LoadCustomBehaviour<T>(string className) where T : class
        {
            var qName = "FracturedState.Game.AI." + className;
            var type = Type.GetType(qName);
            if (type == null) return null;
            var behaviour = Activator.CreateInstance(type) as T;
            return behaviour;
        }

        private static string[] GetMapFileList()
        {
            return Directory.GetDirectories(DataLocationConstants.GameRootPath + DataLocationConstants.MapDirectory);
        }

        public static string[] GetMapFileListForDisplay()
        {
            var mapFiles = GetMapFileList();
            var mapFileList = new string[mapFiles.Length];
            for (var i = 0; i < mapFiles.Length; i++)
            {
                mapFiles[i] = mapFiles[i].Replace(@"\", "/");
                mapFileList[i] = mapFiles[i].Substring(mapFiles[i].LastIndexOf('/') + 1);
            }
            return mapFileList;
        }

        public static void GetMapFileDetails(out int[] playerCount, out int[] hashCode)
        {
            var mapFiles = GetMapFileList();
            playerCount = new int[mapFiles.Length];
            hashCode = new int[mapFiles.Length];
            for (var i = 0; i < mapFiles.Length; i++)
            {
                var data = DeserializeXml<RawMapData>(mapFiles[i] + "/map.xml");
                playerCount[i] = data.StartingPoints.Length;
                hashCode[i] = data.GetHashCode();
            }
        }

        public static GameObject LoadBuiltInRagdoll(string name)
        {
            return (GameObject)(Object.Instantiate(Resources.Load(DataLocationConstants.BuiltInRagdollDirectory + name, typeof(GameObject))));
        }

        public static GameObject LoadBuiltInParticleSystem(string system)
        {
            var go = (GameObject)(Object.Instantiate(Resources.Load(DataLocationConstants.BuiltInParticleDirectory + system, typeof(GameObject))));
            go.name = system.Replace('/', '_');
            return go;
        }

        /// <summary>
        /// Loads a built-in sound effect from the Resources folder in the form of an AudioClip
        /// </summary>
        public static AudioClip LoadBuiltInSound(string soundPath)
        {
            return (AudioClip)(Resources.Load(DataLocationConstants.BuiltInSFXDirectory + soundPath, typeof(AudioClip)));
        }

        public static AudioClip LoadBuiltInBark(string soundPath)
        {
            return (AudioClip)(Resources.Load(DataLocationConstants.BuiltInBarkDirectory + soundPath, typeof(AudioClip)));
        }

        public static Sprite[] LoadBuiltInUiSpritesFromAtlas(string atlasName)
        {
            return Resources.LoadAll<Sprite>(DataLocationConstants.BuiltInTextureDirectory +
                                             DataLocationConstants.BuiltInUITexDirectory + atlasName);
        }

		/// <summary>
		/// Loads a texture out of the given Resources directory
		/// </summary>
		public static Texture2D LoadTexture(string textureName)
		{
		    return Resources.Load<Texture2D>(DataLocationConstants.UnitIconDirectory + textureName);
		}
		
        public static GameObject LoadPrefab(string prefabName)
        {
            var go = (GameObject)(Object.Instantiate(Resources.Load(prefabName, typeof(GameObject))));
            go.name = prefabName.Contains('/') ? prefabName.Substring(prefabName.LastIndexOf('/') + 1) : prefabName;
            return go;
        }

		public static GameObject LoadBuiltInModel(string modelFile)
		{
            var go = (GameObject)(Object.Instantiate(Resources.Load(DataLocationConstants.BuiltInModelDirectory + modelFile, typeof(GameObject))));
            go.name = modelFile.Contains('/') ? modelFile.Substring(modelFile.LastIndexOf('/') + 1) : modelFile;
            return go;
		}

        public static GameObject LoadGroundIndicator(string name)
        {
            var ind = Object.Instantiate(Resources.Load<GameObject>(DataLocationConstants.BuiltInIndicatorDirectory + name));
            ind.SetLayerRecursively(GameConstants.ExteriorLayer);
            return ind;
        }

        /// <summary>
        ///  Processes a RawMapData object and creates the GameObjects required to represent the map.  This method instantiates everything into the world as active. 
        /// </summary>
        public static IEnumerator LoadMap(RawMapData mapData)
        {
            float totalLoadCount = mapData.Terrains.Length + mapData.Props.Length + mapData.Structures.Length;
            var loadedCount = 0;
            const int messageInterval = 5;

            var worldParent = GameObject.Find("WorldParent");
            var terrainParent = worldParent.transform.Find("TerrainParent");
            var propParent = worldParent.transform.Find("PropParent");
            var structureParent = worldParent.transform.Find("StructureParent");

            var terrainList = DeserializeXml<TerrainList>(DataLocationConstants.GameRootPath + DataLocationConstants.TerrainDataFile);

            GameObject[] terrains = null;
            if (mapData.Terrains != null)
            {
                terrains = new GameObject[mapData.Terrains.Length];
                for (var t = 0; t < mapData.Terrains.Length; t++)
                {
                    var terrainEntry = terrainList.Entries.FirstOrDefault(tr => tr.Id == int.Parse(mapData.Terrains[t].BaseObjectName));
                    if (terrainEntry != null)
                    {
                        var terrain = LoadMapObject(terrainEntry.ModelName, mapData.Terrains[t].PositionString, mapData.Terrains[t].RotationString);
                        terrain.transform.parent = terrainParent;
                        // set unique ID - not typically used for terrain, but you never know
                        if (mapData.Terrains[t].UID > 0)
                        {
                            terrain.AddComponent<Identity>().SetUID(mapData.Terrains[t].UID);
                        }
                        // territory assignments
                        if (mapData.Territories.Length > 0 && mapData.Terrains[t].TerritoryID >= 0)
                        {
                            TerritoryManager.Instance.AddTerrainAssignment(terrain, mapData.Territories[mapData.Terrains[t].TerritoryID]);
                        }
                        terrains[t] = terrain;
                        loadedCount++;
                        if (loadedCount % messageInterval == 0)
                        {
                            FracNet.Instance.NetworkActions.CmdUpdateMapProgress((loadedCount / totalLoadCount) * 0.5f);
                        }
                        yield return null;
                    }
                    else
                    {
                        throw new FracturedStateException("Bad terrain reference: \"" + mapData.Terrains[t].BaseObjectName + "\"");
                    }
                }

                TerritoryManager.Instance.FinalizeTerritories();
            }

            // structures need a parent object so we load exterior and interior art without location/rotation
            // and then parent them to the container at world origin and move/rotate as a whole
            var c = Object.FindObjectOfType<ShroudCamera>();
            var shroudMask = c.ShroudMaterial;
            var shroudColor = c.ShroudColor;
            var structList = new List<StructureManager>();
            GameObject[] structures = null;
            if (mapData.Structures != null)
            {
                structures = new GameObject[mapData.Structures.Length];
                var structDefaults = structureParent.GetComponent<GlobalStructureDefaults>();
                for (var b = 0; b < mapData.Structures.Length; b++)
                {
                    var data = XmlCacheManager.Structures[mapData.Structures[b].BaseObjectName];
                    if (data != null)
                    {
                        try
                        {
                            var s = LoadInteriorExteriorMapObject(data.Name, mapData.Structures[b].PositionString, mapData.Structures[b].RotationString,
                                data.Model.ExteriorModel, data.Model.InteriorModel);

                            s.transform.parent = structureParent;
                            if (data.IsEnterable)
                            {
                                var sm = s.AddComponent<StructureManager>();
                                sm.SetData(data);
                                if (data.CanBeCaptured)
                                {
                                    var cp = s.AddComponent<CaptureProgressUI>();
                                    cp.Init(structDefaults, sm);
                                    cp.enabled = false;
                                }
                                sm.SetShroudMask(shroudMask, shroudColor);
                                s.AddComponent<Identity>().SetUID(mapData.Structures[b].UID);
                                ObjectUIDLookUp.Instance.AddStructure(mapData.Structures[b].UID, sm);

                                var colliders = s.GetComponentsInChildren<Collider>();
                                sm.SetNavGrid(new NavGrid(colliders, data.FloorOffset));
                                sm.SetPlacementGrid(new NavGrid(colliders, data.FloorOffset, NavGrid.InteriorSpaceStep));
                                structList.Add(sm);
                            }
                            if (!string.IsNullOrEmpty(data.Bib))
                            {
                                var bib = LoadPrefab("Models/Bib/" + data.Bib);
                                var bibParent = new GameObject("Bibs");
                                bib.transform.parent = bibParent.transform;
                                bibParent.transform.position = s.transform.position;
                                bibParent.transform.rotation = s.transform.rotation;
                                bibParent.transform.parent = s.transform;
                                bibParent.SetLayerRecursively(GameConstants.TerrainLayer);
                            }
                            structures[b] = s;
                            
                        }
                        catch (FracturedStateException e)
                        {
                            throw new FracturedStateException("Unable to load structure data for: \"" + data.Name + "\"", e);
                        }
                        loadedCount++;
                        if (loadedCount % messageInterval == 0)
                        {
                            FracNet.Instance.NetworkActions.CmdUpdateMapProgress((loadedCount / totalLoadCount) * 0.5f);
                        }
                        yield return null;
                    }
                    else
                    {
                        throw new FracturedStateException("Bad structure reference: \"" + mapData.Structures[b].BaseObjectName + "\"");
                    }
                }
            }

            var exteriorCovers = new List<CoverManager>();
            GameObject[] props = null;
            if (mapData.Props != null)
            {
                props = new GameObject[mapData.Props.Length];
                for (var p = 0; p < mapData.Props.Length; p++)
                {
                    var prop = XmlCacheManager.Props[mapData.Props[p].BaseObjectName];
                    if (prop != null)
                    {
                        try
                        {
                            var prp = LoadInteriorExteriorMapObject(prop.Name, mapData.Props[p].PositionString, mapData.Props[p].RotationString,
                                prop.Model.ExteriorModel, prop.Model.InteriorModel);

                            CoverManager cm = null;

                            if (prop.ProvidesCover)
                            {
                                cm = prp.AddComponent<CoverManager>();
                                cm.Init(prop);
                            }

                            if (mapData.Props[p].UID > 0)
                            {
                                prp.AddComponent<Identity>().SetUID(mapData.Props[p].UID);
                                if (cm != null)
                                {
                                    ObjectUIDLookUp.Instance.AddCoverManager(mapData.Props[p].UID, cm);
                                }
                            }

                            if (prop.BoundsBox != null)
                            {
                                Vector3 center, bounds;
                                if (prop.BoundsBox.Center.TryVector3(out center) && prop.BoundsBox.Bounds.TryVector3(out bounds))
                                {
                                    var bc = prp.AddComponent<BoxCollider>();
                                    bc.center = center;
                                    bc.size = bounds;
                                }
                            }

                            prp.SetLayerRecursively(mapData.Props[p].Layer);
                            prp.transform.parent = propParent;
                            prp.tag = GameConstants.PropTag;
                            if (mapData.Props[p].Layer == GameConstants.InteriorLayer)
                            {
                                prp.SetActive(false);
                                var s = RaycastUtil.RaycastExterior(prp.transform.position + Vector3.up * 100);
                                if (s.transform != null)
                                {
                                    var structure = s.transform.GetAbsoluteParent().GetComponent<StructureManager>();
                                    if (structure != null)
                                    {
                                        structure.AddProp(prp, cm);
                                    }
                                }
                            }
                            else if (mapData.Props[p].Layer == GameConstants.ExteriorLayer && cm != null)
                            {
                                exteriorCovers.Add(cm);
                            }
                            props[p] = prp;
                        }
                        catch (FracturedStateException e)
                        {
                            throw new FracturedStateException("Unable to load prop data for: \"" + prop.Name + "\"", e);
                        }
                        loadedCount++;
                        if (loadedCount % messageInterval == 0)
                        {
                            FracNet.Instance.NetworkActions.CmdUpdateMapProgress((loadedCount / totalLoadCount) * 0.5f);
                        }
                        yield return null;
                    }
                    else
                    {
                        throw new FracturedStateException("Bad prop reference: \"" + mapData.Props[p].BaseObjectName + "\"");
                    }
                }
            }

            if (mapData.Decals != null)
            {
                foreach (var mapDecal in mapData.Decals)
                {
                    var d = Resources.Load<GameObject>($"{DataLocationConstants.BuiltInDecalDirecotry}{mapDecal.Name}");
                    var decal = Object.Instantiate(d).GetComponent<Decal>();
                    if (decal == null) continue;

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
                        decal.LimitTo = terrainParent.gameObject;
                    }
                    else if (mapDecal.StructureId > 0)
                    {
                        var s = ObjectUIDLookUp.Instance.GetStructureManager(mapDecal.StructureId);
                        if (s != null)
                        {
                            foreach (Transform child in s.transform)
                            {
                                if (mapDecal.LimitExterior && child.gameObject.layer == GameConstants.ExteriorLayer ||
                                    !mapDecal.LimitExterior && child.gameObject.layer == GameConstants.InteriorLayer)
                                {
                                    decal.LimitTo = child.gameObject;
                                    break;
                                }
                            }
                            s.AddDecal(decal);
                        }
                    }
                    
                    decal.gameObject.SetLayerRecursively(GameConstants.ExteriorLayer);
                }
            }

            AStarPather.Instance.GenerateExteriorGrid(mapData);

            foreach (var structure in structList)
            {
                structure.CheckFirePoints();
            }
            foreach (var cover in exteriorCovers)
            {
                cover.CheckExteriorPoints();
            }
            
            // setup reflection probes and queue rendering
            SetupReflectionProbe(terrains, false);
            SetupReflectionProbe(structures, true);
            SetupReflectionProbe(props, true);

            Loader.Instance.StartCoroutine(Loader.Instance.RenderProbes(terrains, structures, props));
        }

        private static void SetupReflectionProbe(GameObject[] objects, bool moveProbes)
        {
            if (objects == null) return;
            
            var quality = (int)ProfileManager.GetActiveProfile().GameSettings.ReflectionQuality;
            foreach (var obj in objects)
            {
                if (obj == null) continue;
                
                var probes = obj.GetComponentsInChildren<ReflectionProbe>(true);
                if (probes == null) continue;
                
                foreach (var probe in probes)
                {
                    if (quality == 0 || !probe.enabled)
                    {
                        Object.Destroy(probe);
                    }
                    else
                    {
                        probe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
                        probe.timeSlicingMode = ReflectionProbeTimeSlicingMode.AllFacesAtOnce;
                        probe.cullingMask = GameConstants.ReflectionMask;
                        probe.resolution = quality;
                        if (moveProbes)
                        {
                            probe.center = obj.transform.rotation * probe.center;
                            probe.size = obj.transform.rotation * probe.size;
                        }
                        else
                        {
                            var q = obj.transform.eulerAngles;
                            q.x = 0;
                            probe.center = Quaternion.Euler(q) * probe.center;
                            probe.size = Quaternion.Euler(q) * probe.size;
                        }
                    }
                }
            }
        }

        private static GameObject LoadMapObject(string name, Vec3String posString, Vec3String rotString)
        {
            Vector3 pos, rot;
            if (posString.TryVector3(out pos) && rotString.TryVector3(out rot))
            {
                GameObject mapObj;
                if (!string.IsNullOrEmpty(name))
                {
                    mapObj = LoadBuiltInModel(name);
                    if (mapObj == null)
                    {
                        throw new FracturedStateException("Unable to load map.  \"" + name + "\" is not a valid reference.");
                    }
                }
                else
                {
                    mapObj = new GameObject(name);
                }
                mapObj.transform.position = pos;
                mapObj.transform.rotation = Quaternion.Euler(rot);
                return mapObj;
            }
            else
            {
                throw new FracturedStateException("Unable to parse map data.");
            }
        }

        private static GameObject LoadInteriorExteriorMapObject(string name, Vec3String position, Vec3String rotation, string exteriorModel, string interiorModel)
        {
            Vector3 pos, rot;
            if (position.TryVector3(out pos) && rotation.TryVector3(out rot))
            {
                pos.y += GameConstants.ObjectYAdjustment;

                var container = new GameObject(name);

                if (!string.IsNullOrEmpty(exteriorModel))
                {
                    var ext = LoadMapObject(exteriorModel);
                    ext.SetLayerRecursively(GameConstants.ExteriorLayer);
                    ext.transform.parent = container.transform;
                }

                if (!string.IsNullOrEmpty(interiorModel))
                {
                    var intr = LoadMapObject(interiorModel);
                    intr.SetLayerRecursively(GameConstants.InteriorLayer);
                    intr.transform.parent = container.transform;
                }

                container.transform.position = pos;
                container.transform.rotation = Quaternion.Euler(rot);

                return container;
            }
            else
            {
                throw new FracturedStateException("Error parsing position and rotation data.");
            }
        }

        private static GameObject LoadMapObject(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                var mapObj = LoadBuiltInModel(name);
                if (mapObj != null)
                    return mapObj;
            }

            return new GameObject(name);
        }
    }
}