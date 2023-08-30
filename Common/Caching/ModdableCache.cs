using System.Collections.Generic;
using System.Linq;
using FracturedState.Game.Data;

namespace FracturedState.Game
{
    public class ModdableCache<TKey, TValue> where TValue : class
    {
        protected Dictionary<TKey, TValue> cache;

        public ModdableCache()
        {
            cache = new Dictionary<TKey, TValue>();
        }

        public TValue this[TKey key]
        {
            get
            {
                TValue val = null;
                if (cache.TryGetValue(key, out val))
                    return val;

                return null;
            }
            set
            {
                cache[key] = value;
            }
        }

        public TValue[] Values
        {
            get
            {
                return cache.Values.ToArray<TValue>();
            }
        }
    }
}