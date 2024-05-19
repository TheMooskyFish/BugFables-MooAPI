using System.Collections.Generic;
using BepInEx.Logging;
using Mono.Cecil;
using Logger = BepInEx.Logging.Logger;

namespace MooAPI_Patcher
{
    public static class Patcher
    {
        public static IEnumerable<string> TargetDLLs { get; } = ["Assembly-CSharp.dll"];
        internal static ManualLogSource Log = Logger.CreateLogSource("MooAPI Patcher");
        public static void Patch(AssemblyDefinition assembly)
        {
            MapsPatch.Patch(assembly);
        }
    }
}
