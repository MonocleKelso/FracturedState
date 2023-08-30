using UnityEngine;

namespace FracturedState.Game
{
    public enum CameraViewState { Interior, Exterior }

    public static class ExtensionMethods
    {
		/// <summary>
		/// Event hook into Unity error handling.  Displays an error message and stack trace in a GUI window.  While not a true extension method, this seemed like the
		/// best place to put this.
		/// </summary>
		public static void HandleError(string logString, string stackTrace, LogType type)
		{
            if (!ErrorHandler.HasError)
            {
                if ((type == LogType.Exception || type == LogType.Assert))
                {
                    GameObject errObj = new GameObject("ExceptionObject");
                    ErrorHandler handler = errObj.AddComponent<ErrorHandler>();
                    handler.SetError(logString, stackTrace);
                }
            }
        }

        /// <summary>
        /// Searches the given Transform for a Transform with the given name. This starts with direct ancestors and then does a depth-first search of each child
        /// </summary>
        /// <returns></returns>
        public static Transform GetChildByName(this Transform transform, string name)
        {
            Transform t = null;
            int childCount = transform.childCount;
            for (var i = 0; i < childCount; i++)
            {
                if (transform.GetChild(i).name == name)
                {
                    t = transform.GetChild(i);
                }
            }
            if (t == null)
            {
                for (var i = 0; i < childCount; i++)
                {
                    t = transform.GetChild(i).GetChildByName(name);
                    if (t != null)
                        break;
                }
            }
            return t;
        }

        /// <summary>
        /// Gets the topmost Transform in the hierarchy
        /// </summary>
        public static Transform GetAbsoluteParent(this Transform transform)
        {
            Transform curTran = transform;
            while (curTran.parent != null && curTran.parent.name != "WorldParent" && curTran.parent.name != "StructureParent" &&
                curTran.parent.name != "PropParent" && curTran.parent.name != "Props" && curTran.parent.name != "Structures" && curTran.parent.name != "Terrain" &&
                curTran.parent.name != "UnitParent")
            {
                curTran = curTran.parent;
            }
            return curTran;
        }
	
        /// <summary>
        /// Recursively sets the layer for this GameObject and all child GameObjects
        /// </summary>
        public static void SetLayerRecursively(this GameObject gameObject, int layer)
        {
            gameObject.layer = layer;
            foreach (Transform t in gameObject.transform)
            {
                t.gameObject.SetLayerRecursively(layer);
            }
        }

        /// <summary>
        /// Recursively sets the layer for this GameObject and all child GameObjects except for any children with the given name
        /// </summary>
        public static void SetLayerRecursively(this GameObject gameObject, int layer, string exclude)
        {
            gameObject.layer = layer;
            foreach (Transform t in gameObject.transform)
            {
                if (t.name != exclude)
                {
                    t.gameObject.SetLayerRecursively(layer, exclude);
                }
            }
        }

        public static void SetTagRecursively(this GameObject gameObject, string tag)
        {
            gameObject.tag = tag;
            foreach (Transform t in gameObject.transform)
            {
                t.gameObject.tag = tag;
            }
        }

        /// <summary>
        /// Inserts spaces between capital letters in a pascal case string so PascalCase becomes Pascal Case
        /// Regex from: http://stackoverflow.com/questions/155303/net-how-can-you-split-a-caps-delimited-string-into-an-array
        /// </summary>
        public static string PrettyPascal(this string orig)
        {
            return System.Text.RegularExpressions.Regex.Replace(orig, "([a-z](?=[A-Z]|[0-9])|[A-Z](?=[A-Z][a-z]|[0-9])|[0-9](?=[^0-9]))", "$1 ");
        }
    }
}