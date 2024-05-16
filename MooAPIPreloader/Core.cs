using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using Mono.Cecil;
using Logger = BepInEx.Logging.Logger;
public static class MooAPI_Patcher
{
    public static IEnumerable<string> TargetDLLs { get; } = ["Assembly-CSharp.dll"];
    internal static ManualLogSource Log = Logger.CreateLogSource("MooAPI Patcher");
    internal static readonly string[] Maps = ["NewMap", "c1a0"]; //TEMP - NEED TO FIND A WAY TO AUTO ADD MAPS
    public static void Patch(AssemblyDefinition assembly)
    {
        var maps = assembly.MainModule.GetType("MainManager/Maps");
        var battlemaps = assembly.MainModule.GetType("MainManager/BattleMaps");
        Log.LogInfo($"MAPS: {maps.Fields.Count}");
        Log.LogInfo($"BATTLEMAPS: {battlemaps.Fields.Count}");
        //foreach (var map in Maps)
        //{
        //    maps.Fields.Add(new FieldDefinition(map, FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.Public | FieldAttributes.HasDefault, maps) { Constant = maps.Fields.Count - 1 });
        //    Log.LogInfo($"Adding {map}");
        //}
        //assembly.Write($"{Paths.PatcherPluginPath}/PATCHED.dll");
    }
}
