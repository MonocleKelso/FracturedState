using UnityEngine;
using FracturedState.Game.Data;
using System.Collections.Generic;

namespace FracturedState.Game
{
	public sealed class ObjectPool
	{
        private static ObjectPool instance;
        public static ObjectPool Instance => instance ?? (instance = new ObjectPool());

	    private List<GameObject> selectionProjectors;
        private Dictionary<string, List<GameObject>> lookup;
        private List<GameObject> coverHelpers;
        private List<GameObject> firePointHelpers;
        private List<GameObject> damageHelpers;
        private List<GameObject> healHelpers;
        private List<GameObject> buffHelpers;
        private GameObject pool;

        private GameObject worldHelpers;

        private ObjectPool()
        {
            pool = new GameObject("ObjectPool");
            worldHelpers = GameObject.Find("WorldHelpers");
            selectionProjectors = new List<GameObject>();
            lookup = new Dictionary<string, List<GameObject>>();
            coverHelpers = new List<GameObject>();
            firePointHelpers = new List<GameObject>();
            damageHelpers = new List<GameObject>();
            healHelpers = new List<GameObject>();
            buffHelpers = new List<GameObject>();
        }

        public GameObject GetCoverHelper(Vector3 position)
        {
            return GetHelperPrefab(coverHelpers, DataLocationConstants.CoverHelperIconPrefab, position);
        }

        public GameObject GetFirePointHelper(Vector3 position)
        {
            return GetHelperPrefab(firePointHelpers, DataLocationConstants.FirePointHelperIconPrefab, position);
        }

        public GameObject GetDamageHelper(Vector3 position)
        {
            return GetHelperPrefab(damageHelpers, DataLocationConstants.DamageHelperPrefab, position);
        }

        public GameObject GetHealHelper(Vector3 position)
        {
            return GetHelperPrefab(healHelpers, DataLocationConstants.HealHelperPrefab, position);
        }

        public GameObject GetBuffHelper(Vector3 position)
        {
            return GetHelperPrefab(buffHelpers, DataLocationConstants.BuffHelperPrefab, position);
        }

        private GameObject GetHelperPrefab(List<GameObject> searchList, string prefabName, Vector3 location)
        {
            GameObject helper;
            if (searchList.Count > 0)
            {
                helper = searchList[0];
                searchList.RemoveAt(0);
                helper.transform.SetParent(null, false);
                helper.SetActive(true);
                helper.transform.position = location;
                return helper;
            }
            helper = DataUtil.LoadPrefab(prefabName);
            helper.transform.position = location;
            return helper;
        }

        public void ReturnCoverHelper(GameObject helper)
        {
            ReturnHelper(helper, coverHelpers);
        }

        public void ReturnFirePointHelper(GameObject helper)
        {
            ReturnHelper(helper, firePointHelpers);
        }

        public void ReturnDamageHelper(GameObject helper)
        {
            ReturnHelper(helper, damageHelpers);
        }

        public void ReturnHealHelper(GameObject helper)
        {
            ReturnHelper(helper, healHelpers);
        }

        public void ReturnBuffHelper(GameObject helper)
        {
            ReturnHelper(helper, buffHelpers);
        }

        void ReturnHelper(GameObject helper, List<GameObject> list)
        {
            helper.SetActive(false);
            helper.transform.SetParent(worldHelpers.transform, false);
            list.Add(helper);
        }

        public GameObject GetPooledObjectAtLocation(string objName, Vector3 location)
        {
            List<GameObject> objList;
            GameObject obj;
            if (lookup.TryGetValue(objName, out objList))
            {
                if (objList.Count > 0)
                {
                    obj = objList[0];
                    objList.RemoveAt(0);
                    obj.transform.parent = null;
                    obj.transform.position = location;
                    obj.SetActive(true);
                    return obj;
                }
            }
            obj = LoadPoolObject(objName);
            obj.SetActive(false);
            obj.transform.position = location;
            obj.SetActive(true);
            return obj;
        }

