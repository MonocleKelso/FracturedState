using System;
using FracturedState.Game;

namespace FracturedState.Scripting
{
    public static class ScriptManager
    {
        private const string ScriptNamespace = "FracturedState.Scripting.";

        public static Type ResolveScriptType(string scriptName)
        {
            var t = Type.GetType(ScriptNamespace + scriptName);
            if (t == null)
            {
                throw new FracturedStateException(scriptName + " is not a valid script");
            }
            return t;
        }

        public static IFracAbility CreateAbilityScriptInstance(string scriptName, params object[] args)
        {
            var t = ResolveScriptType(scriptName);
            var a = Activator.CreateInstance(t, args) as IFracAbility;
            if (a == null)
            {
                throw new FracturedStateException(scriptName + " must implement the IFracAbility interface");
            }
            return a;
        }
    }
}