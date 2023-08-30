using FracturedState.Game;
using FracturedState.Game.Data;
using System.Collections.Generic;

namespace FracturedState
{
    public static class LocalizedString
    {
        static Dictionary<string, string> localStrings;

        public static void Init()
        {
            localStrings = new Dictionary<string, string>();
            try
            {
                string content = DataUtil.LoadFileContents(DataLocationConstants.GameRootPath + "strings_en.txt");
                var lines = content.Split('\n');
                foreach (var line in lines)
                {
                    var data = line.Split('=');
                    localStrings.Add(data[0], data[1]);
                }
            }
            catch (System.Exception e)
            {
                throw new FracturedStateException("Error loading international string data", e);
            }
        }

        /// <summary>
        /// Returns the localized string associated with the given key.
        /// If no value is associated with the given key then returns the key.
        /// </summary>
        public static string GetString(string key)
        {
#if UNITY_EDITOR
            if (localStrings == null)
            {
                Init();
            }
#endif
            string value;
            if (localStrings.TryGetValue(key, out value))
            {
                return value;
            }
            return key;
        }
    }
}