        public GameObject GetPooledObject(string objName)
        {
            List<GameObject> objList;
            if (lookup.TryGetValue(objName.Substring(objName.LastIndexOf('/') + 1), out objList))
            {
                if (objList.Count > 0)
                {
                    GameObject obj = objList[0];
                    objList.RemoveAt(0);
                    obj.SetActive(true);
                    obj.transform.parent = null;
                    return obj;
                }
            }
            return LoadPoolObject(objName);
        }

        public GameObject GetPooledModel(string modelName)
        {
            List<GameObject> objList;
            if (lookup.TryGetValue(modelName.Substring(modelName.LastIndexOf('/') + 1), out objList))
            {
                if (objList.Count > 0)
                {
                    var model = objList[0];
                    var pb = model.GetComponent<ProjectileBehaviour>();
                    if (pb == null || pb.Primed)
                    {
                        objList.RemoveAt(0);
                        model.SetActive(true);
                        model.transform.parent = null;
                        return model;
                    }
                }
            }
            return DataUtil.LoadBuiltInModel(modelName);
        }

        public GameObject GetPooledModelAtLocation(string modelName, Vector3 location)
        {
            List<GameObject> objList;
            GameObject model;
            if (lookup.TryGetValue(modelName.Substring(modelName.LastIndexOf('/') + 1), out objList))
            {
                if (objList.Count > 0)
                {
                    model = objList[0];
                    var pb = model.GetComponent<ProjectileBehaviour>();
                    if (pb == null || pb.Primed)
                    {
                        objList.RemoveAt(0);
                        model.transform.parent = null;
                        model.transform.position = location;
                        model.SetActive(true);
                        return model;
                    }
                }
            }
            model =  DataUtil.LoadBuiltInModel(modelName);
            model.SetActive(false);
            model.transform.position = location;
            model.SetActive(true);
            return model;
        }

        public void ReturnPooledObject(GameObject obj)
        {
            obj.transform.position = Vector3.zero;
            obj.transform.parent = pool.transform;
            obj.SetActive(false);
            List<GameObject> objList;
            if (lookup.TryGetValue(obj.name, out objList))
            {
                objList.Add(obj);
            }
            else
            {
                objList = new List<GameObject> {obj};
                lookup[obj.name] = objList;
            }
        }

        private GameObject LoadPoolObject(string objName)
        {
            return DataUtil.LoadPrefab(objName);
        }

        public void InitSelectionPool()
        {
            // if init has been called before then flush pool and re-populate
            if (selectionProjectors != null || selectionProjectors?.Count > 0)
            {
                foreach (var g in selectionProjectors)
                {
                    Object.DestroyImmediate(g);
                }
                Resources.UnloadUnusedAssets();
                selectionProjectors.Clear();
            }

            selectionProjectors = new List<GameObject>();
            for (var i = 0; i < ConfigSettings.Instance.Values.InitialSelectionProjectorCount; i++)
            {
                var proj = Object.Instantiate(PrefabManager.SelectionProjectorPrefab);
                proj.SetActive(false);
                proj.transform.parent = pool.transform;
                selectionProjectors.Add(proj);
            }
        }

        public GameObject GetSelectionProjector()
        {
            if (selectionProjectors.Count == 0) 
                return Object.Instantiate(PrefabManager.SelectionProjectorPrefab);
            
            var proj = selectionProjectors[0];
            selectionProjectors.RemoveAt(0);
            proj.transform.parent = null;
            return proj;

        }

        public void ReturnSelectionProjector(GameObject projector)
        {
            projector.SetActive(false);
            projector.GetComponent<SelectionProjectorFollow>().SetTarget(null);
            projector.transform.position = Vector3.zero;
            projector.transform.parent = pool.transform;
            selectionProjectors.Add(projector);
        }
	}
}