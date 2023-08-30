using System;
using System.Collections.Generic;
using System.Reflection;

namespace FracturedState.Game.Modules
{
    public static class LocomotorFactory
    {
        public const string DefaultLocomotor = "DefaultUnitLocomotor";
        
        private static readonly Dictionary<string, ConstructorInfo> ConstructorInfos = new Dictionary<string, ConstructorInfo>();
        
        public static Locomotor Create(string name, UnitManager owner)
        {
            if (string.IsNullOrEmpty(name)) name = DefaultLocomotor;
            
            ConstructorInfo constructor;
            if (!ConstructorInfos.TryGetValue(name, out constructor))
            {
                var qName = "FracturedState.Game.Modules." + name;
                var type = Type.GetType(qName);
                if (type == null) throw new FracturedStateException(name + " is not a valid Locomotor");
                constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new Type[] {typeof(UnitManager)}, null);
                if (constructor == null) throw new FracturedStateException(name + " does not declare a valid Locomotor constructor");
                ConstructorInfos[name] = constructor;
            }
            
            return constructor.Invoke(new object[] {owner}) as Locomotor;
        }
    }
}