using System;
using UnityEditor;
using UnityEngine;

namespace Code.Editor
{
    public class CreateTerrain
    {
        private const string TilePath = "Assets/Resources/Models/Terrain/";

        private const string PrefabPath = "Assets/Built-In/Prefabs/Tiles/";

        private static readonly string[] Prefabs = {
            "NoEdge.prefab", "OneEdge.prefab", "TwoEdge.prefab", "Corner.prefab", "TwoEdgePipe.prefab",
            "OneEdgePipe.prefab"
        };
        

        
        [MenuItem("Assets/Create Terrain Tiles")]
        private static void CreateTiles()
        {
            var material = Selection.activeObject as Material;
            if (material == null) return;
            var path = AssetDatabase.GetAssetPath(material);
            path = path.Substring(path.LastIndexOf("/", StringComparison.Ordinal) + 1);
            path = path.Substring(0, path.Length - 4);
            foreach (var p in Prefabs)
            {
                var src = $"{PrefabPath}{p}";
                var dest = $"{TilePath}{p}";
                if (AssetDatabase.CopyAsset(src, dest))
                {
                    var tile = AssetDatabase.LoadAssetAtPath(dest, typeof(GameObject)) as GameObject;
                    tile.GetComponent<MeshRenderer>().material = material;
                }
            }
            
        }

        [MenuItem("Assets/Create Terrain Tiles", true)]
        private static bool ValidateCreateTiles()
        {
            return Selection.activeObject != null && Selection.activeObject.GetType() == typeof(Material);
        }
    }
}