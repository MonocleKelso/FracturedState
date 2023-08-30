using System;
using System.Reflection;
using FracturedState.Game.Data;

namespace FracturedState.Game
{
    public class ModdableXmlCache<TKey, TValue> : ModdableCache<TKey, TValue> where TValue : class
    {
        /// <summary>
        /// Loads a directory of files and deserializes each one into an object for the cache.
        /// </summary>
        /// <param name="files">The array of file paths to load</param>
        /// <param name="keyName">The name of the property to use as a lookup key in the cache</param>
        public void LoadFromDirectory(string[] files, string keyName)
        {
            for (var i = 0; i < files.Length; i++)
            {
                TValue data = DataUtil.DeserializeXml<TValue>(files[i]);
                PropertyInfo keyProp = typeof(TValue).GetProperty(keyName);
                if (keyProp != null)
                {
                    TKey key = (TKey)keyProp.GetValue(data, null);
                    cache[key] = data;
                }
                else
                {
                    throw new FracturedStateException("Unable to determine property info for key: " + keyName);
                }
            }
        }

        /// <summary>
        /// Loads a single XML file and deserializes the list of objects contained within it.
        /// </summary>
        /// <typeparam name="T">The Type of the container list object</typeparam>
        /// <param name="filePath">The absolute path to the XML file</param>
        /// <param name="keyName">The name of the property to use as a lookup key in the cache</param>
        /// <param name="listName">The name of property representing the list of objects</param>
        public void LoadFromSingleFile<T>(string filePath, string keyName, string listName)
        {
            T data = DataUtil.DeserializeXml<T>(filePath);
            PropertyInfo listProp = typeof(T).GetProperty(listName);
            if (listProp != null)
            {
                TValue[] items = listProp.GetValue(data, null) as TValue[];
                if (items != null && items.Length > 0)
                {
                    Type itemType = items[0].GetType();
                    PropertyInfo itemProp = itemType.GetProperty(keyName);
                    if (itemProp != null)
                    {
                        for (var i = 0; i < items.Length; i++)
                        {
                            TKey key = (TKey)itemProp.GetValue(items[i], null);
                            cache[key] = items[i];
                        }
                    }
                    else
                    {
                        throw new FracturedStateException("Unable to determine property info for key: " + keyName);
                    }
                }
            }
            else
            {
                throw new FracturedStateException("Unable to serialize XML list with name: " + listName);
            }
        }
    }
}