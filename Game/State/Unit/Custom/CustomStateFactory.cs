using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace FracturedState.Game.AI
{
    public static class CustomStateFactory<T> where T : ICustomStatePackage
    {
        const string ns = "FracturedState.Game.AI.";

        static Dictionary<string, Func<T, CustomState>> callCache = new Dictionary<string, Func<T, CustomState>>();

        public static CustomState Create(string typeName, T package)
        {
            Func<T, CustomState> caller;
            if (!callCache.TryGetValue(typeName, out caller))
            {
                var type = Type.GetType(ns + typeName);
                var ctor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, CallingConventions.HasThis, new[] { typeof(T) }, new ParameterModifier[0]);
                var args = new[] { Expression.Parameter(typeof(T), "param1") };
                var ctorExp = Expression.New(ctor, args);
                caller = Expression.Lambda<Func<T, CustomState>>(ctorExp, args).Compile();
                callCache[typeName] = caller;
            }
            return caller(package);
        }
    }
}